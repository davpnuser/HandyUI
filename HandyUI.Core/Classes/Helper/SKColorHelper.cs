using SkiaSharp;

namespace HandyUI.Core.Classes.Helper;

public static class SKColorHelper
{
    #region Hue Methods

    public static SKColor FromHue(float hue)
    {
        return SKColor.FromHsl(hue, 100, 50);
    }

    public static SKColor FromHue(int hue)
    {
        return FromHue((float)hue);
    }

    #endregion

    #region Extension Methods

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

    public static SKColor WithTransparency(this SKColor color, float transparency = 0f)
    {
        return color.WithAlpha((byte)(255 - (transparency * 255)));
    }

    #endregion
}