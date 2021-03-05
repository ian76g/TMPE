namespace TrafficManager.Manager.Impl {
    using API.Manager;
    using API.Manager.Connections;
    using Patch._VehicleAI.Connection;
    using Patch._VehicleAI._PassengerCarAI.Connection;
    using Patch._VehicleAI._TrainAI.Connection;

    internal class GameConnectionManager: IGameConnectionManager {

        internal static GameConnectionManager Instance;
        static GameConnectionManager() {
            Instance = new GameConnectionManager();
        }

        GameConnectionManager() {
            PassengerCarAIConnection = PassengerCarAIHook.GetConnection();
            VehicleAIConnection = VehicleAIHook.GetConnection();
            TrainAIConnection = TrainAIHook.GetConnection();
        }

        public IPassengerCarAIConnection PassengerCarAIConnection { get; }
        public IVehicleAIConnection VehicleAIConnection { get; }
        public ITrainAIConnection TrainAIConnection { get; }
    }
}