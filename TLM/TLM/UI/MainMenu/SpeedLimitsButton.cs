﻿namespace TrafficManager.UI.MainMenu {
    using System.Collections.Generic;
    using TrafficManager.State;
    using TrafficManager.State.Keybinds;
    using TrafficManager.U;
    using TrafficManager.Util;

    public class SpeedLimitsButton : BaseMenuToolModeButton {
        protected override ToolMode ToolMode => ToolMode.SpeedLimits;

        public override void SetupButtonSkin(AtlasBuilder futureAtlas) {
            // Button backround (from BackgroundPrefix) is provided by MainMenuPanel.Start
            this.Skin = ButtonSkin.CreateSimple(
                                      foregroundPrefix: "SpeedLimits",
                                      backgroundPrefix: UConst.MAINMENU_ROUND_BUTTON_BG)
                                  .CanHover(foreground: false)
                                  .CanActivate();
            this.Skin.UpdateAtlasBuilder(
                atlasBuilder: futureAtlas,
                spriteSize: new IntVector2(50));
        }

        protected override string U_OverrideTooltipText() => Translation.Menu.Get("Tooltip:Speed limits");

        public override KeybindSetting U_OverrideTooltipShortcutKey() => KeybindSettingsBase.SpeedLimitsTool;

        protected override bool IsVisible() => IsButtonEnabled();

        public static bool IsButtonEnabled() => Options.customSpeedLimitsEnabled;
    }
}
