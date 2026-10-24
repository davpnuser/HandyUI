using HandyUI.Core.Enums;
using HandyUI.Core.Helper;
using SkiaSharp;

namespace HandyUI.Core.Theming;

public sealed class ThemeManager
{
    private static readonly Lazy<ThemeManager> _instance = new(() => new());
    public static ThemeManager Instance => _instance.Value;

    private readonly Lock _lock = new();
    private readonly Dictionary<int, SKColor> _toneCache = [];
    private readonly Dictionary<string, SKColor> _derivedCache = [];

    private byte _baseR = 0;
    private byte _baseG = 120;
    private byte _baseB = 215;

    public event Action? ThemeChanged;

    private ThemeManager() { }

    public ThemeMode Mode { get; private set; } = ThemeMode.Unknown;

    public void SetTheme(byte r, byte g, byte b, ThemeMode mode)
    {
        lock (_lock)
        {
            _baseR = r;
            _baseG = g;
            _baseB = b;
            Mode = mode;
            _toneCache.Clear();
            _derivedCache.Clear();
        }

        ThemeChanged?.Invoke();
    }

    public SKColor GetTone(int tone)
    {
        lock (_lock)
        {
            if (_toneCache.TryGetValue(tone, out var cachedColor))
                return cachedColor;

            var (r, g, b) = ColorEngine.GetToneRgb(_baseR, _baseG, _baseB, tone);
            var skiaColor = new SKColor(r, g, b);
            _toneCache[tone] = skiaColor;
            return skiaColor;
        }
    }

    public SKColor BackgroundPrimary => GetTone(Mode == ThemeMode.Dark ? 900 : 50);
    public SKColor BackgroundSecondary => GetTone(Mode == ThemeMode.Dark ? 800 : 100);
    public SKColor BackgroundTertiary => GetTone(Mode == ThemeMode.Dark ? 700 : 200);

    public SKColor SurfacePrimary => GetTone(Mode == ThemeMode.Dark ? 800 : 0);
    public SKColor SurfaceSecondary => GetTone(Mode == ThemeMode.Dark ? 700 : 50);
    public SKColor SurfaceTertiary => GetTone(Mode == ThemeMode.Dark ? 600 : 100);

    public SKColor TextPrimary => GetTone(Mode == ThemeMode.Dark ? 50 : 900);
    public SKColor TextSecondary => GetTone(Mode == ThemeMode.Dark ? 200 : 700);
    public SKColor TextTertiary => GetTone(Mode == ThemeMode.Dark ? 400 : 500);

    public SKColor TextDisabled
    {
        get
        {
            lock (_lock)
            {
                if (_derivedCache.TryGetValue(nameof(TextDisabled), out var cached))
                    return cached;

                var color = TextPrimary.WithTransparency(0.72f);
                _derivedCache[nameof(TextDisabled)] = color;
                return color;
            }
        }
    }

    public SKColor Accent => GetTone(500);
    public SKColor AccentHover => GetTone(Mode == ThemeMode.Dark ? 400 : 600);

    public SKColor Border => GetTone(Mode == ThemeMode.Dark ? 700 : 200);
    public SKColor DisabledSurface => GetTone(Mode == ThemeMode.Dark ? 800 : 100);

    public SKColor Shadow
    {
        get
        {
            lock (_lock)
            {
                if (_derivedCache.TryGetValue(nameof(Shadow), out var cached))
                    return cached;

                var color = Mode == ThemeMode.Dark
                    ? SKColors.Black.WithTransparency(0.62f)
                    : SKColors.Black.WithTransparency(0.15f);

                _derivedCache[nameof(Shadow)] = color;
                return color;
            }
        }
    }
}