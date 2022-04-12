namespace TrafficManager.Manager.Overlays.Layers {
    using CSUtil.Commons;
    using System.Diagnostics;
    using TrafficManager.API.Attributes;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Util.Extensions;
    using UnityEngine;

    public abstract class LabelBase : ILabel {

        public LabelBase(int targetId) {
            TargetId = targetId;
            DiagnoseErrors(); // DEBUG-only
        }

        public virtual Overlays Overlay => Overlays.None;

        public virtual CacheTargets Target => CacheTargets.None;

        public int TargetId { get; private set; }

        public bool IsHidden { get; set; }
        public string Text { get; set; }
        public virtual Color TextColor { get; set; }
        public int TextSize { get; set; }
        public virtual int Custom1 { get; set; }
        public virtual int Custom2 { get; set; }

        [Hot("Occasionally called in large batches")]
        public abstract Vector3 GetWorldPos();

        [Cold("Mouse interaction")]
        public abstract bool IsInteractive();

        [Cold("Mouse interaction")]
        public virtual bool OnClick(bool mouseDown, bool mouseInside) => false;

        [Cold("Mouse interaction")]
        public virtual void OnHover(bool mouseInside) { }

        [Cold("Mouse interaction")]
        public virtual bool OnScroll(int scrollDelta) => false;

        [Conditional("DEBUG")]
        internal void DiagnoseErrors() {
            if (!OverlayManager.IsIndividualOverlay(Overlay))
                Log.Error("label.Overlay must be singular");

            if (!Target.IsSingleFlag())
                Log.Error("label.Target must specify a single target");

            if (TargetId <= 0)
                Log.Error("label.TargetId must be > 0");
        }
    }
}
