namespace TrafficManager.Manager.Overlays.Layers {
    using TrafficManager.Util.Extensions;
    using TrafficManager.API.Attributes;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;
    using TrafficManager.Manager.Impl.OverlayManagerData;

    /// <summary>
    /// Renders basic text labels on-screen.
    /// </summary>
    internal class LabelLayer {

        private const float MAX_CAMERA_DISTANCE = 300f;
        private const float MAX_MOUSE_DISTANCE = 150f;

        private const int FONT_STEP_SIZE = 3;
        private const int FONT_MIN_SIZE = 6;
        private const int FONT_MAX_SIZE = 24;

        /// <summary>List of all current label overlays.</summary>
        private readonly FastList<LabelCache> cachedLabels;

        internal delegate bool CustomLabelFilter(ILabel label);

        static LabelLayer() {
            Instance = new LabelLayer();
        }

        internal LabelLayer() {
            cachedLabels = new();
        }

        internal static LabelLayer Instance { get; private set; }

        /// <summary>
        /// Add a single label. Not suitable for large batches
        /// due to method call overheads and incremental memalloc.
        /// </summary>
        /// <param name="label">The label to add.</param>
        [Spike("May cause lag spike if lots of labels added.")]
        internal void Add(ILabel label) =>
            cachedLabels.Add(new LabelCache(label));

        /// <summary>
        /// Preferred method of adding batches labels.
        /// </summary>
        /// <param name="labels">The list of labels to add.</param>
        [Spike("May cause lag spike if lots of labels added.")]
        internal void Add(FastList<ILabel> labels) {
            cachedLabels.EnsureCapacity(
                cachedLabels.m_size + labels.m_size);

            for (int idx = 0; idx < labels.m_size; idx++)
                cachedLabels.Add(new LabelCache(labels[idx]));
        }

        /// <summary>Clears all labels.</summary>
        /// <param name="trim">If <c>true</c>, trim excess.</param>
        [Cold("Does not harm performance.")]
        internal void Clear(bool trim = false) {
            cachedLabels.Clear();
            if (trim) cachedLabels.Trim();
        }

        /// <summary>Clear labels asspciated with overlay(s).</summary>
        /// <param name="overlay">Overlay(s) filter.</param>
        /// <param name="trim">If <c>true</c>, trim excess.</param>
        [Cold("Does not harm performance.")]
        internal void Clear(Overlays overlay, bool trim = false) {

            for (int idx = cachedLabels.m_size; idx --> 0;) {
                if ((cachedLabels[idx].Label.Overlay & overlay) != 0)
                    cachedLabels.RemoveAt(idx);
            }

            if (trim) cachedLabels.Trim();
        }

        /// <summary>Clear matching labels.</summary>
        /// <param name="filter">Filter function.</param>
        /// <param name="trim">If <c>true</c>, trim excess.</param>
        [Spike("May cause lag spike if label filter is slow.")]
        internal void Clear(CustomLabelFilter filter, bool trim = false) {
            for (int idx = 0; idx < cachedLabels.m_size; idx++) {
                var cache = cachedLabels[idx];
                if (filter(cache.Label)) cache.IsDirty = true;
            }

            if (trim) cachedLabels.Trim();
        }

        /// <summary>Marks all labels dirty, eg. camera change.</summary>
        [Spike("May cause lag spike if lots of labels are dirty.")]
        internal void Dirty() {
            for (int idx = 0; idx < cachedLabels.m_size; idx++)
                cachedLabels[idx].IsDirty = true;
        }

        /// <summary>
        /// Marks all labels from specified overlay(s) dirty.
        /// Use when overlay needs a refresh.
        /// </summary>
        /// <param name="overlay">Overlay(s) filter.</param>
        [Spike("May cause lag spike if lots of labels are dirty.")]
        internal void Dirty(Overlays overlay) {
            for (int idx = 0; idx < cachedLabels.m_size; idx++) {
                var cache = cachedLabels[idx];
                if ((cache.Label.Overlay & overlay) != 0) cache.IsDirty = true;
            }
        }

