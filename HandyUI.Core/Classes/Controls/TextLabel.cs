using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextLabel : UIControlBase<TextLabel>
{
    private SKTypeface? _cachedTypeface;
    private bool _colorsInitialized;
    private bool _customTextColorSet;

    private readonly SKPaint _textPaint = new() { IsAntialias = true };

    public TextLabel(string text = "")
    {
        Text = text;
        RecalculateBounds();
        UpdateBrushes();
    }

    public new bool IsEnabled
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = true;

    public ThemeRecord Theme
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            if (!_customTextColorSet) TextColor = value.TextColor;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme();

    public float Width
    {
        get => Bounds.Width;
        set
        {
            if (Math.Abs(Bounds.Width - value) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, value, Bounds.Height);
            Invalidate();
        }
    }

    public float Height
    {
        get => Bounds.Height;
        set
        {
            if (Math.Abs(Bounds.Height - value) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, Bounds.Width, value);
            Invalidate();
        }
    }

    public SKSize Size
    {
        get => new(Bounds.Width, Bounds.Height);
        set
        {
            if (Math.Abs(Bounds.Width - value.Width) < 0.001f && Math.Abs(Bounds.Height - value.Height) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, value.Width, value.Height);
            Invalidate();
        }
    }

    public string Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            if (!RecalculateBounds()) Invalidate();
        }
    } = string.Empty;

    public SKColor TextColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customTextColorSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().TextColor;

    public float TextSize
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = 16f;

    public SKTypeface? Typeface
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public string FontFamily
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            if (!RecalculateBounds()) Invalidate();
        }
    } = "Segoe UI";

    public SKFontStyleWeight FontWeight
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            if (!RecalculateBounds()) Invalidate();
        }
    } = SKFontStyleWeight.Normal;

    public SKFontStyleWidth FontWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            if (!RecalculateBounds()) Invalidate();
        }
    } = SKFontStyleWidth.Normal;

    public SKFontStyleSlant FontSlant
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            if (!RecalculateBounds()) Invalidate();
        }
    } = SKFontStyleSlant.Upright;

    private SKTypeface GetEffectiveTypeface()
    {
        return Typeface ?? GetOrCreateTypeface();
    }

    private SKTypeface GetOrCreateTypeface()
    {
        return _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);
    }

    private void InvalidateTypeface()
    {
        _cachedTypeface?.Dispose();
        _cachedTypeface = null;
    }

    private void UpdateBrushes()
    {
        _colorsInitialized = true;
        _textPaint.Color = IsEnabled ? TextColor : Theme.DisabledTextColor;
    }

    public bool RecalculateBounds()
    {
        if (string.IsNullOrEmpty(Text)) return false;

        using var font = new SKFont(GetEffectiveTypeface(), TextSize) { Subpixel = true };
        font.MeasureText(Text, out var textBounds);
        var metrics = font.Metrics;
        var targetHeight = metrics.Descent - metrics.Ascent;

        if (Math.Abs(Width - textBounds.Width) < 0.001f && Math.Abs(Height - targetHeight) < 0.001f)
            return false;

        Bounds = SKRect.Create(Location.X, Location.Y, textBounds.Width, targetHeight);
        return true;
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint.X, clientPoint.Y);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!_colorsInitialized)
            UpdateBrushes();
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible || string.IsNullOrEmpty(Text)) return;

        using var font = new SKFont(GetEffectiveTypeface(), TextSize) { Subpixel = true };
        var baselineY = Bounds.Top - font.Metrics.Ascent;
        canvas.DrawText(Text, Bounds.Left, baselineY, SKTextAlign.Left, font, _textPaint);
    }

    protected override void OnDispose()
    {
        InvalidateTypeface();
        _textPaint.Dispose();
        base.OnDispose();
    }

    public TextLabel WithIsEnabled(bool isEnabled) { IsEnabled = isEnabled; return this; }
    public TextLabel WithTheme(ThemeRecord theme) { Theme = theme; return this; }
    public TextLabel WithWidth(float width) { Width = width; return this; }
    public TextLabel WithHeight(float height) { Height = height; return this; }
    public TextLabel WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public TextLabel WithText(string text) { Text = text; return this; }
    public TextLabel WithTextColor(SKColor color) { TextColor = color; return this; }
    public TextLabel WithTextSize(float size) { TextSize = size; return this; }
    public TextLabel WithTypeface(SKTypeface? typeface) { Typeface = typeface; return this; }
    public TextLabel WithFont(string family, float size = 16f, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        FontFamily = family;
        TextSize = size;
        FontWeight = weight;
        FontSlant = slant;
        return this;
    }
    public TextLabel WithParent(UIControlBase? parent) { Parent = parent; return this; }
}