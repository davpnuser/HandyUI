using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Enums;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextButton : UIControlBase
{
    private SKPoint _padding = new(12f, 6f);
    private SKTypeface? _cachedTypeface;
    private bool _colorsInitialized;

    private bool _customNormalBgSet;
    private bool _customHoverBgSet;
    private bool _customPressedBgSet;
    private bool _customNormalBorderSet;
    private bool _customActiveBorderSet;
    private bool _customTextSet;
    private bool _customLightTextSet;

    private readonly SKPaint _bgPaint = new() { Style = SKPaintStyle.Fill, IsAntialias = false };
    private readonly SKPaint _borderPaint = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 1.0f, IsAntialias = false };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };

    public TextButton()
    {
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
            if (!_customNormalBgSet) NormalBackgroundColor = value.BackgroundColor;
            if (!_customHoverBgSet) HoverBackgroundColor = value.SemiActiveColor;
            if (!_customPressedBgSet) PressedBackgroundColor = value.ActiveColor;
            if (!_customNormalBorderSet) NormalBorderColor = value.BorderColor;
            if (!_customActiveBorderSet) ActiveBorderColor = value.ActiveColor;
            if (!_customTextSet) TextColor = value.TextColor;
            if (!_customLightTextSet) LightTextColor = value.LightTextColor;
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

    public bool AutoSize
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            RecalculateBounds();
        }
    } = false;

    public SKPoint Padding
    {
        get => _padding;
        set
        {
            if (Math.Abs(_padding.X - value.X) < 0.001f && Math.Abs(_padding.Y - value.Y) < 0.001f) return;
            _padding = value;
            RecalculateBounds();
        }
    }

    public string Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = "Button";

    public SKColor TextColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customTextSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().TextColor;

    public SKColor LightTextColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customLightTextSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().LightTextColor;

    public float TextSize
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = 13.0f;

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

    public SKColor NormalBackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customNormalBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BackgroundColor;

    public SKColor HoverBackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customHoverBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().SemiActiveColor;

    public SKColor PressedBackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customPressedBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().ActiveColor;

    public SKColor NormalBorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customNormalBorderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BorderColor;

    public SKColor ActiveBorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customActiveBorderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().ActiveColor;

    public float BorderThickness
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = 1.0f;

    public BorderDirection BorderDirection
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = BorderDirection.Inside;

    public event Action? Clicked;

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

        if (!IsEnabled)
        {
            _bgPaint.Color = Theme.DarkerBackgroundColor;
            _borderPaint.Color = Theme.DisabledColor;
            _textPaint.Color = Theme.DisabledTextColor;
        }
        else
        {
            if (IsMouseDown && IsHovered)
            {
                _bgPaint.Color = PressedBackgroundColor;
                _borderPaint.Color = ActiveBorderColor;
                _textPaint.Color = LightTextColor;
            }
            else if (IsHovered)
            {
                _bgPaint.Color = HoverBackgroundColor;
                _borderPaint.Color = ActiveBorderColor;
                _textPaint.Color = TextColor;
            }
            else
            {
                _bgPaint.Color = NormalBackgroundColor;
                _borderPaint.Color = NormalBorderColor;
                _textPaint.Color = TextColor;
            }

            _borderPaint.StrokeWidth = BorderThickness;
        }
    }

    public bool RecalculateBounds()
    {
        if (!AutoSize || string.IsNullOrEmpty(Text)) return false;

        using var font = new SKFont(GetEffectiveTypeface(), TextSize);
        font.MeasureText(Text, out var textBounds);

        var targetWidth = textBounds.Width + (Padding.X * 2f);
        var targetHeight = font.Metrics.CapHeight + (Padding.Y * 2f);

        if (Math.Abs(Width - targetWidth) < 0.001f && Math.Abs(Height - targetHeight) < 0.001f)
            return false;

        Width = targetWidth;
        Height = targetHeight;
        return true;
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint.X, clientPoint.Y);
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            Clicked?.Invoke();
            return true;
        }
        return false;
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!_colorsInitialized)
            UpdateBrushes();
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        UpdateBrushes();

        canvas.DrawRect(Bounds, _bgPaint);

        if (BorderThickness > 0)
        {
            var borderRect = BorderDirection switch
            {
                BorderDirection.Inside => SKRect.Create(
                    Bounds.Left + (BorderThickness / 2f),
                    Bounds.Top + (BorderThickness / 2f),
                    Bounds.Width - BorderThickness,
                    Bounds.Height - BorderThickness),
                BorderDirection.Outside => SKRect.Create(
                    Bounds.Left - (BorderThickness / 2f),
                    Bounds.Top - (BorderThickness / 2f),
                    Bounds.Width + BorderThickness,
                    Bounds.Height + BorderThickness),
                _ => Bounds
            };

            canvas.DrawRect(borderRect, _borderPaint);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(GetEffectiveTypeface(), TextSize);

            var x = Bounds.MidX;
            var y = Bounds.MidY + (font.Metrics.CapHeight / 2f);

            canvas.DrawText(Text, x, y, SKTextAlign.Center, font, _textPaint);
        }
    }

    protected override void OnDispose()
    {
        InvalidateTypeface();
        _bgPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        base.OnDispose();
    }

    public TextButton WithIsEnabled(bool isEnabled) { IsEnabled = isEnabled; return this; }
    public TextButton WithTheme(ThemeRecord theme) { Theme = theme; return this; }
    public TextButton WithWidth(float width) { Width = width; return this; }
    public TextButton WithHeight(float height) { Height = height; return this; }
    public TextButton WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public TextButton WithAutoSize(bool autoSize) { AutoSize = autoSize; return this; }
    public TextButton WithPadding(float x, float y) { Padding = new SKPoint(x, y); return this; }
    public TextButton WithText(string text) { Text = text; return this; }
    public TextButton WithTextSize(float size) { TextSize = size; return this; }
    public TextButton WithTextColor(SKColor color) { TextColor = color; return this; }
    public TextButton WithLightTextColor(SKColor color) { LightTextColor = color; return this; }
    public TextButton WithTypeface(SKTypeface? typeface) { Typeface = typeface; return this; }
    public TextButton WithFont(string family, float size = 13f, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        FontFamily = family;
        TextSize = size;
        FontWeight = weight;
        FontSlant = slant;
        return this;
    }
    public TextButton WithFontWeight(SKFontStyleWeight weight) { FontWeight = weight; return this; }
    public TextButton WithFontSlant(SKFontStyleSlant slant) { FontSlant = slant; return this; }
    public TextButton WithBorder(SKColor normalColor, SKColor activeColor, float thickness = 1.0f, BorderDirection direction = BorderDirection.Inside)
    {
        NormalBorderColor = normalColor;
        ActiveBorderColor = activeColor;
        BorderThickness = thickness;
        BorderDirection = direction;
        return this;
    }
    public TextButton WithBorderThickness(float thickness) { BorderThickness = thickness; return this; }
    public TextButton WithBorderDirection(BorderDirection direction) { BorderDirection = direction; return this; }
    public TextButton WithOnClick(Action onClick) { Clicked += onClick; return this; }
    public TextButton WithColors(SKColor normal, SKColor hover, SKColor pressed)
    {
        NormalBackgroundColor = normal;
        HoverBackgroundColor = hover;
        PressedBackgroundColor = pressed;
        return this;
    }
    public TextButton WithParent(UIControlBase? parent) { Parent = parent; return this; }
}