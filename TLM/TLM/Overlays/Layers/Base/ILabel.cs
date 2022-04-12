namespace TrafficManager.Manager.Overlays.Layers {
    using TrafficManager.API.Attributes;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;

    public interface ILabel {
        Overlays Overlay { get; }

        CacheTargets Target { get; }

        int TargetId { get; }

        bool IsHidden { get; set; }

        string Text { get; set; }

        Color TextColor { get; set; }

        int TextSize { get; set; }

        int Custom1 { get; set; }

        int Custom2 { get; set; }

        [Hot("Not per-frame, but occasionally called in large batches")]
        Vector3 GetWorldPos();

        [Cold("Mouse interaction")]
        bool IsInteractive();

        [Cold("Mouse interaction")]
        void OnHover(bool mouseInside);

        [Cold("Mouse interaction")]
        bool OnClick(bool mouseDown, bool mouseInside);

        [Cold("Mouse interaction")]
        bool OnScroll(int scrollDelta);
    }
}
