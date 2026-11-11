using HandyUI.Core.Data;

namespace HandyUI.Core.Helpers;

public static class Easing
{
    public static float Evaluate(float t, EaseType easeType)
    {
        t = Math.Clamp(t, 0f, 1f);
        return easeType switch
        {
            EaseType.Linear => t,
            EaseType.CubicIn => t * t * t,
            EaseType.CubicOut => 1f - MathF.Pow(1f - t, 3f),
            EaseType.CubicInOut => t < 0.5f ? 4f * t * t * t : 1f - (MathF.Pow((-2f * t) + 2f, 3f) / 2f),
            EaseType.SineIn => 1f - MathF.Cos(t * MathF.PI / 2f),
            EaseType.SineOut => MathF.Sin(t * MathF.PI / 2f),
            EaseType.SineInOut => -(MathF.Cos(MathF.PI * t) - 1f) / 2f,
            EaseType.QuartIn => t * t * t * t,
            EaseType.QuartOut => 1f - MathF.Pow(1f - t, 4f),
            EaseType.QuartInOut => t < 0.5f ? 8f * t * t * t * t : 1f - (MathF.Pow((-2f * t) + 2f, 4f) / 2f),
            _ => t
        };
    }
}
