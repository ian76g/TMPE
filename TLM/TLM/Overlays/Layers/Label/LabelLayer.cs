namespace TrafficManager.Manager.Overlays.Layers {
    using TrafficManager.Util.Extensions;
    using TrafficManager.API.Attributes;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;
    using TrafficManager.Manager.Impl.OverlayManagerData;
    using TrafficManager.Overlays.Layers;
    using System.Collections.Generic;

    /// <summary>
    /// Renders basic text labels on-screen.
    /// </summary>
    internal class LabelLayer
        : ILayer
    {
        private const float MAX_CAMERA_DISTANCE = 300f;
        private const float MAX_MOUSE_DISTANCE = 150f;

        private const int FONT_STEP_SIZE = 3;
        private const int FONT_MIN_SIZE = 6;
        private const int FONT_MAX_SIZE = 24;

        // TODO: Find some way to load balance across layers
        private const int MAX_UPDATE = 50;

        private readonly FastList<Task> tasks;
        private readonly FastList<int> slots; // indexes of 'deleted' slots in tasks list

        /* Layer instance */

        internal static LabelLayer Instance { get; private set; }

        static LabelLayer() {
            Instance = new LabelLayer();
        }

        internal LabelLayer() {
            tasks = new();
        }

        /* Layer state */

        public LayerState State { get; private set; }

        public bool HasTasks =>
            tasks.Count() - slots.Count() > 0;

        /* Add tasks */

        public void MakeRoomFor(int numItems) =>
            tasks.MakeRoomFor(numItems - slots.Count()); // -ve ignored

        public void Add(ILabel label) {
            if (slots.m_size > 0) {
                tasks[slots.Pop()] = Task.For(label);
            } else {
                tasks.Add(Task.For(label));
            }
            State |= LayerState.NeedsUpdate;
        }

        /* Remove all tasks */

        public void Clear() {
            tasks.Clear();
            slots.Clear();

            State &= ~LayerState.AllNeeds;
        }

        public void Release() {
            tasks.Release();
            slots.Release();

            State &= ~LayerState.AllNeeds;
        }

        /* Reposition tasks - task will be hidden until repositioned */

        public void QueueReposition(InstanceID id) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.ID == id) {
                    State |= task.RequiresReposition(ref tasks.m_buffer, i);
                    // don't break
                }
            }
        }

        public void QueueReposition(HashSet<InstanceID> hash, bool deleteOthers = false) {
            if (deleteOthers) {
                slots.EnsureCapacity(tasks.Count());
            }
            for (int i = 0; i < tasks.m_size; i++) {
                ref var task = ref tasks.m_buffer[i];
                if (hash.Contains(task.ID)) {
                    State |= task.RequiresReposition(ref tasks.m_buffer, i);
                } else if (deleteOthers) {
                    task.Delete(ref tasks.m_buffer, i, slots);
                }
            }

            if (!HasTasks) Clear();
        }

        public void QueueReposition(Overlays overlay) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if ((task.Label.Overlay & overlay) != 0) {
                    State |= task.RequiresReposition(ref tasks.m_buffer, i);
                }
            }
        }

        public void QueueReposition(ILabel label) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.Label == label) {
                    State |= task.RequiresReposition(ref tasks.m_buffer, i);
                    break;
                }
            }
        }

        [Spike]
        public void Reposition(ref OverlayState state) {
            State &= ~LayerState.NeedsReposition;

            for (int i = 0; i < tasks.m_size; i++) {
                ref var task = ref tasks.m_buffer[i];

                if (!task.NeedsReposition) continue;

                State = task.Reposition(ref state, ref tasks.m_buffer, i)
                    ? LayerState.NeedsRender
                    : LayerState.NeedsReposition;
            }
        }

        /* Update tasks - task will be visible while waiting for update */

        internal void QueueUpdate(InstanceID id) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.ID == id) {
                    State |= task.RequiresUpdate(ref tasks.m_buffer, i);
                    // don't break
                }
            }
        }

        internal void QueueUpdate(HashSet<InstanceID> hash) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (hash.Contains(task.ID)) {
                    State |= task.RequiresUpdate(ref tasks.m_buffer, i);
                }
            }
        }

        internal void QueueUpdate(Overlays overlay) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if ((task.Label.Overlay & overlay) != 0) {
                    State |= task.RequiresUpdate(ref tasks.m_buffer, i);
                }
            }
        }

        internal void QueueUpdate(ILabel label) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.Label == label) {
                    State |= task.RequiresUpdate(ref tasks.m_buffer, i);
                    break;
                }
            }
        }

        [Spike]
        public void Update(ref OverlayState state) {
            State &= ~LayerState.NeedsUpdate;

            int numUpdate = 0;

            for (int i = 0; i < tasks.m_size; i++) {
                ref var task = ref tasks.m_buffer[i];

                if (!task.NeedsUpdate) continue;

                if (++numUpdate > MAX_UPDATE) {
                    State |= LayerState.NeedsUpdate;
                    return;
                }

                State |= task.Update(ref state, ref tasks.m_buffer, i)
                    ? LayerState.NeedsRender
                    : LayerState.NeedsUpdate;
            }
        }

        /* Hide tasks - task will remain hidden until told otherwise */

        internal void Hide(InstanceID id) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.ID == id) {
                    task.Hide(ref tasks.m_buffer, i);
                    // don't break
                }
            }
        }

        internal void Hide(HashSet<InstanceID> hash) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (hash.Contains(task.ID)) {
                    task.Hide(ref tasks.m_buffer, i);
                }
            }
        }

        internal void Hide(Overlays overlay) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if ((task.Label.Overlay & overlay) != 0) {
                    task.Hide(ref tasks.m_buffer, i);
                }
            }
        }

        internal void Hide(ILabel label) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.Label == label) {
                    task.Hide(ref tasks.m_buffer, i);
                    break;
                }
            }
        }

        /* Delete tasks */

        internal void Delete(InstanceID id) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.ID == id) {
                    task.Delete(ref tasks.m_buffer, i, slots);
                    // don't break
                }
            }

            if (!HasTasks) Clear();
        }

        internal void Delete(HashSet<InstanceID> hash) {
            slots.MakeRoomFor(hash.Count);

            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (hash.Contains(task.ID)) {
                    task.Delete(ref tasks.m_buffer, i, slots);
                }
            }

            if (!HasTasks) Clear();
        }

        internal void Delete(Overlays overlay) {
            slots.EnsureCapacity(tasks.Count());

            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if ((task.Label.Overlay & overlay) != 0) {
                    task.Delete(ref tasks.m_buffer, i, slots);
                }
            }

            if (!HasTasks) Clear();
        }

        internal void Delete(ILabel label) {
            for (int i = 0; i < tasks.Count(); i++) {
                ref var task = ref tasks.m_buffer[i];
                if (task.Label == label) {
                    task.Delete(ref tasks.m_buffer, i, slots);
                    break;
                }
            }

            if (!HasTasks) Clear();
        }

        [Hot]
        public void Render(ref OverlayConfig config, ref OverlayState state) {
            State &= ~LayerState.NeedsRender;

            for (int i = 0; i < tasks.m_size; i++) {
                ref var task = ref tasks.m_buffer[i];

                if (!task.NeedsRender) continue;

                task.Render();

                State |= LayerState.NeedsRender;
            }
        }

        [Hot]
        internal void InteractTasks(ref OverlayConfig config, ref OverlayState state) {

            for (int i = 0; i < tasks.m_size; i++) {
                ref var task = ref tasks.m_buffer[i];

                if (!task.NeedsRender) continue;

                bool hovered = task.Bounds.Contains(state.MousePos);

                if (hovered && task.Label.IsInteractive) {
                    ProcessTaskState(
                        task.Interact(hovered, ref state, ref tasks.m_buffer, i, slots));
                } else if (task.Hovered) {
                    ProcessTaskState(
                        task.Interact(false, ref state, ref tasks.m_buffer, i, slots));
                }
            }
        }

        private void ProcessTaskState(TaskState? taskState) {
            if (!taskState.HasValue || taskState.Value == 0) return;

            if ((taskState.Value & TaskState.NeedsReposition) != 0)
                State |= LayerState.NeedsReposition;

            if ((taskState.Value & TaskState.NeedsUpdate) != 0)
                State |= LayerState.NeedsUpdate;
        }

        /* Render Task: */

        private struct Task {
            internal TaskState State => taskState_;
            internal InstanceID ID { get; private set; }
            internal ILabel Label { get; private set; }
            internal Vector3 WorldPos { get; private set; }
            internal Vector3 ScreenPos { get; private set; }
            internal Rect Bounds { get; private set; }

            private TaskState taskState_;
            private string text_;
            private byte textSize_;
            private GUIStyle style_;

            [Spike]
            internal static Task For(ILabel label) => new () {
                ID = label.ID,
                Label = label,
                text_ = null,
                style_ = new(),
                taskState_ = TaskState.NeedsUpdate | TaskState.Hidden,
            };

            internal bool Deleted =>
                taskState_ == 0;

            [Hot]
            internal bool EveryFramne =>
                (taskState_ & TaskState.EveryFrame) != 0;

            internal bool NeedsReposition =>
                taskState_ != 0 &&
                (taskState_ & TaskState.NeedsReposition | TaskState.Hidden) == TaskState.NeedsReposition;

            internal bool NeedsUpdate =>
                taskState_ != 0 &&
                (taskState_ & TaskState.NeedsUpdate | TaskState.NeedsReposition) == TaskState.NeedsUpdate;

            [Hot]
            internal bool NeedsRender =>
                taskState_ != 0 &&
                (taskState_ & TaskState.Hidden | TaskState.NeedsReposition) == 0;

            internal bool Hovered =>
                (taskState_ & TaskState.Hovered) != 0;

            internal bool Clicked =>
                (taskState_ & TaskState.Clicked) != 0;

            internal bool Hidden =>
                taskState_ == 0 || (taskState_ & TaskState.Hidden) != 0;

            internal void Delete(ref Task[] tasks, int i, FastList<int> slots) {
                taskState_ = 0;
                tasks[i] = this;
                slots.Add(i);
            }

            [Spike]
            internal LayerState RequiresReposition(ref Task[] tasks, int i) {
                if (taskState_ == 0) return LayerState.None;
                taskState_ |= TaskState.NeedsReposition;
                tasks[i] = this;
                return LayerState.NeedsReposition;
            }

            [Spike]
            internal LayerState RequiresUpdate(ref Task[] tasks, int i) {
                if (taskState_ == 0) return LayerState.None;
                taskState_ |= TaskState.NeedsUpdate;
                tasks[i] = this;
                return LayerState.NeedsUpdate;
            }

            internal void Hide(ref Task[] tasks, int i) {
                if (taskState_ == 0) return;
                taskState_ |= TaskState.Hidden;
                tasks[i] = this;
            }

            [Hot]
            internal void Render() => GUI.Label(Bounds, text_, style_);

            [Spike]
            internal bool Reposition(ref OverlayState state, ref Task[] tasks, int i) {
                // process world pos
                float distance = (WorldPos - state.CameraPos).magnitude;
                if (distance > MAX_CAMERA_DISTANCE || !WorldPos.IsOnScreen(out var screenPos)) {
                    // unfinished interactions?
                    if ((taskState_ & TaskState.HoveredOrClicked) != 0)
                        Interact(false, ref state);

                    return false;
                }
                ScreenPos = screenPos;

                // scale font size
                float zoom = 1.0f / distance * 150f;
                float fontSize = textSize_ * zoom;
                fontSize = Mathf.Floor(fontSize / FONT_STEP_SIZE) * FONT_STEP_SIZE;
                style_.fontSize = (int)Mathf.Clamp(fontSize, FONT_MIN_SIZE, FONT_MAX_SIZE);

                // calc bounds box
                Vector2 size = style_.CalcSize(new GUIContent(text_));
                Bounds = new(screenPos.x - (size.x / 2f), screenPos.y, size.x, size.y);

                // save
                taskState_ &= ~TaskState.NeedsReposition;
                tasks[i] = this;

                return true;
            }

            [Spike]
            internal bool Update(ref OverlayState state, ref Task[] tasks, int i) {
                // get worldpos from label
                WorldPos = Label.WorldPos;

                // process world pos
                float distance = (WorldPos - state.CameraPos).magnitude;
                if (distance > MAX_CAMERA_DISTANCE || !WorldPos.IsOnScreen(out var screenPos)) {
                    // unfinished interactions?
                    if ((taskState_ & TaskState.HoveredOrClicked) != 0)
                        Interact(false, ref state);

                    return false;
                }
                ScreenPos = screenPos;

                // first tiem update for this task?
                if (string.IsNullOrEmpty(text_)) {
                    style_.alignment = TextAnchor.MiddleCenter;
                    taskState_ &= ~TaskState.Hidden;
                }

                // get remaining data from label
                text_ = Label.GetText(Hovered, ref state);
                textSize_ = Label.TextSize;
                style_.normal.textColor = Label.TextColor;

                // scale font size
                float zoom = 1.0f / distance * 150f;
                float fontSize = textSize_ * zoom;
                fontSize = Mathf.Floor(fontSize / FONT_STEP_SIZE) * FONT_STEP_SIZE;
                style_.fontSize = (int)Mathf.Clamp(fontSize, FONT_MIN_SIZE, FONT_MAX_SIZE);

                // calc bounds box
                Vector2 size = style_.CalcSize(new GUIContent(text_));
                Bounds = new(screenPos.x - (size.x / 2f), screenPos.y, size.x, size.y);

                // save
                taskState_ &= ~TaskState.NeedsUpdate;
                tasks[i] = this;

                return true;
            }

            internal TaskState? Interact(bool mouseInside, ref OverlayState state, ref Task[] tasks, int i, FastList<int> slots) {
                bool save = false;

                if (Hovered != mouseInside) {
                    save = true;

                    if (mouseInside) {
                        taskState_ |= TaskState.Hovered;
                    } else {
                        taskState_ &= ~TaskState.Hovered;
                    }

                    ProcessResponse(Label.OnHover(mouseInside, ref state));
                }

                if ((Clicked != (state.Primary || state.Secondary)) || (Clicked && !mouseInside)) {
                    save = true;

                    if (mouseInside && (state.Primary || state.Secondary)) {
                        taskState_ |= TaskState.Clicked;
                    } else {
                        taskState_ &= ~TaskState.Clicked;
                    }

                    ProcessResponse(Label.OnClick(mouseInside, ref state));
                }

                if (save) tasks[i] = this;

                if (taskState_ == 0) slots.Add(i);

                return taskState_;
            }

            private void ProcessResponse(TaskState? response) {
                if (!response.HasValue) return;

                response = response.Value & ~TaskState.HoveredOrClicked;

                if (response.Value == 0) {
                    taskState_ = 0; // delete
                    return;
                }

                taskState_ |= response.Value;
            }
        }
    }
}
