namespace TrafficManager.Overlays {
    using System;
    using TrafficManager.API.Attributes;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl.OverlayManagerData;

    public abstract class ManagedOverlayBase : IManagedOverlay {
        public virtual bool CanBeUsed => true;

        public virtual NetInfo.LaneType? LaneTypes => null;

        public virtual VehicleInfo.VehicleType? VehicleTypes => null;

        public abstract Overlays Overlay { get; }

        public abstract EntityType Targets { get; }

        [Cold("User pressed/released modifier key")]
        public virtual void OnCameraMoved(ref OverlayConfig config, ref OverlayState state) { }

        [Hot("Each frame while overlay active")]
        [Obsolete("This will be removed once tunnel overlay is done via patching")]
        public virtual void OnFrameChanged(ref OverlayConfig config, ref OverlayState state) { }

        [Cold("Camera moved noticeably")]
        public virtual void OnInfoViewChanged(ref OverlayConfig config, ref OverlayState state) { }

        [Cold("InfoMode changed")]
        public virtual void OnModifierChanged(ref OverlayConfig config, ref OverlayState state) { }

        public abstract void Reset();
    }
}
