namespace TrafficManager.API.Traffic.Enums {
    using System;

    // draft - this will need refinement
    [Flags]
    public enum CacheTargets {
        None = 0,

        Nodes = 1 << 0,
        SegmentEnds = 1 << 1,
        Segments = 1 << 2,
        Lanes = 1 << 3,
        SegmentLanes = 1 << 4,
        Citizens = 1 << 5,
        Vehicles = 1 << 6,
        Buildings = 1 << 7,
        ParkingSpaces = 1 << 8,
    }
}
