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

        // TODO: Find some way to load balance across layers
        private const int MAX_REFRESH = 50;
        private int numRefreshes = 0;

        private readonly FastList<Task> tasks;

        internal static LabelLayer Instance { get; private set; }

        static LabelLayer() {
            Instance = new LabelLayer();
        }

        internal LabelLayer() {
            tasks = new();
        }

        internal void MakeRoomFor(int numItems) =>
            tasks.MakeRoomFor(numItems);

        internal void Add(ILabel label) => tasks.Add(Task.For(label));

        internal void Clear() => tasks.Clear();

        internal void Remove(Overlays overlay) {
            for (int i = tasks.Count(); i-- > 0;)
                if (tasks[i].Label.Overlay == overlay) tasks.RemoveAt(i);
        }

        internal void Invalidate() {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                task.IsDirty = true;
                task.Save(ref tasks.m_buffer, i);
            }
        }

        internal void Invalidate(Overlays overlay) {
            for (int i = 0; i < tasks.Count(); i++) {
                if (tasks[i].Label.Overlay == overlay) {
                    ref var task = ref tasks.m_buffer[i];
                    task.IsDirty = true;
                    task.Save(ref tasks.m_buffer, i);
                }
            }
        }

        [Hot]
        internal void Render(OverlayConfig config, OverlayState state) {
            if (tasks.m_size == 0) return;

            numRefreshes = 0;

            for (int i = 0; i < tasks.m_size; i++) {
                ref var task = ref tasks.m_buffer[i];

                if (task.IsHidden) continue;

                if (task.IsDirty &&
                    (numRefreshes++ > MAX_REFRESH || !task.Refresh(ref state))) {

                    continue;
                }

                task.Render();

                bool hovered = task.Bounds.Contains(state.MousePos);

                if (hovered && task.Label.IsInteractive) {
                    task.Interact(hovered, ref state);
                } else if (task.IsHovered) {
                    task.Interact(false, ref state);
                }

                if (task.Altered) task.Save(ref tasks.m_buffer, i);
            }
        }

        public void OnInfoViewChanged(ref OverlayConfig config, ref OverlayState state) =>
            Invalidate();


        private struct Task {
            private string text_;
            private GUIStyle style_;
            private bool isClicked;

            internal ILabel Label { get; private set; }

            internal Vector3 ScreenPos { get; private set; }
            internal Rect Bounds { get; private set; }

            internal bool Altered { get; private set; }
            internal bool IsHovered { get; private set; }
            internal bool IsHidden { get; private set; }

            internal bool IsDirty { get; set; }

            internal static Task For(ILabel label) => new() {
                Label = label,
                style_ = new(),
                IsDirty = true,
            };

            internal void Render() => GUI.Label(Bounds, text_, style_);

            [Hot][Spike]
            internal bool Refresh(ref OverlayState state) {
                var worldPos = Label.WorldPos;
                
                float distance = (worldPos - state.CameraPos).magnitude;

                if (distance > MAX_CAMERA_DISTANCE || !worldPos.IsOnScreen(out var screenPos)) {
                    //Hide(ref state);
                    return false;
                }

                ScreenPos = screenPos;

                float zoom = 1.0f / distance * 150f;
                float fontSize = Label.TextSize * zoom;

                fontSize = Mathf.Floor(fontSize / FONT_STEP_SIZE) * FONT_STEP_SIZE;

                style_.fontSize = (int)Mathf.Clamp(fontSize, FONT_MIN_SIZE, FONT_MAX_SIZE);
                style_.normal.textColor = Label.TextColor;

                text_ = Label.GetText(IsHovered, ref state);
                Vector2 size = style_.CalcSize(new GUIContent(text_));
                Bounds = new(screenPos.x - (size.x / 2f), screenPos.y, size.x, size.y);

                IsDirty = false;
                Altered = true;
                return true;
            }

            private void Hide(ref OverlayState state) {
                if (IsHovered || isClicked) Interact(false, ref state);

                IsDirty = false;
                IsHidden = true;
                Altered = true;
            }

            internal void Interact(bool mouseInside, ref OverlayState state) {
                if (IsHovered != mouseInside) {
                    IsHovered = mouseInside;
                    IsDirty |= Label.OnHover(mouseInside, ref state);
                    Altered = true;
                }
                if ((isClicked != state.Primary) || (isClicked && !mouseInside)) {
                    isClicked = mouseInside && state.Primary;
                    IsDirty |= Label.OnClick(mouseInside, ref state);
                    Altered = true;
                }
            }

            internal void Save(ref Task[] tasks, int i) {
                Altered = false;
                tasks[i] = this;
            }
        }
    }
}
