namespace TrafficManager.State {
    using ICities;
    using TrafficManager.UI;
    using TrafficManager.UI.Helpers;

    public static class PoliciesTab_InEmergenciesGroup {

        public static CheckboxOption EvacBussesMayIgnoreRules =
            new (nameof(Options.evacBussesMayIgnoreRules), Options.PersistTo.Savegame) {
                Label = "VR.Checkbox:Evacuation buses may ignore traffic rules",
                Validator = NaturalDisastersDlcValidator,
            };

        private static bool HasNaturalDisastersDLC
            => SteamHelper.IsDLCOwned(SteamHelper.DLC.NaturalDisastersDLC);

        internal static void AddUI(UIHelperBase tab) {
            if (!HasNaturalDisastersDLC) return;

            var group = tab.AddGroup(T("VR.Group:In case of emergency/disaster"));

            EvacBussesMayIgnoreRules.AddUI(group);
        }

        private static bool NaturalDisastersDlcValidator(bool desired, out bool result) {
            result = HasNaturalDisastersDLC && desired;
            return true;
        }

        private static string T(string key) => Translation.Options.Get(key);
    }
}