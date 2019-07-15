﻿using TrafficManager.State.Keybinds;

namespace TrafficManager.UI.MainMenu {
    public class ToggleTrafficLightsButton : MenuToolModeButton {
        public override ToolMode ToolMode => ToolMode.SwitchTrafficLight;
        public override ButtonFunction Function => ButtonFunction.ToggleTrafficLights;
        public override string Tooltip => "Switch_traffic_lights";
        public override bool Visible => true;
        public override KeybindSetting ShortcutKey => KeybindSettingsBase.ToggleTrafficLightTool;
    }
}