namespace TrafficManager.API.Traffic.Enums {

    // order defines priority, eg. custom > tool > info > none
    public enum OverlayContext {
        None = 0,
        Info = 1 << 0, // InfoManager.instance.CurrentMode
        Tool = 1 << 1, // ToolsModifierControl.toolController?.CurrentTool
        Custom = 1 << 2, // Custom state = eg. external mod or TMPE subtool
    }

}
