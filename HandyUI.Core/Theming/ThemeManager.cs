using HandyUI.Core.Enums;
using SkiaSharp;

namespace HandyUI.Core.Theming;

public sealed class ThemeManager
{
    private static readonly Lazy<ThemeManager> _instance = new(() => new ThemeManager());
    public static ThemeManager Instance => _instance.Value;

    private byte _baseR;
    private byte _baseG;
    private byte _baseB;
    private readonly Dictionary<int, SKColor> _toneCache = [];

    public event Action? ThemeChanged;

    private ThemeManager() { }

    public ThemeMode Mode { get; private set; } = ThemeMode.Unknown;

    public void SetTheme(byte r, byte g, byte b, ThemeMode mode)
    {
        _baseR = r;
        _baseG = g;
        _baseB = b;
        Mode = mode;
        _toneCache.Clear();
        ThemeChanged?.Invoke();
    }

    public SKColor GetTone(int tone)
    {
        if (_toneCache.TryGetValue(tone, out var cachedColor))
        {
            return cachedColor;
        }

        var (R, G, B) = ColorEngine.GetToneRgb(_baseR, _baseG, _baseB, tone);
        var skiaColor = new SKColor(R, G, B);
        _toneCache[tone] = skiaColor;
        return skiaColor;
    }

    public SKColor SurfacePrimary => GetTone(Mode == ThemeMode.Dark ? 900 : 50);
    public SKColor SurfaceSecondary => GetTone(Mode == ThemeMode.Dark ? 850 : 100);
    public SKColor SurfaceTertiary => GetTone(Mode == ThemeMode.Dark ? 800 : 150);

    public SKColor TextPrimary => GetTone(Mode == ThemeMode.Dark ? 50 : 900);
    public SKColor TextSecondary => GetTone(Mode == ThemeMode.Dark ? 300 : 700);
    public SKColor TextTertiary => GetTone(Mode == ThemeMode.Dark ? 500 : 500);
    public SKColor TextDisabled => GetTone(Mode == ThemeMode.Dark ? 700 : 300);

    public SKColor Accent => GetTone(500);
    public SKColor AccentHover => GetTone(Mode == ThemeMode.Dark ? 400 : 600);

    public SKColor Border => GetTone(Mode == ThemeMode.Dark ? 750 : 250);
    public SKColor DisabledSurface => GetTone(Mode == ThemeMode.Dark ? 850 : 150);
}