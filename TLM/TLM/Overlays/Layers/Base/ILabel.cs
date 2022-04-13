namespace TrafficManager.Manager.Overlays.Layers {
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using UnityEngine;

    public interface ILabel {
        Overlays Overlay { get; }

        int Id { get; }

        Color TextColor { get; }

        int TextSize { get; }

        string GetText(bool mouseInside, ref OverlayState state);

        Vector3 WorldPos { get; }

        bool IsInteractive { get; }

        bool OnHover(bool mouseInside, ref OverlayState data);

        bool OnClick(bool mouseInside, ref OverlayState data);
    }
}
