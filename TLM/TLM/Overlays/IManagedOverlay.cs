namespace TrafficManager.Overlays {
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl.OverlayManagerData;

    public interface IManagedOverlay {

        /// <summary>
        /// Indicates if the overlay can be used. This
        /// is primarily to allow feature-reliant overlays
        /// to check their prerequisite mod option.
        /// </summary>
        bool CanBeUsed { get; }

        NetInfo.LaneType? LaneTypes { get; }

        VehicleInfo.VehicleType? VehicleTypes { get; }

        Overlays Overlay { get; }

        CacheTargets Targets { get; }

        void Reset();

        void Render(OverlayRenderSettings settings);
    }
}
