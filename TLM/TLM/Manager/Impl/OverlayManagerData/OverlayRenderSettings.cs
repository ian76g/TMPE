namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using System;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.State;
    using static InfoManager;

    internal struct OverlayRenderSettings {

        internal OverlayContext Context;

        // Only applicable when Context = Info
        internal InfoMode Info;

        // Only applicable when Context = Tool
        internal Type Tool;

        internal OverlayCulling Culling;

        internal Overlays Persistent;

        internal Overlays Interactive;

        internal RestrictedVehicles Filter;

        internal OverlayRenderSettings Compile() {

            // Replace with TMPE mod option config
            if ((Persistent & Overlays.TMPE) != 0)
                Persistent = Options.CompiledOverlays;

            // Remove interactive overlay from persistent overlays
            Persistent &= ~Interactive;

            return this;
        }

        internal bool IsEnabled =>
            Context != OverlayContext.None &&
            AllOverlays != 0;

        internal bool IsContext(InfoMode mode) =>
            Context == OverlayContext.Info &&
            Info == mode;

        internal bool IsContext(Type tool) =>
            Context == OverlayContext.Tool &&
            Tool == tool;

        internal bool IsInteractive(Overlays overlay) =>
            (Interactive & overlay) != 0;

        internal bool IsPersistent(Overlays overlay) =>
            (Persistent & overlay) != 0;

        internal Overlays AllOverlays =>
            Persistent | Interactive;

        internal bool HasOverlay(Overlays overlay) =>
            (AllOverlays & overlay) != 0;
    }
}
