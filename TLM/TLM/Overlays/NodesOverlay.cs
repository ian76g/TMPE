namespace TrafficManager.Overlays {
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Util.Extensions;
    using UnityEngine;

    // this is just rough sketch to scope out requirements
    // for label-type overlays and hopefully make them a
    // bit more pleasant to work with
    public class NodesOverlay : IManagedOverlay
    {
        public NodesOverlay() {
            // register with overlay manager
            CanBeUsed = OverlayManager.Instance.RegisterOverlay(this);
        }

        public bool CanBeUsed { get; private set; }

        public NetInfo.LaneType LaneTypes =>
            NetInfo.LaneType.None;

        public VehicleInfo.VehicleType VehicleTypes =>
            VehicleInfo.VehicleType.None;

        public Overlays Overlay =>
            Overlays.Nodes;

        public bool CanBeInteractive =>
            true;

        public bool CanBePersistent =>
            true;

        public OverlayTargets Targets =>
            OverlayTargets.Nodes;

        internal void Render(OverlayRenderSettings settings) {

            bool interactive = settings.IsInteractive(this.Overlay);

            // todo: iterate visible nodes form cache manager instead
            for (int nodeId = 1; nodeId < NetManager.MAX_NODE_COUNT; ++nodeId) {
                var node = ((ushort)nodeId).ToNode();

                // validity check already done by cache manager

                // in camera check already done by cache manager

                // distance from camera done by cache manager

                // what if distance from mouse? would need clean way to check that
                // maybe two iterators - one for camera range, the other mouse range?

                // zoom, font size, etc, should maybe be handled by manager

                // need to specify text colour

                var label = interactive
                    ? GenerateInteractiveLabel(nodeId)
                    : GenerateStaticLabel(nodeId);

                // todo: pass that to manager somehow
            }
        }

        private LabelOverlay GenerateStaticLabel(int nodeId) =>
            new LabelOverlay {
                IsEnabled = true,
                Target = OverlayTargets.Nodes,
                TargetId = nodeId,
                Text = $"#{nodeId}",
                TextColor = textColor,
                TextSize = textSize,
            };

        private LabelOverlay GenerateInteractiveLabel(int nodeId) =>
            new LabelOverlay {
                IsEnabled = true,
                Target = OverlayTargets.Nodes,
                TargetId = nodeId,
                Text = $"#{nodeId}",
                TextColor = textColor,
                TextSize = textSize,
                OnHover = OnHover,
                OnClick = OnClick,
           };

        private LabelOverlay OnHover(LabelOverlay label, OverlayRenderSettings settings) {
            // tell manager to throb node
            label.TextSize = textSize + 3;
            return label;
        }

        private LabelOverlay OnClick(LabelOverlay label, OverlayRenderSettings settings) {
            // tell manager node is selected
            // display a panel with node data
            return label;
        }

        private int textSize = 15;
        private Color textColor = new Color(0f, 0f, 1f);
    }
}
