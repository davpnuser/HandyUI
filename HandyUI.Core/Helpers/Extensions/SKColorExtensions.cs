using SkiaSharp;

namespace HandyUI.Core.Helpers.Extensions;

public static class SKColorExtensions
{
    public static SKColor WithOpacity(this SKColor color, float opacity)
        => new(color.Red, color.Green, color.Blue, (byte)(color.Alpha * opacity));

    public static SKColor WithTransparency(this SKColor color, float transparency)
        => color.WithOpacity(1.0f - transparency);
}