namespace TrafficManager.Manager.Impl {
    using CSUtil.Commons;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.API.Manager;
    using TrafficManager.Lifecycle;
    using TrafficManager.UI;
    using static InfoManager;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using System;
    using TrafficManager.Overlays;

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
        : AbstractGeometryObservingManager, IOverlayManager {

        static OverlayManager() {
            IndividualOverlaysArray = IndividualOverlays.ToArray();

            Instance = new OverlayManager();
        }

        public OverlayManager() {
            currentRenderSettings = InactiveSettings;
        }

        public static OverlayManager Instance;

        /// <summary>
        /// Used for fast iteration of individual overlay flags.
        /// </summary>
        private static readonly Overlays[] IndividualOverlaysArray;

        /// <summary>
        /// The currently active render settings.
        /// </summary>
        private OverlayRenderSettings currentRenderSettings;

        /// <summary>Check if an overlay is interactive.</summary>
        /// <param name="overlays">Overlays to check.</param>
        /// <returns>Returns <c>true</c> if any are interactive.</returns>
        public bool IsInteractive(Overlays overlays) =>
            currentRenderSettings.IsEnabled &&
            currentRenderSettings.IsInteractive(overlays);

        /// <summary>Check if an overlay is persistent.</summary>
        /// <param name="overlays">Overlays to check.</param>
        /// <returns>Returns <c>true</c> if any are presistent.</returns>
        public bool IsPersistent(Overlays overlays) =>
            currentRenderSettings.IsEnabled &&
            currentRenderSettings.IsPersistent(overlays);

        // TODO: this is just rough sketch - should be event driven
        protected void DetectCurrentContext() {
            if (!TMPELifecycle.InGameOrEditor() || TMPELifecycle.Instance.Deserializing) {
                // TODO: should be event driven
                currentRenderSettings = InactiveSettings;
                UpdateCacheTargets();
                // todo: clear throbbers
                return;
            }

            bool valid = IsValidContext(currentRenderSettings);

            // true -> we can render it
            // false -> check tool, then info, otherwise inactive
            //          ...then render if valid
        }

        /// <summary>
        /// Checks the settings to see if their context is still valid.
        /// </summary>
        /// <param name="settings">
        /// The settings to inspect.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if valid, otherwise <c>false</c>.
        /// </returns>
        private bool IsValidContext(OverlayRenderSettings settings) =>
            settings.Context switch {
                OverlayContext.Custom => true,
                OverlayContext.Tool =>
                    settings.Tool == GetToolControllerType(),
                OverlayContext.Info =>
                    settings.Info == InfoManager.instance.CurrentMode,
                OverlayContext.None => false,
                _ => false,
            };

        private void UpdateCacheTargets() {
            var targets = OverlayTargets.None;

            if (!currentRenderSettings.IsEnabled) {
                // pass targets to cache manager
                return;
            }

            for (var idx = 0; idx < IndividualOverlaysArray.Length; idx++) {
                var overlay = IndividualOverlaysArray[idx];
                if (currentRenderSettings.HasOverlay(overlay))
                    targets |= Targets[overlay];
            }

            // pass targets to cache manager
        }

        internal bool RegisterOverlay(IManagedOverlay overlay) {
            if (!IsIndividualOverlay(overlay.Overlay))
                return false;

            Targets[overlay.Overlay] = overlay.Targets;

            return true;
        }

        // Keys must match the keys of IndividualOverlays.
        // TODO: these should be defined by the ovelay itself
        // and have overlays register themselves with the manager
        // Map: Overlays flag -> overlay class -> targets
        private static readonly Dictionary<Overlays, OverlayTargets> Targets = new(IndividualOverlays.Count) {
            { Overlays.PrioritySigns, OverlayTargets.Nodes },
            { Overlays.TrafficLights, OverlayTargets.Nodes },
            { Overlays.SpeedLimits, OverlayTargets.Segments },
            { Overlays.VehicleRestrictions, OverlayTargets.Segments },
            { Overlays.ParkingRestrictions, OverlayTargets.Segments },
            { Overlays.ParkingSpaces, OverlayTargets.Buildings },
            { Overlays.JunctionRestrictions, OverlayTargets.Nodes },
            { Overlays.LaneConnectors, OverlayTargets.Nodes },
            { Overlays.LaneArrows, OverlayTargets.Nodes },
            { Overlays.Nodes, OverlayTargets.Nodes },
            { Overlays.Lanes, OverlayTargets.Lanes },
            { Overlays.Vehicles, OverlayTargets.Vehicles },
            { Overlays.PathUnits, OverlayTargets.None },
            { Overlays.Citizens, OverlayTargets.None },
            { Overlays.Buildings, OverlayTargets.Buildings },
        };

        // gently throb click targets to draw user attention to them
        // base on code from (I think...) https://github.com/CitiesSkylinesMods/TMPE/pull/391
        // will probably need some sort of filter - eg. _which_ nodes can junction restrictions be used on?
        // but that's task for call site (ie. in the overlay itself where it scans the cache of its targets
        // to determine which are valid, then it passes the ids of those to Throb() and they'll just keep throbbing
        // until told otherwise (passing no params = turn off throb)
        internal void Throb(OverlayTargets type = OverlayTargets.None, int[] ids = null) {
            if (type != OverlayTargets.None && ids != null) {
                ThrobberType = type;
                ThrobberIds = ids;
                // todo: start throbing those ids
            } else {
                ThrobberType = OverlayTargets.None;
                ThrobberIds = null;
                // todo: stop throbbing
            }
        }

        private OverlayTargets ThrobberType;

        private int[] ThrobberIds;

        /// <summary>
        /// Overlay render settings to use when inactive.
        /// </summary>
        private static OverlayRenderSettings InactiveSettings =>
            new OverlayRenderSettings {
                Context = OverlayContext.None,
                Culling = OverlayCulling.None,
                Persistent = Overlays.None,
                Interactive = Overlays.None,
                Filter = RestrictedVehicles.None,
            };

        private static OverlayRenderSettings AwarenessSettings =>
            new OverlayRenderSettings {
                Context = OverlayContext.Custom,
                Culling = OverlayCulling.Mouse,
                Persistent = Overlays.GroupAwareness,
                Interactive = Overlays.None,
                Filter = RestrictedVehicles.All,
            };

        // Vague idea - manager should know when a target
        // is hovered (eg. via throbber ids) and sets
        // relevant id (could just be one id and a separate
        // field for id type, similar to throbber targets)
        // which in turn will replace its throbber with
        // static outline so user knows mouse is over
        // clickable item. This is only relevant when an
        // interactive overlay is present.
        internal ushort HoveredNodeId { get; set; }
        internal ushort HoveredSegmentId { get; set; }
        internal uint HoveredLaneId { get; set; }

        // Vague idea - manager should keep track of what's
        // selected (overlay can too?) so that selection
        // outlines can be automated. Some items will need
        // option to specify 'selection style' - eg. the
        // "lane tubes" for lane connectors vs. the "lane
        // outlines" for parking restrictions.
        internal ushort[] SelectedNodeIds { get; set; }
        internal ushort[] SelectedSegmentIds { get; set; }
        internal uint[] SelectedLaneIds { get; set; }
        internal ushort SelectedBuildingId { get; set; }
        internal ushort SelectedVehicleId { get; set; }

        /// <summary>
        /// A list of the individual overlays, excluding any compound flags.
        /// </summary>
        internal static readonly ReadOnlyCollection<Overlays> IndividualOverlays =
            // TODO: find way to sync this list to Overlays enum
            new(new List<Overlays>(15) {
                Overlays.PrioritySigns,
                Overlays.TrafficLights,
                Overlays.SpeedLimits,
                Overlays.VehicleRestrictions,
                Overlays.ParkingRestrictions,
                Overlays.ParkingSpaces,
                Overlays.JunctionRestrictions,
                Overlays.LaneConnectors,
                Overlays.LaneArrows,
                Overlays.Nodes,
                Overlays.Lanes,
                Overlays.Vehicles,
                Overlays.PathUnits,
                Overlays.Citizens,
                Overlays.Buildings,
        });

        /// <summary>
        /// Determine if exactly one overlay is specified in the supplied flags.
        /// </summary>
        /// <param name="overlay">The overlay flags to inspect.</param>
        /// <param name="silent">If <c>true</c>, error logging is suppressed.</param>
        /// <returns>Returns <c>true</c> if only a single flag is set.</returns>
        public bool IsIndividualOverlay(Overlays overlay, bool silent = false) {
            if (!IndividualOverlays.Contains(overlay)) {
                if (!silent)
                    Log.Error($"Invalid overlay flags: {overlay}");

                return false;
            }
            return true;
        }

        /// <summary>
        /// Turn on situational awareness overlays.
        /// Ideal for external mods.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        public bool TurnOn() =>
            TurnOn(AwarenessSettings);

        /// <summary>Turn on specified persistent overlays.</summary>
        /// <param name="persistent">The overlays to show.</param>
        /// <param name="culling">Overlay culling mode.</param>
        /// <returns>Returns <c>true</c> if successful.</returns>
        public bool TurnOn(
            Overlays persistent,
            OverlayCulling culling = OverlayCulling.Mouse) =>
                TurnOn(new OverlayRenderSettings {
                    Context = OverlayContext.Custom,
                    Culling = culling,
                    Persistent = persistent,
                    Interactive = Overlays.None,
                    Filter = RestrictedVehicles.All,
                });

        /// <summary>Apply render settings to turn on overlays.</summary>
        /// <param name="settings">Overlay render settings.</param>
        /// <returns>Returns <c>true</c> if successful.</returns>
        internal bool TurnOn(OverlayRenderSettings settings) {

            if (!TMPELifecycle.InGameOrEditor() || TMPELifecycle.Instance.Deserializing)
                return false;

            currentRenderSettings = settings.Compile();

            return AnyOverlaysActive;
        }

        /// <summary>
        /// Turns off all overlays (persistent and interactive).
        /// </summary>
        public void TurnOff() {
            currentRenderSettings = InactiveSettings;

            // TODO: update caching
        }

        // TODO: this is just rough sketch
        internal void RenderOverlays() {
            Overlays overlay = currentRenderSettings.Interactive;

            if (overlay != Overlays.None) {
                // trigger interactive overlay
            }

            // Persistent overlays
            for (var idx = 0; idx < IndividualOverlaysArray.Length; idx++) {
                overlay = IndividualOverlaysArray[idx];

                if (currentRenderSettings.IsPersistent(overlay)) {
                    // trigger persistent overlay
                }
            }

            // TODO: process hovered elements?
            // TODO: process outlines?
        }

        // Vague idea - overlays generate list of icons they want shown,
        // along with any selections, labels, to display. Manager then
        // handles those in batches, checking for hovered state, etc.
        internal void DrawGUI() {
            // TODO: draw outlines (can/should that be done in GUI mode?)
            // TODO: draw list of labels (eg. lane ids)
            // TODO: draw GUI for list of prepared icons
        }

        /// <summary>Check if any overlays are currently active.</summary>
        /// <returns>Returns <c>true</c> if at least on overlay is active.</returns>
        public bool AnyOverlaysActive =>
            currentRenderSettings.IsEnabled;

        /// <summary>
        /// When <c>true</c>, tunnels will be rendered in overground view,
        /// allowing underground overlays to be displayed and interactive.
        /// </summary>
        public bool PersistentTunnels =>
            currentRenderSettings.IsEnabled &&
            currentRenderSettings.IsPersistent(Overlays.Tunnels);

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
        /// Gets the <see cref="Type"/> of the
        /// <see cref="ToolsModifierControl.toolController.CurrentTool"/>.
        /// </summary>
        /// <returns>
        /// Returns the type if successful, otherwise <c>null</c>.
        /// </returns>
        private Type GetToolControllerType() {
            if (ToolsModifierControl.toolController?.CurrentTool == null)
                return null;

            var toolBase = ToolsModifierControl.toolController.CurrentTool;

            return toolBase.GetType();
        }

        /// <summary>
        /// Attempts to retrieve <see cref="OverlayRenderSettings"/> for the currently
        /// active <see cref="ToolsModifierControl.toolController.CurrentTool"/>.
        /// </summary>
        /// <param name="settings">
        /// The retrieved render settings.</param>
        /// <returns>
        /// Returns <c>true</c> if successful, otherwise <c>false</c>.
        /// </returns>
        private bool TryGetToolSettings(out OverlayRenderSettings settings) {
            Type tool = GetToolControllerType();

            if (tool != null &&
                ToolControllerOverlays.Lookup.TryGetValue(tool, out var toolSettings)) {

                settings = toolSettings;
                return true;
            }

            settings = InactiveSettings;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve <see cref="OverlayRenderSettings"/> for the currently
        /// active <see cref="InfoManager.instance.CurrentMode"/>.
        /// </summary>
        /// <param name="settings">
        /// The retrieved render settings.</param>
        /// <returns>
        /// Returns <c>true</c> if successful, otherwise <c>false</c>.
        /// </returns>
        private bool TryGetInfoSettings(out OverlayRenderSettings settings) {
            InfoMode info = InfoManager.instance.CurrentMode;

            if (InfoViewOverlays.Lookup.TryGetValue(info, out var toolSettings)) {
                settings = toolSettings;
                return true;
            }

            settings = InactiveSettings;
            return false;
        }
    }
}
