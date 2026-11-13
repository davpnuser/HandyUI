using SkiaSharp;

namespace HandyUI.Core.Helpers.Extensions;

public static class SKColorExtensions
{
    public static SKColor WithOpacity(this SKColor color, float opacity)
    {
        return color.WithAlpha((byte)(opacity * 255));
    }

    public static SKColor WithTransparency(this SKColor color, float transparency)
    {
        return color.WithOpacity(1.0f - transparency);
    }

    public static SKColor WithBrightness(this SKColor color, float brightness = 0.5f)
    {
        color.ToHsv(out var hue, out var saturation, out var value);

        var newValue = Math.Clamp(value * (brightness * 2f), 0f, 100f);

        return SKColor.FromHsv(hue, saturation, newValue, color.Alpha);
    }

    public static SKColor WithBrightnessHsl(this SKColor color, float brightness = 0.5f)
    {
        color.ToHsl(out var hue, out var saturation, out var lightness);

        var newLightness = brightness <= 0.5f
            ? lightness * (brightness * 2f)
            : lightness + ((100f - lightness) * ((brightness - 0.5f) * 2f));

        return SKColor.FromHsl(hue, saturation, newLightness);
    }

    public static SKColor Lerp(this SKColor c1, SKColor c2, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        var r = (byte)(c1.Red + ((c2.Red - c1.Red) * t) + 0.5f);
        var g = (byte)(c1.Green + ((c2.Green - c1.Green) * t) + 0.5f);
        var b = (byte)(c1.Blue + ((c2.Blue - c1.Blue) * t) + 0.5f);
        var a = (byte)(c1.Alpha + ((c2.Alpha - c1.Alpha) * t) + 0.5f);
        return new SKColor(r, g, b, a);
    }
}