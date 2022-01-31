using ColossalFramework;
using TrafficManager.API.Traffic.Enums;
using TrafficManager.Manager.Impl;

namespace TrafficManager.Util.Extensions {
    public static class VehicleExtensions {
        private static Vehicle[] _vehBuffer = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

        /// <summary>Returns a reference to the vehicle instance.</summary>
        /// <param name="vehicleId">The ID of the vehicle instance to obtain.</param>
        /// <returns>The vehicle instance.</returns>
        public static ref Vehicle ToVehicle(this ushort vehicleId) => ref _vehBuffer[vehicleId];

        /// <summary>
        /// Checks if the vehicle is Created, but not Deleted.
        /// </summary>
        /// <param name="vehicle">vehicle</param>
        /// <returns>True if the vehicle is valid, otherwise false.</returns>
        public static bool IsValid(this ref Vehicle vehicle) =>
            vehicle.m_flags.CheckFlags(
                required: Vehicle.Flags.Created,
                forbidden: Vehicle.Flags.Deleted);

        /// <summary>Queues the vehicle to be despawned.</summary>
        /// <param name="vehicle">The vehicle instance to despawn.</param>
        /// <remarks>
        /// Don't use to despawn large numbers of vehicles as it will
        /// spam SimulationManager with queued items; for many vehicles
        /// use <see cref="UtilityManager.DespawnVehicles(ExtVehicleType?)"/> instead.
        /// </remarks>
        // TODO: Would vehicle.Unspawn() be viable/better approach?
        public static void Despawn(this Vehicle vehicle) =>
            Singleton<SimulationManager>.instance.AddAction(() =>
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicle.Info.m_instanceID.Vehicle));

        /// <summary>Determines the <see cref="ExtVehicleType"/> for a vehicle.</summary>
        /// <param name="vehicle">The vehocle to inspect.</param>
        /// <returns>The extended vehicle type.</returns>
        public static ExtVehicleType ToExtVehicleType(this ref Vehicle vehicle) {
            var vehicleId = vehicle.Info.m_instanceID.Vehicle;
            var vehicleAI = vehicle.Info.m_vehicleAI;
            var emergency = vehicle.m_flags.IsFlagSet(Vehicle.Flags.Emergency2);

            var ret = ExtVehicleManager.Instance.DetermineVehicleTypeFromAIType(
                vehicleId,
                vehicleAI,
                emergency);

            return ret ?? ExtVehicleType.None;
        }

    }
}
