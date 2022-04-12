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
    public class TunnelsOverlay : IManagedOverlay
    {
        public TunnelsOverlay() {
            // register with overlay manager
            OverlayManager.Instance.RegisterOverlay(this);
        }

        public bool CanBeUsed =>
            true;

        public NetInfo.LaneType? LaneTypes =>
            null;

        public VehicleInfo.VehicleType? VehicleTypes =>
            null;

        public Overlays Overlay =>
            Overlays.Tunnels;

        public CacheTargets Targets =>
            CacheTargets.None;

        public void Reset() { }

        public void Render(OverlayRenderSettings settings) {
            if (InfoManager.instance.CurrentMode == InfoMode.None)
                TransportManager.instance.TunnelsVisible = true;
        }
    }
}
