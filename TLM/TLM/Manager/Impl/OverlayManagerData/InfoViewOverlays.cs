namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using TrafficManager.API.Traffic.Enums;
    using static InfoManager;

    public class InfoViewOverlays {

        static InfoViewOverlays() {
            Lookup = new(Settings.Count);

            foreach (var setting in Settings)
                Lookup.Add(setting.Info, setting);
        }

        internal static Dictionary<InfoMode, OverlayRenderSettings> Lookup;

        /// <summary>
        /// Seettings for automatic <see cref="InfoManager.instance.CurrentMode"/> overlays.
        /// </summary>
        private static readonly ReadOnlyCollection<OverlayRenderSettings> Settings = new(new List<OverlayRenderSettings>() {
            // Public Transport
            // https://github.com/CitiesSkylinesMods/TMPE/wiki/Public-Transport-Info-View
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Transport,
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

            // Natural Resources
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.NaturalResources,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupCargo,
                Filter =
                    RestrictedVehicles.Cargo,
            },

            // Outside Connections
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Connections,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupNetwork,
                Filter =
                    RestrictedVehicles.AllTypes |
                    RestrictedVehicles.AllPlaces,
            },

            // Traffic
            // https://github.com/CitiesSkylinesMods/TMPE/wiki/Traffic-Info-View
            // TODO: Don't show parking if Parking AI disabled?
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Traffic,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupTransport |
                    Overlays.GroupService |
                    Overlays.GroupCargo,
                Filter =
                    RestrictedVehicles.All,
            },

            // Garbage
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Garbage,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Services,
            },

            // Fire Safety
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.FireSafety,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Emergency,
            },

            // Entertainment
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Entertainment,
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

            // Road Maintenance - affects speed
            // https://github.com/CitiesSkylinesMods/TMPE/wiki/Road-Maintenance-Info-View
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Maintenance,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupNetwork,
                Filter =
                    RestrictedVehicles.Services,
            },

            // Snow - affects speed
            // https://github.com/CitiesSkylinesMods/TMPE/wiki/Snow-Info-View
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Snow,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupNetwork,
                Filter =
                    RestrictedVehicles.Services,
            },

            // Escape Routes
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.EscapeRoutes,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupTransport,
                Filter =
                    RestrictedVehicles.Transport |
                    RestrictedVehicles.Emergency |
                    RestrictedVehicles.Cars,
            },

            // Destruction
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Destruction,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Emergency,
            },

            // Disaster Detection (early warning)
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.DisasterDetection,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Emergency |
                    RestrictedVehicles.Transport,
            },

            // Disaster Hazard
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.DisasterHazard,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Emergency |
                    RestrictedVehicles.Transport,
            },

            // Traffic Routes - shows journey times (Real Time mod)
            // https://github.com/CitiesSkylinesMods/TMPE/wiki/Traffic-Routes-Info-View
            // TODO: Need to be careful re: Adjust Roads panel (bulk applicators)
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.TrafficRoutes,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupNetwork,
                Filter =
                    RestrictedVehicles.All,
            },

            // Underground view
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Underground,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.TMPE,
                Filter =
                    RestrictedVehicles.All,
            },

            // Tours
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Tours,
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

            // Tourism
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Tours,
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

            // Park Maintenance
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.ParkMaintenance,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Services,
            },

            // Post
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Post,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Services,
            },

            // Water
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Water,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Services,
            },

            // Crime Rate
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.CrimeRate,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupService,
                Filter =
                    RestrictedVehicles.Emergency,
            },

            // Industry
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Industry,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupCargo,
                Filter =
                    RestrictedVehicles.Cargo,
            },

            // Fishing
            new OverlayRenderSettings {
                Context =
                    OverlayContext.Info,
                Info =
                    InfoMode.Fishing,
                Culling =
                    OverlayCulling.Mouse,
                Interactive =
                    Overlays.None,
                Persistent =
                    Overlays.GroupCargo,
                Filter =
                    RestrictedVehicles.Cargo,
            },
        });
    }
}
