namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Util.Extensions;
    using UnityEngine;

    public class ViewportCache {
        // Flags to inspect during validity checks
        private const NetNode.Flags NODE_FLAGS =
            NetNode.Flags.Created |
            NetNode.Flags.Deleted |
            NetNode.Flags.Collapsed;

        private const NetSegment.Flags SEGMENT_FLAGS =
            NetSegment.Flags.Created |
            NetSegment.Flags.Deleted |
            NetSegment.Flags.Collapsed;

        private const Building.Flags BUILDING_FLAGS =
            Building.Flags.Created |
            Building.Flags.Deleted |
            Building.Flags.Collapsed;

        private static ViewportCache instance;

        public static ViewportCache Instance =>
            instance ??= new ViewportCache();

        // managers
        private static readonly NetManager Networks = NetManager.instance;
        private static readonly BuildingManager Buildings = BuildingManager.instance;

        // grid cells currently in view
        private readonly FastList<int> visibleCells = new();

        // lookup: id -> object
        private readonly NetNode[] allNodes = Networks.m_nodes.m_buffer;
        private readonly NetSegment[] allSegments = Networks.m_segments.m_buffer;
        private readonly Building[] allBuildings = Buildings.m_buildings.m_buffer;

        // reusable id
        private InstanceID id = InstanceID.Empty;

        // temp id list (because we can't `HashSet.EnsureCapacity()` *sigh*)
        // current usage is not thread-safe
        private readonly FastList<InstanceID> movedIDs = new();

        // moved
        public HashSet<InstanceID> movedNodes = new();
        public HashSet<InstanceID> movedSegments = new();
        public HashSet<InstanceID> movedBuildings = new();

        // added
        public readonly FastList<InstanceID> newNodes = new();
        public readonly FastList<InstanceID> newSegments = new();
        public readonly FastList<InstanceID> newBuildings = new();

        // old
        private HashSet<InstanceID> oldNodes = new();
        private HashSet<InstanceID> oldSegments = new();
        private HashSet<InstanceID> oldBuildings = new();

        // grids
        private readonly ushort[] nodeGrid = Networks.m_nodeGrid;
        private readonly ushort[] segmentGrid = Networks.m_segmentGrid;
        private readonly ushort[] buildingGrid = Buildings.m_buildingGrid;

        // todo: pull from mod option
        private bool useFastestApproach = true;

        /// <summary>
        /// Cache objects that are within camera view.
        /// </summary>
        /// <param name="config">Current overlay config.</param>
        /// <param name="state">Current overlay state.</param>
        /// <remarks>Should be called from <c>SimulationManager.EndRenderingImpl()</c>.</remarks>
        public void ScanVisibleMap(ref OverlayConfig config, ref OverlayState state) {

            var requires = state.RefreshMapCache;

            if (requires == EntityType.None) return;

            // TODO: if camera zoomed out too far (eg. satellite view) just bail

            // TODO: if camera zoomed in (ie. it's within the previous viewport)
            // there's no point updating caches - layer managers can easily filter
            // out what's not in view. We should only rescan when camera is showing
            // part of map that was not previously visible. I have no clue how to do that.

            // cache grid cells currently in view
            if (useFastestApproach) {
                BuildGrid_Fastest();
            } else {
                BuildGrid_Quality(ref state.CameraInfo);
            }

            // TODO: Now that we have handy list of cells, can we compare with
            // a list of previous cells to work out what changed? And if not
            // too much, do a micro-generate of just the new stuff? The layers
            // can easily filter out stuff not in view (or we could even pass
            // them a list of removed cells = batch hide).

            // I've split the scans in to object type for now
            // as I'm not sure if we can do them all together
            // or which approach has better performance.

            if ((requires & EntityType.Node) != 0)
                ScanNodes();

            if ((requires & EntityType.Segment) != 0)
                ScanSegments();

            if ((requires & EntityType.Building) != 0)
                ScanBuildings();
        }

        /* TODO:
         * 
         * We could probably put the ScanNodes() etc methods in to threads
         * but would need to ensure therad-safe iteration of `visibleCells`
         * list and also create separate instances of the `movedIDs` list.
         * 
         */

        private void ScanNodes() {
            oldNodes = new(newNodes.m_buffer);
            movedIDs.ClearAndEnsureCapacity(newNodes.m_size);
            newNodes.Clear();

            ref int[] cells = ref visibleCells.m_buffer;

            for (var i = 0; i < visibleCells.m_size; i++) {

                ushort currentID = nodeGrid[cells[i]];

                while (currentID != 0) {

                    if ((allNodes[currentID].m_flags & NODE_FLAGS) == NetNode.Flags.Created) {
                        id.NetNode = currentID;

                        if (oldNodes.Contains(id)) {
                            movedIDs.Add(id);
                        } else {
                            newNodes.Add(id);
                        }
                    }

                    currentID = allNodes[currentID].m_nextGridNode;
                }
            }

            movedNodes = new(movedIDs.ToArray());
        }

        private void ScanSegments() {
            oldSegments = new(newSegments.m_buffer);
            movedIDs.ClearAndEnsureCapacity(newSegments.m_size);
            newSegments.Clear();

            ref int[] cells = ref visibleCells.m_buffer;

            for (var i = 0; i < visibleCells.m_size; i++) {

                ushort currentID = segmentGrid[cells[i]];

                while (currentID != 0) {

                    if ((allSegments[currentID].m_flags & SEGMENT_FLAGS) == NetSegment.Flags.Created) {
                        id.NetSegment = currentID;

                        if (oldSegments.Contains(id)) {
                            movedIDs.Add(id);
                        } else {
                            newSegments.Add(id);
                        }
                    }

                    currentID = allSegments[currentID].m_nextGridSegment;
                }
            }

            movedSegments = new(movedIDs.ToArray());
        }

        private void ScanBuildings() {
            oldBuildings = new(newBuildings.m_buffer);
            movedIDs.ClearAndEnsureCapacity(newBuildings.m_size);
            newBuildings.Clear();

            ref int[] cells = ref visibleCells.m_buffer;

            for (var i = 0; i < visibleCells.m_size; i++) {

                ushort currentID = buildingGrid[cells[i]];

                while (currentID != 0) {
                    if ((allBuildings[currentID].m_flags & BUILDING_FLAGS) == Building.Flags.Created) {
                        id.Building = currentID;

                        if (oldSegments.Contains(id)) {
                            movedIDs.Add(id);
                        } else {
                            newBuildings.Add(id);
                        }
                    }

                    currentID = allBuildings[currentID].m_nextGridBuilding;
                }
            }

            movedBuildings = new(movedIDs.ToArray());
        }

        private void BuildGrid_Fastest() {

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

                            visibleCells.Add(j * 270 + k);
                        }
                    }
                }
            }
        }

        private void BuildGrid_Quality(ref RenderManager.CameraInfo cameraInfo) {

            GetQualityMinMaxXZ(ref cameraInfo, out int minX, out int minZ, out int maxX, out int maxZ);

            for (int j = minZ; j <= maxZ; j++) {

                for (int k = minX; k <= maxX; k++) {

                    visibleCells.Add(j * 270 + k);
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "Readability.")]
        private void GetQualityMinMaxXZ(
            ref RenderManager.CameraInfo cameraInfo,
            out int minX, out int minZ,
            out int maxX, out int maxZ)
        {
            const float maxDistance = 2000f; // needs tuning

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
