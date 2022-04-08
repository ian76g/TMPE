namespace TrafficManager.Manager.Impl {
    using CSUtil.Commons;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.API.Manager;
    using TrafficManager.Lifecycle;
    using TrafficManager.UI;

    /// <summary>
    /// <para>The overlay manager is controls display of both interactive and
    /// persistent world overlays.</para>
    /// <para>Overlays can be things such as icons, outlines, and text.</para>
    /// </summary>
    /// <remarks>
    /// A key goal is to make it much easier for TM:PE and other mods
    /// to toggle persistent overlays on/off. For example, when using
    /// bulldozer tool (vanilla, or Move It, etc.) the <c>Common</c>
    /// overlays could be enabled so user is aware of TM:PE customisations
    /// while performing actions with those tools.
    /// </remarks>
    public class OverlayManager
        : AbstractGeometryObservingManager, IOverlayManager
    {

        public OverlayManager() {
            individualOverlays_ = IndividualOverlays.ToArray();
        }

        // TODO: Move enums to API once stable

        [Flags]
        public enum Overlays {
            None = 0,

            /// <summary>
            /// Priority Signs at junctions.
            /// </summary>
            PrioritySigns = 1 << 0,

            /// <summary>
            /// Traffic Lights at junctions.
            /// </summary>
            TrafficLights = 1 << 1,

            /// <summary>
            /// Speed Limits on applicable segments/lanes.
            /// </summary>
            SpeedLimits = 1 << 2,

            /// <summary>
            /// Vehicle restrictions on applicalbe segment lanes.
            /// </summary>
            VehicleRestrictions = 1 << 3,

            /// <summary>
            /// Parking Restrictions on applicable segment lanes.
            /// </summary>
            ParkingRestrictions = 1 << 4,

            /// <summary>
            /// Parking Space props in buildings.
            /// </summary>
            ParkingSpaces = 1 << 5, // future

            /// <summary>
            /// Junction Restrictions on applicalbe segment ends.
            /// </summary>
            JunctionRestrictions = 1 << 6,

            /// <summary>
            /// Lane Connectors 
            /// </summary>
            LaneConnectors = 1 << 7,

            /// <summary>
            /// <para>
            /// If used as persistent overlay, tunnels will be rendered
            /// in overground mode, allowing display and interaction of
            /// underground overlays.
            /// </para>
            /// <para>
            /// If used as interactive overlay, it will render tunnels
            /// in overground mode but they won't be interactive.
            /// </para>
            /// </summary>
            Tunnels = 1 << 8,

            /// <summary>
            /// Useful for external mods, so user can see what
            /// their actions might affect.
            /// </summary>
            Common =
                PrioritySigns | TrafficLights | SpeedLimits |
                VehicleRestrictions | ParkingRestrictions |
                JunctionRestrictions | LaneConnectors,

            /// <summary>
            /// Useful for bulk edit tools / mass applicators.
            /// </summary>
            Bulk =
                PrioritySigns | JunctionRestrictions |
                SpeedLimits | LaneConnectors,

            // Developer/niche overlays
            Nodes = 1 << 25,
            Lanes = 1 << 26,
            Vehicles = 1 << 27,
            PathUnits = 1 << 28, // future
            Citizens = 1 << 29,
            Buildings = 1 << 30,

            // TM:PE use only - special flag that denotes user choices in Overlays tab
            TMPE = 1 << 31,
        }

        public enum OverlayCulling {
            /// <summary>
            /// Uses <c>Camera</c> by default, but will switch to
            /// <c>Mouse</c> if there are too many render tasks.
            /// </summary>
            Automatic = 0, // not implemented yet

            /// <summary>
            /// Overlay display range is based on camera position.
            /// </summary>
            /// <remarks>Standard for TM:PE toolbar.</remarks>
            Camera = 1 << 0,

            /// <summary>
            /// Overlay display range based on mouse pointer position,
            /// but constrained to the camera viewport.
            /// </summary>
            /// <remarks>Useful for bulldozer tool, etc.</remarks>
            Mouse = 1 << 1,
        }

        internal ushort HoveredNodeId { get; set; }

        internal ushort HoveredSegmentId { get; set; }

        internal uint HoveredLaneId { get; set; }

        internal ushort[] SelectedNodeIds { get; set; }

        internal ushort[] SelectedSegmentIds { get; set; }

        internal uint[] SelectedLaneIds { get; set; }

        internal ushort SelectedBuildingId { get; set; }

        internal ushort SelectedVehicleId { get; set; }

        /// <summary>
        /// A list of the individual overlays, excluding any compound flags.
        /// </summary>
        public readonly List<Overlays> IndividualOverlays = new() {
            // TODO: find way to sync this list to Overlays enum
            Overlays.PrioritySigns,
            Overlays.TrafficLights,
            Overlays.SpeedLimits,
            Overlays.VehicleRestrictions,
            Overlays.ParkingRestrictions,
            Overlays.ParkingSpaces,
            Overlays.JunctionRestrictions,
            Overlays.LaneConnectors,
            Overlays.Nodes,
            Overlays.Lanes,
            Overlays.Vehicles,
            Overlays.PathUnits,
            Overlays.Citizens,
            Overlays.Buildings,
        };

        #region Private vars - do NOT access from outside

        /// <summary>
        /// Used for fast iteration of individual overlay flags.
        /// </summary>
        private readonly Overlays[] individualOverlays_;

        /// <summary>
        /// The current persistent overlays.
        /// </summary>
        private Overlays persistentOverlays_;

        /// <summary>
        /// The current interactive overlay.
        /// </summary>
        private Overlays interactiveOverlay_;

        /// <summary>
        /// Persistent overlays to include when Key overlay is interactive.
        /// </remarks>
        private Dictionary<Overlays, Overlays> overlayInclusions_;

        /// <summary>
        /// Persistent overlays to exclude when Key overlay is interactive.
        /// </remarks>
        private Dictionary<Overlays, Overlays> overlayExclusions_;

        /// <summary>
        /// Persistent overlays with includes and exclusions applied.
        /// </summary>
        private Overlays compiledPersistentOverlays_;

        #endregion

        /// <summary>
        /// Determine if exactly one overlay is specified in the supplied flags.
        /// </summary>
        /// <param name="overlay">
        /// The overlay flags to inspect.
        /// </param>
        /// <param name="silent">
        /// If <c>true</c>, error logging will be suppressed.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if only a single flag is set; otherwise <c>false</c>.
        /// </returns>
        public bool IsIndividualOverlay(Overlays overlay, bool silent = false) {
            if (!IndividualOverlays.Contains(overlay)) {
                if (!silent)
                    Log.Error($"Invalid overlay flags: {overlay}");

                return false;
            }
            return true;
        }

        /// <summary>
        /// A quick way to show common persistent overlays in
        /// range of the mouse pointer. Ideal for external mods.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        public bool TurnOn() =>
            TurnOn(Overlays.Common, OverlayCulling.Mouse);

        /// <summary>
        /// Turn on specified <paramref name="persistentOverlays"/>
        /// and optionally set overlay culling <paramref name="mode"/>.
        /// </summary>
        /// <param name="overlays">
        /// The overlays to make persistent.
        /// </param>
        /// <param name="mode">
        /// The overlay culling mode.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        public bool TurnOn(
            Overlays overlays,
            OverlayCulling mode = OverlayCulling.Mouse) =>
            TurnOn(persistent: overlays, culling: mode);

        /// <summary>
        /// Turns on persistent overlays and, optionally, activates
        /// the specified <paramref name="interactive"/>.
        /// </summary>
        /// <param name="culling">
        /// The overlay culling mode.
        /// </param>
        /// <param name="interactive">
        /// Optional: If specified, the specified overlay will be
        /// made interactive.
        /// </param>
        /// <param name="persistent">
        /// Optional: The overlays to make persistent (replaces any
        /// existing persistent overlays).
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        internal bool TurnOn(
            OverlayCulling culling = OverlayCulling.Camera,
            Overlays? interactive = null,
            Overlays? persistent = null) {

            if (!TMPELifecycle.InGameOrEditor())
                return false;

            CullingMode = culling;

            if (interactive.HasValue)
                InteractiveOverlay = interactive.Value;

            if (persistent.HasValue)
                PersistentOverlays = persistent.Value;

            ApplyInclusionsAndExclusions();

            return AnyOverlaysActive;
        }

        /// <summary>
        /// Turns off all overlays (persistent and interactive).
        /// </summary>
        public void TurnOff() =>
            TurnOff(true);

        /// <summary>
        /// Turn off the current interactive overlay and, optionally,
        /// turn off all persistent overlays.
        /// </summary>
        /// <param name="persistent">
        /// If <c>true</c>, persistent overlays are also turned off.
        /// </param>
        internal void TurnOff(bool persistent) {
            if (!AnyOverlaysActive)
                return;

            InteractiveOverlay = Overlays.None;

            if (persistent)
                PersistentOverlays = Overlays.None;

            ApplyInclusionsAndExclusions();
            // TODO: update caching
        }

        // TODO: this is just rough sketch
        internal void Prepare() {
            if (interactiveOverlay_ != 0)
                PrepareInteractiveOverlay();

            if (compiledPersistentOverlays_ != 0)
                PreparePersistentOverlays();

            // TODO: process hovered elements?
            // TODO: process outlines?
        }

        // TODO: this is just a rough sketch
        private void PrepareInteractiveOverlay() {
            // TODO: render the overlay
        }

        // TODO: this is just a rough sketch
        private void PreparePersistentOverlays() {
            Overlays overlay;

            for (var idx = 0; idx < individualOverlays_.Length; idx++) {
                overlay = individualOverlays_[idx];
                if ((compiledPersistentOverlays_ & overlay) != 0) {
                    // TODO: render the overlay
                }
            }
        }

        internal void DrawGUI() {
            // TODO: draw outlines (can/should that be done in GUI mode?)
            // TODO: draw list of labels (eg. lane ids)
            // TODO: draw GUI for list of prepared icons
        }

        /// <summary>
        /// When <c>true</c>, overlays are being displayed.
        /// </summary>
        /// <remarks>
        /// Control this value via the <c>TurnOn()</c> and
        /// <c>TurnOff()</c> methods.
        /// </remarks>
        public bool AnyOverlaysActive =>
                PersistentOverlays != 0 ||
                InteractiveOverlay != 0;

        /// <summary>
        /// Overlay range culling mode.
        /// </summary>
        public OverlayCulling CullingMode { get; private set; }

        /// <summary>
        /// When <c>true</c>, tunnels will be rendered in overground view,
        /// allowing underground overlays to be displayed and interactive.
        /// </summary>
        public bool PersistentTunnels =>
            IsPersistent(Overlays.Tunnels);

        /// <summary>
        /// If <c>true</c>, underground overlays can be displayed.
        /// </summary>
        internal bool CanDisplayUnderground =>
            // TODO: this will need a rethink
            PersistentTunnels ||
            TrafficManagerTool.IsUndergroundMode;

        /// <summary>
        /// If <c>true</c>, overground overlays can be displayed.
        /// </summary>
        internal bool CanDisplayOverground =>
            // TODO: this will need a rethink
            !TrafficManagerTool.IsUndergroundMode;

        /// <summary>
        /// Determine if any of the specified overlays are persistent,
        /// taking in to account any inclusions/exclusions due to current
        /// interactive overlay.
        /// </summary>
        /// <param name="overlays">
        /// The overlays to check.
        /// </param>
        /// <param name="raw">
        /// If <c>true</c>, ignore any inclusions/exclusions which have
        /// been applied due to the current interactive overlay.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if any of the overlays are persistent.
        /// </returns>
        internal bool IsPersistent(Overlays overlays, bool raw = false) =>
            raw
                ? (persistentOverlays_ & overlays) != 0
                : (compiledPersistentOverlays_ & overlays) != 0;

        /// <summary>
        /// Determine if one of the specified overlays are currently interactive.
        /// </summary>
        /// <param name="overlays">The overlays to check.</param>
        /// <returns>
        /// Returns <c>true</c> if one of the overlays are interactive.
        /// </returns>
        internal bool IsInteractive(Overlays overlays) =>
            (InteractiveOverlay & overlays) != 0;

        /// <summary>
        /// Get the current persistent overlays.
        /// </summary>
        internal Overlays PersistentOverlays {
            get => compiledPersistentOverlays_;
            private set => persistentOverlays_ = ApplyFlagTMPE(value);
        }

        /// <summary>
        /// Get the current interactive overlay.
        /// </summary>
        internal Overlays InteractiveOverlay {
            get => interactiveOverlay_;
            private set => interactiveOverlay_ =
                IsIndividualOverlay(value) ? value : Overlays.None;
        }

        /// <summary>
        /// Set additional persistent overlays to be displayed when
        /// the specified <paramref name="overlay"/> is interactive.
        /// </summary>
        /// <param name="overlay">
        /// The target interactive overlay. If <paramref name="inclusions"/>
        /// are already defined for the overlay, they will be overwritten.
        /// </param>
        /// <param name="inclusions">
        /// Additional overlays to display when the specified interactive
        /// overlay is visible.
        /// </param>
        internal void AddInclusion(Overlays overlay, Overlays inclusions) {
            if (!IsIndividualOverlay(overlay))
                return;

            if (overlayInclusions_.ContainsKey(overlay)) {
                overlayExclusions_[overlay] = inclusions;
            } else {
                overlayInclusions_.Add(overlay, inclusions);
            }

            if (AnyOverlaysActive)
                // TODO: update caching
                ApplyInclusionsAndExclusions();
        }

        /// <summary>
        /// Set persistent overlays that will be hidden when
        /// the specified <paramref name="overlay"/> is interactive.
        /// </summary>
        /// <param name="overlay">
        /// The target interactive overlay. If <paramref name="exclusions"/>
        /// are already defined for the overlay, they will be overwritten.
        /// </param>
        /// <param name="exclusions">
        /// Overlays to hide when specified interactive overlay is visible.
        /// </param>
        internal void AddExclusion(Overlays overlay, Overlays exclusions) {
            if (!IsIndividualOverlay(overlay))
                return;

            if (overlayExclusions_.ContainsKey(overlay)) {
                overlayExclusions_[overlay] = exclusions;
            } else {
                overlayExclusions_.Add(overlay, exclusions);
            }

            if (AnyOverlaysActive)
                // TODO: update caching
                ApplyInclusionsAndExclusions();
        }

        // this could become generic Refresh() method?
        private void ApplyInclusionsAndExclusions() {
            compiledPersistentOverlays_ = persistentOverlays_;

            if (interactiveOverlay_ == Overlays.None)
                return;

            if (overlayInclusions_.TryGetValue(interactiveOverlay_, out var inclusions))
                compiledPersistentOverlays_ |= inclusions;

            if (overlayExclusions_.TryGetValue(interactiveOverlay_, out var exclusions))
                compiledPersistentOverlays_ &= ~exclusions;

            // TODO: also update caching?
        }

        /// <summary>
        /// If the specified overlays include <see cref="Overlays.TMPE"/>,
        /// merge in the user-specified persistent overlays defined via
        /// Overlays tab in mod options.
        /// </summary>
        /// <param name="overlays">The overlays to check.</param>
        /// <returns>Returns compiled set of overlays flags.</returns>
        private Overlays ApplyFlagTMPE(Overlays overlays) {
            if ((Overlays.TMPE & overlays) != 0) {
                // TODO: merge in user options
            }
            return overlays;
        }
    }
}
