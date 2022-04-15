namespace TrafficManager.Manager.Overlays.Layers {
    using CSUtil.Commons;
    using System.Diagnostics;
    using System.Text;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Overlays.Layers;
    using UnityEngine;

    public abstract class LabelBase : ILabel {
        protected const string Space = " "; // useful for stringbuilder
        protected const byte TEXT_SIZE = 15;

        public LabelBase(InstanceID id) {
            ID = id;
            DiagnoseErrors(); // DEBUG-only
        }

        public virtual Overlays Overlay => Overlays.None;

        public InstanceID ID { get; private set; }

        public virtual Color TextColor =>
            InfoManager.instance.CurrentMode == InfoManager.InfoMode.None
                ? Color.yellow
                : Color.blue;

        public virtual byte TextSize => 15;

        public virtual string GetText(bool mouseInside, ref OverlayState state) =>
            new StringBuilder(15 + 6)
                .Append(ID.Type.ToString("F"))
                .Append(Space)
                .Append(ID.RawData & 0xFFFFFFu)
                .ToString();

        public abstract Vector3 WorldPos { get; }

        public virtual bool IsInteractive => false;

        public virtual TaskState? OnHover(bool mouseInside, ref OverlayState data) => null;

        public virtual TaskState? OnClick(bool mouseInside, ref OverlayState data) => null;

        [Conditional("DEBUG")]
        internal void DiagnoseErrors() {
            if (!OverlayManager.IsIndividualOverlay(Overlay))
                Log.Error("label.Overlay must be singular");

            if (ID.IsEmpty)
                Log.Error("label.TargetId should be > 0");
        }
    }
}
