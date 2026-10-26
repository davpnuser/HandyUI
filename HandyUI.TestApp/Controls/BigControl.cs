using HandyUI.Core.Base;
using HandyUI.Core.Helper;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

public class BigControl : BaseControl<BigControl>
{
    private readonly SKPaint _currentPaint = new();
    private readonly SKPaint _textPaint = new()
    {
        IsAntialias = true
    };

    private readonly SKFont _font = new()
    {
        Size = 13,
        Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal)
    };

    private SKRect[]? _cachedParts;
    private SKRect _lastBounds;

    private const int ColorAmount = 15;
    private const int ToneStep = 50;
    private const int ToneAmount = (900 / ToneStep) + 1;

    private static readonly string[] ColorNames =
    [
        "Primary Color",
        "Secondary Color",
        "Tertiary Color",
        "Text Primary Color ",
        "Text Secondary Color",
        "Text Tertiary Color",
        "Text Disabled Color",
        "Accent Color",
        "Accent Hover Color",
        "Border Color",
        "Disabled Surface Color",
        "Background Primary Color",
        "Background Secondary Color",
        "Background Tertiary Color",
        "Shadow Color"
    ];

    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return true;
    }

    public override void OnDraw(SKCanvas canvas)
    {
        if (_cachedParts == null || _lastBounds != Bounds)
        {
            _lastBounds = Bounds;

            var leftParts = Bounds.LeftHalf().DivideHorizontal(ColorAmount);
            var rightParts = Bounds.RightHalf().DivideHorizontal(ToneAmount);
            _cachedParts = [.. leftParts, .. rightParts];
        }

        for (var i = 0; i < ColorAmount; i++)
        {
            var color = i switch
            {
                0 => ThemeManager.Instance.SurfacePrimary,
                1 => ThemeManager.Instance.SurfaceSecondary,
                2 => ThemeManager.Instance.SurfaceTertiary,
                3 => ThemeManager.Instance.TextPrimary,
                4 => ThemeManager.Instance.TextSecondary,
                5 => ThemeManager.Instance.TextTertiary,
                6 => ThemeManager.Instance.TextDisabled,
                7 => ThemeManager.Instance.Accent,
                8 => ThemeManager.Instance.AccentHover,
                9 => ThemeManager.Instance.Border,
                10 => ThemeManager.Instance.DisabledSurface,
                11 => ThemeManager.Instance.BackgroundPrimary,
                12 => ThemeManager.Instance.BackgroundSecondary,
                13 => ThemeManager.Instance.BackgroundTertiary,
                14 => ThemeManager.Instance.Shadow,
                _ => ThemeManager.Instance.SurfacePrimary
            };

            DrawSwatch(canvas, _cachedParts[i], color, ColorNames[i]);
        }

        for (var i = 0; i < ToneAmount; i++)
        {
            var tone = i * ToneStep;
            var color = ThemeManager.Instance.GetTone(tone);

            var accentTone = tone == 500 ? "(System Accent)" : string.Empty;
            DrawSwatch(canvas, _cachedParts[ColorAmount + i], color, $"Tone {tone} {accentTone}");
        }
    }

    private void DrawSwatch(SKCanvas canvas, SKRect part, SKColor color, string name)
    {
        _currentPaint.Color = color;
        canvas.DrawRect(part, _currentPaint);

        var luminance = ((0.299f * color.Red) + (0.587f * color.Green) + (0.114f * color.Blue)) / 255f;
        _textPaint.Color = luminance > 0.5f ? SKColors.Black : SKColors.White;

        var label = color.Alpha == 255
            ? $"{name} (#{color.Red:X2}{color.Green:X2}{color.Blue:X2})"
            : $"{name} (#{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2})";

        _font.MeasureText(label, out var textBounds);
        var x = part.Left + 12;
        var y = part.MidY - textBounds.MidY;

        canvas.DrawText(label, x, y, SKTextAlign.Left, _font, _textPaint);
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition)
    {
    }

    protected override void OnDispose()
    {
        _currentPaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        _cachedParts = null;
        base.OnDispose();
    }
}