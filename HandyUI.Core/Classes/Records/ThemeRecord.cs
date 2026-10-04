using SkiaSharp;

namespace HandyUI.Core.Classes.Records;

public record ThemeRecord
   (SKColor PrimaryColor,
    SKColor SecondaryColor,
    SKColor DisabledColor,
    SKColor TextColor,
    SKColor LightTextColor,
    SKColor DisabledTextColor,
    SKColor BackgroundColor,
    SKColor DarkerBackgroundColor,
    SKColor ActiveColor,
    SKColor SemiActiveColor,
    SKColor BorderColor);