using SkiaSharp;

namespace HandyUI.Core.Classes.Records;

public record Theme
{
    public SKColor PrimaryColor = SKColor.Parse("#2563EB"); // Primary
    public SKColor SecondaryColor = SKColor.Parse("#3B82F6"); // Secondary
    public SKColor DisabledColor = SKColor.Parse("#E2E8F0"); // Disabled
    public SKColor TextColor = SKColor.Parse("#0F172A"); // Text
    public SKColor LightTextColor = SKColor.Parse("#FFFFFF"); // Light Text
    public SKColor DisabledTextColor = SKColor.Parse("#94A3B8"); // Disabled Text
    public SKColor BackgroundColor = SKColor.Parse("#F8FAFC"); // Background
    public SKColor DarkerBackgroundColor = SKColor.Parse("#FFFFFF"); // Darker Background
    public SKColor ActiveColor = SKColor.Parse("#2563EB"); // Active
    public SKColor SemiActiveColor = SKColor.Parse("#DBEAFE"); // Semi-Active
    public SKColor BorderColor = SKColor.Parse("#CBD5E1"); // Border

    public static Theme Default => new(); // Default property
}