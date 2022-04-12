namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using System.Diagnostics.CodeAnalysis;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;

    public class MapCache {
        // Flags to inspect during validity checks
        private const NetNode.Flags NODE_FLAGS =
            NetNode.Flags.Created |
            NetNode.Flags.Deleted |
            NetNode.Flags.Collapsed;

        private const NetSegment.Flags SEGMENT_FLAGS =
            NetSegment.Flags.Created |
            NetSegment.Flags.Deleted |
            NetSegment.Flags.Collapsed;

        private static readonly NetManager Networks = NetManager.instance;

        // lookup id -> object
        private readonly NetNode[] allNodes = Networks.m_nodes.m_buffer;
        private readonly NetSegment[] allSegments = Networks.m_segments.m_buffer;

        // results: lists of ids in view
        private readonly FastList<ushort> visibleNodes = new();
        private readonly FastList<ushort> visibleSegments = new();

        // grids
        private readonly ushort[] nodeGrid = Networks.m_nodeGrid;
        private readonly ushort[] segmentGrid = Networks.m_segmentGrid;

        // todo: pull from mod option
        private bool useFastestApproach = true;

        // controls which caches are generated
        private bool scanNodes;
        private bool scanSegments;

        /// <summary>
        /// Cache objects that are within camera view.
        /// </summary>
        /// <param name="settings">Current overlay render settings.</param>
        /// <param name="cameraInfo">Current main camera info.</param>
        /// <remarks>Should be called from <c>SimulationManager.EndRenderingImpl()</c>.</remarks>
        public void CacheVisibleObjects(OverlayRenderSettings settings, RenderManager.CameraInfo cameraInfo) {
            var requires = settings.Targets;

            scanNodes = (requires & CacheTargets.Nodes) != 0;
            scanSegments = (requires & CacheTargets.Segments) != 0;

            if (useFastestApproach) {
                CacheVisible_Fastest(cameraInfo);
            } else {
                CacheVisible_Quality(cameraInfo);
            }
        }

        private void CacheVisible_Fastest(RenderManager.CameraInfo cameraInfo) {
            visibleNodes.Clear();
            visibleSegments.Clear();

            FastList<RenderGroup> renderedGroups = RenderManager.instance.m_renderedGroups;

            for (int i = 0; i < renderedGroups.m_size; i++) {

                RenderGroup renderGroup = renderedGroups.m_buffer[i];

                if (renderGroup.m_instanceMask != 0) {

                    int minX = renderGroup.m_x * 270 / 45;
                    int minZ = renderGroup.m_z * 270 / 45;
                    int maxX = (renderGroup.m_x + 1) * 270 / 45 - 1;
                    int maxZ = (renderGroup.m_z + 1) * 270 / 45 - 1;

                    for (int j = minZ; j <= maxZ; j++) {
                        for (int k = minX; k <= maxX; k++) {

                            int cellIdx = j * 270 + k;

                            if (scanNodes) {
                                ushort nodeID = nodeGrid[cellIdx];
                                while (nodeID != 0) {
                                    if ((allNodes[nodeID].m_flags & NODE_FLAGS) == NetNode.Flags.Created)
                                        visibleNodes.Add(nodeID);

                                    nodeID = allNodes[nodeID].m_nextGridNode;
                                }
                            }

                            if (scanSegments) {
                                ushort segmentID = segmentGrid[cellIdx];
                                while (segmentID != 0) {
                                    if ((allSegments[segmentID].m_flags & SEGMENT_FLAGS) == NetSegment.Flags.Created)
                                        visibleSegments.Add(segmentID);

                                    segmentID = allSegments[segmentID].m_nextGridSegment;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CacheVisible_Quality(RenderManager.CameraInfo cameraInfo) {
            visibleNodes.Clear();
            visibleSegments.Clear();

            float maxDistance = 2000f; // needs tuning

            GetQualityMinMaxXZ(cameraInfo, maxDistance, out int minX, out int minZ, out int maxX, out int maxZ);

            for (int j = minZ; j <= maxZ; j++) {
                for (int k = minX; k <= maxX; k++) {

                    int cellIdx = j * 270 + k;

                    if (scanNodes) {
                        ushort nodeID = nodeGrid[cellIdx];
                        while (nodeID != 0) {
                            if ((allNodes[nodeID].m_flags & NODE_FLAGS) == NetNode.Flags.Created)
                                visibleNodes.Add(nodeID);

                            nodeID = allNodes[nodeID].m_nextGridNode;
                        }
                    }

                    if (scanSegments) {
                        ushort segmentID = segmentGrid[cellIdx];
                        while (segmentID != 0) {
                            if ((allSegments[segmentID].m_flags & SEGMENT_FLAGS) == NetSegment.Flags.Created)
                                visibleSegments.Add(segmentID);

                            segmentID = allSegments[segmentID].m_nextGridSegment;
                        }
                    }
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "Readability.")]
        private void GetQualityMinMaxXZ(
            RenderManager.CameraInfo cameraInfo,
            float maxDistance,
            out int minX, out int minZ,
            out int maxX, out int maxZ) {

            var camPos = cameraInfo.m_position;

            var dirA = cameraInfo.m_directionA;
            var dirB = cameraInfo.m_directionB;
            var dirC = cameraInfo.m_directionC;
            var dirD = cameraInfo.m_directionD;

            float near = cameraInfo.m_near;
            float far = Mathf.Min(maxDistance, cameraInfo.m_far);

            Vector3 nearA = camPos + dirA * near;
            Vector3 nearB = camPos + dirB * near;
            Vector3 nearC = camPos + dirC * near;
            Vector3 nearD = camPos + dirD * near;

            Vector3 farA = camPos + dirA * far;
            Vector3 farB = camPos + dirB * far;
            Vector3 farC = camPos + dirC * far;
            Vector3 farD = camPos + dirD * far;

            Vector3 posMin = Vector3.Min(
                Vector3.Min(Vector3.Min(nearA, nearB), Vector3.Min(nearC, nearD)),
                Vector3.Min(Vector3.Min(farA, farB), Vector3.Min(farC, farD)));

            Vector3 posMax = Vector3.Max(
                Vector3.Max(Vector3.Max(nearA, nearB), Vector3.Max(nearC, nearD)),
                Vector3.Max(Vector3.Max(farA, farB), Vector3.Max(farC, farD)));

            minX = Mathf.Max((int)((posMin.x - 40f) / 64f + 135f), 0);
            minZ = Mathf.Max((int)((posMin.z - 40f) / 64f + 135f), 0);
            maxX = Mathf.Min((int)((posMax.x + 40f) / 64f + 135f), 269);
            maxZ = Mathf.Min((int)((posMax.z + 40f) / 64f + 135f), 269);
        }
    }
}
