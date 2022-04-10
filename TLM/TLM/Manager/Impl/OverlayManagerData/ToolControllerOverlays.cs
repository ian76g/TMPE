namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.UI;

    public class ToolControllerOverlays {

        static ToolControllerOverlays() {
            Lookup = new(Settings.Count);

            foreach (var setting in Settings)
                Lookup.Add(setting.Tool, setting);
        }

        internal static Dictionary<Type, OverlayRenderSettings> Lookup;

        /// <summary>
        /// Seettings for automatic <see cref="ToolsModifierControl.toolController.CurrentTool"/> overlays.
        /// </summary>
        private static readonly ReadOnlyCollection<OverlayRenderSettings> Settings = new(new List<OverlayRenderSettings>() {
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Tool,
                Tool =
                    typeof(TrafficManagerTool),
                Culling =
                    OverlayCulling.Camera,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.TMPE,
                Filter =
                    RestrictedVehicles.All,
            },

            new OverlayRenderSettings {
                Context =
                    OverlayContext.Tool,
                Tool =
                    typeof(NetTool),
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupAwareness,
                Filter =
                    RestrictedVehicles.All,
            },

            new OverlayRenderSettings {
                Context =
                    OverlayContext.Tool,
                Tool =
                    typeof(BulldozeTool),
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupAwareness,
                Filter =
                    RestrictedVehicles.All,
            },

            new OverlayRenderSettings {
                Context =
                    OverlayContext.Tool,
                Tool =
                    typeof(BuildingTool),
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.VehicleRestrictions |
                    Overlays.ParkingRestrictions,
                Filter =
                    RestrictedVehicles.All,
            },

            new OverlayRenderSettings {
                Context =
                    OverlayContext.Tool,
                Tool =
                    typeof(TransportTool),
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupTransport,
                Filter =
                    RestrictedVehicles.Transport |
                    RestrictedVehicles.Taxis |
                    RestrictedVehicles.Cars,
            },
        });
    }
}
