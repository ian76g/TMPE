namespace TrafficManager.Overlays {
    using TrafficManager.API.Attributes;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Manager.Overlays.Layers;
    using TrafficManager.Util.Extensions;
    using UnityEngine;

    public class NodesOverlay : IManagedOverlay
    {
        public NodesOverlay() {
            // register with overlay manager
            OverlayManager.Instance.RegisterOverlay(this);
        }

        public bool CanBeUsed =>
            true;

        public NetInfo.LaneType? LaneTypes =>
            null;

        public VehicleInfo.VehicleType? VehicleTypes =>
            null;

        public Overlays Overlay =>
            Overlays.Nodes;

        public CacheTargets Targets =>
            CacheTargets.Nodes;

        public void Reset() {
            // clear selection
        }

        public void Render(OverlayRenderSettings settings) {

            // todo: iterate visible nodes form cache manager instead
            for (int nodeId = 1; nodeId < NetManager.MAX_NODE_COUNT; ++nodeId) {
                // validity check done by cache manager

                // in camera check done by cache manager
                LabelLayer.Instance.Add(new NodeLabel(nodeId));
            }
        }

        internal class NodeLabel : LabelBase, ILabel {

            private const int TEXT_SIZE = 15;
            private readonly Color TEXT_COLOR = new(0f, 0f, 1f);

            public NodeLabel(int nodeId)
                : base(nodeId)
            {
                Text = $"N|{nodeId}";
                TextSize = TEXT_SIZE;
            }

            public override Overlays Overlay => Overlays.Nodes;

            public override CacheTargets Target => CacheTargets.Nodes;

            public override Color TextColor => TEXT_COLOR;

            [Hot("Not per-frame, but occasionally called in large batches")]
            public override Vector3 GetWorldPos() =>
                ((ushort)TargetId).ToNode().m_position;

            [Cold("Mouse interaction")]
            public override bool IsInteractive() =>
                OverlayManager.Instance.IsInteractive(Overlay);

            [Cold("Mouse interaction")]
            public override void OnHover(bool mouseInside) {
                if (mouseInside) {
                    // todo: ask manager to throb node
                    TextSize += 3;
                } else {
                    // todo: tell manager to stop node throb
                    TextSize -= 3;
                }
                return;
            }

            [Cold("Mouse interaction")]
            public override bool OnClick(bool mouseDown, bool mouseInside) {
                // todo: tell manager node is selected
                // todo: display additional node data?

                return true; // consumed
            }
        }
    }
}
