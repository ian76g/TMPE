namespace TrafficManager.API.Traffic.Enums {

    // order defines priority, eg. custom > tool > info > none

    /// <summary>
    /// The context indicates what caused the overlay to be displayed.
    /// <list type="bullet">
    /// <item>
    /// <term>None</term> <description>No overlay (inactive mode)</description>
    /// </item>
    /// <item>
    /// <term>Info</term> <description>Autoamtic based on info view</description>
    /// </item>
    /// <item>
    /// <term>Tool</term> <description>Automeric based on tool controller</description>
    /// </item>
    /// <item>
    /// <term>Custom</term>
    /// <description>
    /// Either TMPE (toolbar, subtool, etc) or an external mod.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public enum OverlayContext {
        None = 0,
        Info = 1 << 0, // InfoManager.instance.CurrentMode
        Tool = 1 << 1, // ToolsModifierControl.toolController?.CurrentTool
        Custom = 1 << 2, // Custom state = eg. external mod or TMPE subtool
    }

}
