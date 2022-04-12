namespace TrafficManager.Manager.Impl.OverlayManagerData {
    using UnityEngine;

    internal struct OverlayRenderData {
        /// <summary>The position of camera, in screen space.</summary>
        internal Vector3 CameraPos;

        /// <summary>The position of mouse, in screen space.</summary>
        internal Vector2 MousePos;

        /// <summary>Will be <c>true</c> if primary mouse button pressed.</summary>
        internal bool Clicked;

        /// <summary>Delta mouse scroll amount.</summary>
        internal int Scrolled; // todo
    }
}