        /// <summary>
        /// Marks matching labels as dirty.
        /// Use to refresh specific labels.
        /// </summary>
        /// <param name="filter">Filter function.</param>
        [Spike("May cause lag spike if lots of labels are dirty.")]
        internal void Dirty(CustomLabelFilter filter) {
            for (int idx = 0; idx < cachedLabels.m_size; idx++) {
                var cache = cachedLabels[idx];
                if (filter(cache.Label)) cache.IsDirty = true;
            }
        }

        [Hot("Every frame while overlays active.")]
        [Spike("May cause lag spike if lots of labels are dirty.")]
        internal void Render(OverlayRenderSettings settings, OverlayRenderData data) {
            if (cachedLabels.m_size == 0)
                return;

            for (int idx = 0; idx < cachedLabels.m_size; idx++) {
                var cache = cachedLabels[idx];

                // if label is dirty, recalcuate render details and save
                if (cache.IsDirty && !RefreshCache(cache, data))
                    continue;

                if (!cache.IsHidden) {
                    // render label gui
                    GUI.Label(cache.Bounds, cache.Text, cache.Style);

                    // handle mouse ineraction if neccessary (infrequent)
                    bool hovered = cache.Bounds.Contains(data.MousePos);

                    if (hovered && cache.Label.IsInteractive()) {
                        cache.IsHovered = hovered;
                        cache.IsClicked = data.Clicked;
                    } else if (cache.IsHovered) {
                        cache.MouseOut();
                    }
                }
            }
        }

        [Hot("May be called multiple times per frame.")]
        [Spike("May cause lag spike if lots of labels are dirty.")]
        private bool RefreshCache(LabelCache cache, OverlayRenderData data) {
            var label = cache.Label;

            // get worldpos
            cache.WorldPos = label.GetWorldPos();

            // work out distance from camera
            float distance = (cache.WorldPos - data.CameraPos).magnitude;

            // skip if too far away
            // what if distance from mouse? would need clean way to check that
            // maybe two iterators - one for camera range, the other mouse range?
            if (distance > MAX_CAMERA_DISTANCE) {
                cache.HideAndMouseOut();
                return false;
            }

            // now check if it's on screen
            if (!cache.WorldPos.WorldToScreenPoint(out var screenPos)) {
                cache.HideAndMouseOut();
                return false;
            }

            // determine font size based on distance from camera.
            float zoom = 1.0f / distance * 150f;
            float fontSize = label.TextSize * zoom;

            // snap font to nearest 3px step, to font size texture atlases
            fontSize = Mathf.Floor(fontSize / FONT_STEP_SIZE) * FONT_STEP_SIZE;

            // apply style, constraining font to reasonable bounds
            cache.Style.fontSize = (int)Mathf.Clamp(fontSize, FONT_MIN_SIZE, FONT_MAX_SIZE);
            cache.Style.normal.textColor = label.TextColor;

            // determine label bounds rect (this is likely expensive - can we cache and scale?)
            cache.Text = label.Text;
            Vector2 size = cache.Style.CalcSize(new GUIContent(cache.Text));
            cache.Bounds = new(screenPos.x - (size.x / 2f), screenPos.y, size.x, size.y);

            cache.IsDirty = false;
            return true;
        }

        internal class LabelCache {
            internal LabelCache(ILabel label) {
                Label = label;
                IsDirty = true;
                isHovered = false;
                isClicked = false;
            }

            internal ILabel Label { get; private set; }

            internal string Text;

            internal bool IsDirty;
            internal bool IsHidden;

            internal void HideAndMouseOut() {
                IsDirty = false;
                IsHidden = true;
                MouseOut();
            }

            internal void MouseOut() {
                isHovered = false;
                isClicked = false;
            }

            internal Vector3 WorldPos;
            internal GUIStyle Style;
            internal Rect Bounds;

            private bool isHovered;
            internal bool IsHovered {
                get => isHovered;
                set {
                    if (isHovered == value) return;
                    isHovered = value;
                    Label.OnHover(isHovered);
                }
            }

            private bool isClicked;
            internal bool IsClicked {
                get => isClicked;
                set {
                    if (isClicked == value) return;
                    isClicked = value;
                    Label.OnClick(isClicked, isHovered);
                }
            }
        }
    }
}
