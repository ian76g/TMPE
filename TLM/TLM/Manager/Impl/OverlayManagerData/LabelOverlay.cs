namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;

    internal struct LabelOverlay {
        internal bool IsEnabled;

        internal OverlayTargets Target;

        internal int TargetId;

        internal string Text;

        internal Color TextColor;

        internal int TextSize;

        internal EventHandle OnClick;

        internal EventHandle OnHover;

        internal delegate LabelOverlay EventHandle(LabelOverlay label, OverlayRenderSettings settings);
    }
}
