namespace TrafficManager.Manager.Overlays.Layers {
    using CSUtil.Commons;
    using System.Diagnostics;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using UnityEngine;

    public abstract class LabelBase : ILabel {
        protected const string Space = " "; // useful for stringbuilder
        protected const int TEXT_SIZE = 15;

        public LabelBase(int targetId) {
            Id = targetId;
            DiagnoseErrors(); // DEBUG-only
        }

        public virtual Overlays Overlay => Overlays.None;

        public int Id { get; private set; }

        public virtual Color TextColor =>
            InfoManager.instance.CurrentMode == InfoManager.InfoMode.None
                ? Color.yellow
                : Color.blue;

        public virtual int TextSize => 15;

        public abstract string GetText(bool mouseInside, ref OverlayState state);

        public abstract Vector3 WorldPos { get; }

        public virtual bool IsInteractive => false;

        public virtual bool OnHover(bool mouseInside, ref OverlayState data) => false;

        public virtual bool OnClick(bool mouseInside, ref OverlayState data) => false;

        [Conditional("DEBUG")]
        internal void DiagnoseErrors() {
            if (!OverlayManager.IsIndividualOverlay(Overlay))
                Log.Error("label.Overlay must be singular");

            if (Id <= 0)
                Log.Error("label.TargetId should be > 0");
        }
    }
}
