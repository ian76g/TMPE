namespace TrafficManager.Overlays {
    using System;
    using System.Text;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Manager.Overlays.Layers;
    using TrafficManager.Util.Extensions;
    using UnityEngine;

    public class NodesOverlay : ManagedOverlayBase, IManagedOverlay {

        private static bool permaDetail;

        public NodesOverlay() {
            OverlayManager.Instance.RegisterOverlay(this);
        }

        public override Overlays Overlay => Overlays.Nodes;

        public override CacheTargets Targets => CacheTargets.Nodes;

        public override void OnCameraMoved(ref OverlayConfig config, ref OverlayState state) {

            var nodes = ViewportCache.Instance.visibleNodes;
            var numItems = nodes.Count();

            LabelLayer.Instance.MakeRoomFor(numItems);

            for (int i = 0; i < numItems; i++)
                LabelLayer.Instance.Add(new Label(nodes.m_buffer[i]));
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

            public Label(int nodeId)
                : base(nodeId) { }

            public override Overlays Overlay => Overlays.Nodes;

            public override Vector3 WorldPos => ((ushort)Id).ToNode().m_position;

            public override bool IsInteractive => true;

            public override string GetText(bool mouseInside, ref OverlayState state) {
                return permaDetail || detailed || mouseInside
                    ? DetailString()
                    : BasicString();
            }

            public override bool OnClick(bool mouseInside, ref OverlayState state) {
                detailed = !detailed;
                return false;
            }

            private string BasicString() =>
                new StringBuilder("N", 6).Append(Id).ToString();

            private string DetailString() {
                const int rowLimit = 3;
                int count = 0;

                ref var node = ref ((ushort)Id).ToNode();

                var nodeFlags = node.m_flags;

                var sb = new StringBuilder(150);

                sb.Append("[N ").Append(Id).AppendLine("]");

                sb.Append("Lane: ").AppendLine(node.m_lane.ToString());

                foreach (NetNode.Flags flag in Enum.GetValues(typeof(NetNode.Flags))) {
                    if ((nodeFlags & flag) != 0) {
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
