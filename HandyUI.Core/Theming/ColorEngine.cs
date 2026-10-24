namespace HandyUI.Core.Theming;

public static class ColorEngine
{
    public static (byte R, byte G, byte B) GetToneRgb(float baseR, float baseG, float baseB, int requestedTone)
    {
        var tone = Math.Clamp(requestedTone, 50, 900);
        var (_, c, h) = RgbToOklch(baseR, baseG, baseB);

        var targetL = 0.98f - (tone / 900f * 0.83f);
        var targetC = (tone is <= 50 or >= 900) ? Math.Min(c, 0.05f) : c;

        return OklchToRgb(targetL, targetC, h);
    }

    private static (float L, float C, float H) RgbToOklch(float r, float g, float b)
    {
        var lr = SrgbToLinear(r / 255f);
        var lg = SrgbToLinear(g / 255f);
        var lb = SrgbToLinear(b / 255f);

        var l = (0.4122214708f * lr) + (0.5363325363f * lg) + (0.0514459929f * lb);
        var m = (0.2119034982f * lr) + (0.6806995451f * lg) + (0.1073969566f * lb);
        var s = (0.0883024619f * lr) + (0.2817188376f * lg) + (0.6299787005f * lb);

        var l_ = MathF.Cbrt(l);
        var m_ = MathF.Cbrt(m);
        var s_ = MathF.Cbrt(s);

        var L = (0.2104542553f * l_) + (0.7936177850f * m_) - (0.0040720468f * s_);
        var a = (1.9779984951f * l_) - (2.4285922050f * m_) + (0.4505937099f * s_);
        var bVal = (0.0259040371f * l_) + (0.7827717662f * m_) - (0.8086757660f * s_);

        var C = MathF.Sqrt((a * a) + (bVal * bVal));
        var H = MathF.Atan2(bVal, a) * (180f / MathF.PI);
        if (H < 0) H += 360f;

        return (L, C, H);
    }

    private static (byte R, byte G, byte B) OklchToRgb(float L, float C, float H)
    {
        var hRad = H * (MathF.PI / 180f);
        var a = C * MathF.Cos(hRad);
        var bVal = C * MathF.Sin(hRad);

        var l_ = L + (0.3963377774f * a) + (0.2158037573f * bVal);
        var m_ = L - (0.1055613458f * a) - (0.0638541728f * bVal);
        var s_ = L - (0.0894841775f * a) - (1.2914855480f * bVal);

        var l = l_ * l_ * l_;
        var m = m_ * m_ * m_;
        var s = s_ * s_ * s_;

        var lr = (+4.0767416621f * l) - (3.3077115913f * m) + (0.2309699292f * s);
        var lg = (-1.2684380046f * l) + (2.6097574011f * m) - (0.3413193965f * s);
        var lb = (-0.0041960863f * l) - (0.7034186147f * m) + (1.7076147010f * s);

        var red = (byte)((Math.Clamp(LinearToSrgb(lr), 0f, 1f) * 255f) + 0.5f);
        var green = (byte)((Math.Clamp(LinearToSrgb(lg), 0f, 1f) * 255f) + 0.5f);
        var blue = (byte)((Math.Clamp(LinearToSrgb(lb), 0f, 1f) * 255f) + 0.5f);

        return (red, green, blue);
    }

    private static float SrgbToLinear(float c)
    {
        return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    private static float LinearToSrgb(float c)
    {
        return c <= 0.0031308f ? c * 12.92f : (1.055f * MathF.Pow(c, 1f / 2.4f)) - 0.055f;
    }
}