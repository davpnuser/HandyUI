using SkiaSharp;

namespace HandyUI.Core.Classes.Themes;

public static class VS2017Theme
{
    public static SKColor Background { get; set; } = SKColor.Parse("#2D2D30");
    public static SKColor PanelBackground { get; set; } = SKColor.Parse("#1E1E1E");
    public static SKColor ControlBorder { get; set; } = SKColor.Parse("#3F3F46");

    // Buttons
    public static SKColor ButtonNormal { get; set; } = SKColor.Parse("#333337");
    public static SKColor ButtonHover { get; set; } = SKColor.Parse("#1C97EA");
    public static SKColor ButtonPressed { get; set; } = SKColor.Parse("#007ACC");

    // Text
    public static SKColor TextPrimary { get; set; } = SKColor.Parse("#F1F1F1");
    public static SKColor TextDisabled { get; set; } = SKColor.Parse("#656565");

    // Scrollbars
    public static SKColor ScrollTrack { get; set; } = SKColor.Parse("#1E1E1E");
    public static SKColor ScrollThumb { get; set; } = SKColor.Parse("#3E3E42");
    public static SKColor ScrollThumbHover { get; set; } = SKColor.Parse("#686868");
}