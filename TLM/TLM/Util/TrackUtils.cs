namespace TrafficManager.Util {
    using TrafficManager.Util.Extensions;

    internal static class TrackUtils {
        internal const NetInfo.LaneType LANE_TYPES =
            NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;

        internal const VehicleInfo.VehicleType VEHICLE_TYPES =
            VehicleInfo.VehicleType.Metro |
            VehicleInfo.VehicleType.Train |
            VehicleInfo.VehicleType.Tram |
            VehicleInfo.VehicleType.Monorail;

        internal static bool IsTrackOnly(this NetInfo.Lane laneInfo) {
            return
                laneInfo != null &&
                laneInfo.m_laneType.IsFlagSet(LANE_TYPES) &&
                !laneInfo.m_laneType.IsFlagSet(~LANE_TYPES) &&
                laneInfo.m_vehicleType.IsFlagSet(VEHICLE_TYPES) &&
                !laneInfo.m_vehicleType.IsFlagSet(~VEHICLE_TYPES);
        }

        /// <summary>
        /// checks if vehicles move backward or bypass backward (considers LHT)
        /// </summary>
        /// <returns>true if vehicles move backward including AvoidForward,
        /// false if vehicles going forward, bi-directional, or non-directional</returns>
        internal static bool IsGoingBackward(this NetInfo.Direction direction) =>
            (direction & NetInfo.Direction.Both) == NetInfo.Direction.Backward ||
            (direction & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidForward;

        /// <summary>
        /// checks if vehicles move forward or bypass forward (considers LHT)
        /// </summary>
        /// <returns>true if vehicles move forward including AvoidBackward,
        /// false if vehicles going backward, bi-directional, or non-directional</returns>
        internal static bool IsGoingForward(this NetInfo.Direction direction) =>
        (direction & NetInfo.Direction.Both) == NetInfo.Direction.Forward ||
            (direction & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidBackward;

        /// <summary>
        /// checks if vehicles move backward or bypass backward (considers LHT)
        /// </summary>
        /// <returns>true if vehicles move backward including AvoidForward,
        /// false if vehicles going ward, bi-directional, or non-directional</returns>
        internal static bool IsGoingBackward(this NetInfo.Lane laneInfo) =>
            laneInfo.m_finalDirection.IsGoingBackward();

        /// <summary>
        /// checks if vehicles move forward or bypass forward (considers LHT)
        /// </summary>
        /// <returns>true if vehicles move forward including AvoidBackward,
        /// false if vehicles going backward, bi-directional, or non-directional</returns>
        internal static bool IsGoingForward(this NetInfo.Lane laneInfo) =>
            laneInfo.m_finalDirection.IsGoingForward();
    }
}
