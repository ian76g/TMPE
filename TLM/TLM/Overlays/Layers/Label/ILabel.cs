namespace TrafficManager.Manager.Overlays.Layers {
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Overlays.Layers;
    using UnityEngine;

    public interface ILabel {
        Overlays Overlay { get; }

        InstanceID ID { get; }

        Color TextColor { get; }

        byte TextSize { get; }

        string GetText(bool mouseInside, ref OverlayState state);

        Vector3 WorldPos { get; }

        bool IsInteractive { get; }

        TaskState? OnHover(bool mouseInside, ref OverlayState data);

        TaskState? OnClick(bool mouseInside, ref OverlayState data);
    }
}
