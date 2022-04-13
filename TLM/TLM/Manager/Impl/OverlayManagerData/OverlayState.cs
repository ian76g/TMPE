namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;

    public struct OverlayState {
        /// <summary>The position of camera, in screen space.</summary>
        internal Vector3 CameraPos;

        internal RenderManager.CameraInfo CameraInfo; // ?

        /// <summary>The position of mouse, in screen space.</summary>
        internal Vector3 MousePos;

        /// <summary>Is <c>true</c> if primary mouse button pressed.</summary>
        internal bool Primary;

        /// <summary>Is <c>true</c> if secondary mouse button pressed.</summary>
        internal bool Secondary;

        /// <summary>Internal use for overlay layer managers.</summary>
        internal bool PriSec;

        /// <summary>Delta mouse scroll amount.</summary>
        internal int Wheel;

        internal bool Shift;

        internal bool Control;

        internal bool Alt;

        internal CacheTargets RefreshMapCache;
        internal Overlays RefreshOverlays;
    }
}
