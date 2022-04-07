namespace TrafficManager.API.Manager {
    public interface IOverlayManager {

        /// <summary>
        /// Turn on common persistent overlays in vicinity of
        /// mouse cursor.
        /// </summary>
        /// <returns>Returns <c>true</c> if successful, otherwise <c>false</c>.</returns>
        public bool TurnOn();

        //public bool TurnOn(Overlays overlays, OverlayCulling mode);

        /// <summary>
        /// Turn off all overlays.
        /// </summary>
        public void TurnOff();

        /// <summary>
        /// Returns <c>true</c> if any overlays active, otherwise <c>false</c>.
        /// </summary>
        public bool AnyOverlaysActive { get; }

        /// <summary>
        /// Returns <c>true</c> if persistent tunnels overlay is active.
        /// This means that underground overlays will be available in
        /// overground mode.
        /// </summary>
        public bool PersistentTunnels { get; }
    }
}
