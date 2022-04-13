namespace TrafficManager.Overlays {
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using static InfoManager;

    /// <summary>
    /// Renders tunnels in overground views. This enables
    /// rendering and interaction of some underground overlays
    /// (at discretion of the overlay).
    /// </summary>
    /// <remarks>
    /// Cannot be used in Info contexts. Cannot be interactive.
    /// </remarks>
    public class TunnelsOverlay : ManagedOverlayBase, IManagedOverlay
    {
        public TunnelsOverlay() {
            // register with overlay manager
            OverlayManager.Instance.RegisterOverlay(this);
        }

        public override Overlays Overlay =>
            Overlays.Tunnels;

        public override CacheTargets Targets =>
            CacheTargets.None;

        public override void OnFrameChanged(
            ref OverlayConfig settings,
            ref OverlayState data) {

            if (InfoManager.instance.CurrentMode == InfoMode.None)
                TransportManager.instance.TunnelsVisible = true;
        }

        public override void Reset() { }
    }
}
