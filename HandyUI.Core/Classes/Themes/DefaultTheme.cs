using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Themes;

public static class DefaultTheme
{
    public static ThemeRecord GetTheme()
    {
        return new ThemeRecord(
            SKColor.Parse("#2563EB"), // Primary 
            SKColor.Parse("#60A5FA"), // Secondary
            SKColor.Parse("#1E293B"), // Disabled
            SKColor.Parse("#F8FAFC"), // Text
            SKColor.Parse("#64748B"), // Disabled Text
            SKColor.Parse("#131E37"), // Background
            SKColor.Parse("#090C13"), // Darker Background
            SKColor.Parse("#3B82F6"), // Active
            SKColor.Parse("#1D4ED8"), // Semi-Active
            SKColor.Parse("#3F719A")  // Border
        );
    }
}