namespace TrafficManager.Overlays {
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl.OverlayManagerData;

    public interface IManagedOverlay {

        /// <summary>
        /// Indicates if the overlay can be used. This
        /// is primarily to allow feature-reliant overlays
        /// to check their prerequisite mod option.
        /// </summary>
        public bool CanBeUsed { get; }

        public NetInfo.LaneType LaneTypes { get; }

        public VehicleInfo.VehicleType VehicleTypes { get; }

        public Overlays Overlay { get; }

        public bool CanBeInteractive { get; }

        public bool CanBePersistent { get; }

        public OverlayTargets Targets { get; }

        internal abstract void Render(OverlayRenderSettings settings);
    }
}
