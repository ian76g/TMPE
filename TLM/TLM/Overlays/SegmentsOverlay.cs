namespace TrafficManager.Overlays {
    using System;
    using System.Text;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Manager.Overlays.Layers;
    using TrafficManager.Util.Extensions;
    using UnityEngine;

    public class SegmentsOverlay : ManagedOverlayBase, IManagedOverlay {

        private static bool permaDetail;

        public SegmentsOverlay() {
            OverlayManager.Instance.RegisterOverlay(this);
        }

        public override Overlays Overlay => Overlays.Segments;

        public override EntityType Targets => EntityType.Segment;

        public override void OnCameraMoved(ref OverlayConfig config, ref OverlayState state) {

            var segments = ViewportCache.Instance.newSegments;
            var numItems = segments.Count();

            LabelLayer.Instance.MakeRoomFor(numItems);

            for (int i = 0; i < numItems; i++) {
                LabelLayer.Instance.Add(new Label(segments.m_buffer[i]));
            }
        }

        public override void OnModifierChanged(ref OverlayConfig config, ref OverlayState state) {
            if (state.Shift || permaDetail) {
                permaDetail = state.Shift;
                LabelLayer.Instance.Invalidate(this.Overlay);
            }
        }

        public override void Reset() { }

        private class Label : LabelBase, ILabel {

            private bool detailed;

            public Label(ExtId segmentId)
                : base(segmentId) { }

            public override Overlays Overlay => Overlays.Segments;

            // .m_bounds.center ?
            public override Vector3 WorldPos => ((ushort)this.ID.Id).ToSegment().m_middlePosition;

            public override bool IsInteractive => true;

            public override string GetText(bool mouseInside, ref OverlayState state) {
                return permaDetail || this.detailed || mouseInside
                    ? this.DetailString()
                    : this.BasicString();
            }

            public override bool OnClick(bool mouseInside, ref OverlayState state) {
                this.detailed = !this.detailed;
                return false;
            }

            private string BasicString() =>
                new StringBuilder("S", 6).Append(this.ID.Id).ToString();

            private string DetailString() {
                const int rowLimit = 3;
                int count = 0;

                ref var segment = ref ((ushort)this.ID.Id).ToSegment();

                var segmentFlags = segment.m_flags;

                var sb = new StringBuilder(150);

                sb.Append("[N ").Append(this.ID.Id).AppendLine("]");

                NetInfo info = segment.Info;

                sb.Append(info.GetService()).Append(" > ").AppendLine(info.GetSubService().ToString("f"));

                // we need some sort of pop-up window to display details IMO

                foreach (NetSegment.Flags flag in Enum.GetValues(typeof(NetSegment.Flags))) {
                    if ((segmentFlags & flag) != 0) {
                        sb.Append(flag);
                        if (count++ > rowLimit) {
                            sb.AppendLine();
                            count = 0;
                        } else {
                            sb.Append(Space);
                        }
                    }
                }

                return sb.ToString();
            }
        }
    }
}
