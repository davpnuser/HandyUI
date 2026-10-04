using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Themes;

public static class DefaultTheme
{
    public static ThemeRecord GetTheme()
    {
        return new ThemeRecord(
            //SKColor.Parse("#2563EB"), // Primary ==> These colors are in Dark Theme.
            //SKColor.Parse("#60A5FA"), // Secondary
            //SKColor.Parse("#1E293B"), // Disabled
            //SKColor.Parse("#F8FAFC"), // Text
            //SKColor.Parse("#64748B"), // Disabled Text
            //SKColor.Parse("#131E37"), // Background
            //SKColor.Parse("#090C13"), // Darker Background
            //SKColor.Parse("#3B82F6"), // Active
            //SKColor.Parse("#1D4ED8"), // Semi-Active
            //SKColor.Parse("#3F719A")  // Border
            SKColor.Parse("#2563EB"), // Primary
            SKColor.Parse("#3B82F6"), // Secondary
            SKColor.Parse("#E2E8F0"), // Disabled
            SKColor.Parse("#0F172A"), // Text
            SKColor.Parse("#FFFFFF"), // Light Text
            SKColor.Parse("#94A3B8"), // Disabled Text
            SKColor.Parse("#F8FAFC"), // Background
            SKColor.Parse("#FFFFFF"), // Darker Background
            SKColor.Parse("#2563EB"), // Active
            SKColor.Parse("#DBEAFE"), // Semi-Active
            SKColor.Parse("#CBD5E1")  // Border
        );
    }
}