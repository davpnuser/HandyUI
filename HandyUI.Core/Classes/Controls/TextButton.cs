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
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.TextPrimary;

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
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ButtonNormal;

    public SKColor HoverBackgroundColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ButtonHover;

    public SKColor PressedBackgroundColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ButtonPressed;

    public SKColor NormalBorderColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ControlBorder;

    public SKColor ActiveBorderColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ButtonHover;

    public float BorderThickness
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 1.0f;

    public BorderDirection BorderDirection
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = BorderDirection.Inside;

    public event Action? Clicked;

    private SKTypeface GetOrCreateTypeface() => _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);

    private void InvalidateTypeface()
    {
        _cachedTypeface?.Dispose();
        _cachedTypeface = null;
    }

    public bool RecalculateBounds()
    {
        if (!AutoSize || string.IsNullOrEmpty(Text)) return false;

        using var font = new SKFont(GetOrCreateTypeface(), TextSize);
        font.MeasureText(Text, out var textBounds);

        var targetWidth = textBounds.Width + (Padding.X * 2f);
        var targetHeight = font.Metrics.CapHeight + (Padding.Y * 2f);

        if (Math.Abs(Width - targetWidth) < 0.001f && Math.Abs(Height - targetHeight) < 0.001f)
            return false;

        Width = targetWidth;
        Height = targetHeight;
        return true;
    }

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered)
        {
            Clicked?.Invoke();
            return true;
        }
        return false;
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition) { }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var currentBg = NormalBackgroundColor;
        var currentBorder = NormalBorderColor;

        if (IsMouseDown && IsHovered)
        {
            currentBg = PressedBackgroundColor;
            currentBorder = ActiveBorderColor;
        }
        else if (IsHovered)
        {
            currentBg = HoverBackgroundColor;
            currentBorder = ActiveBorderColor;
        }

        using var bgPaint = new SKPaint { Color = currentBg, Style = SKPaintStyle.Fill, IsAntialias = false };
        canvas.DrawRect(Bounds, bgPaint);

        if (BorderThickness > 0)
        {
            using var borderPaint = new SKPaint { Color = currentBorder, Style = SKPaintStyle.Stroke, StrokeWidth = BorderThickness, IsAntialias = false };

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

            canvas.DrawRect(borderRect, borderPaint);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(GetOrCreateTypeface(), TextSize);
            using var textPaint = new SKPaint { Color = IsEnabled ? TextColor : VS2017Theme.TextDisabled, IsAntialias = true };

            var x = Bounds.MidX;
            var y = Bounds.MidY + (font.Metrics.CapHeight / 2f);

            canvas.DrawText(Text, x, y, SKTextAlign.Center, font, textPaint);
        }
    }

    protected override void OnDispose()
    {
        InvalidateTypeface();
        base.OnDispose();
    }

    public TextButton WithWidth(float width) { Width = width; return this; }
    public TextButton WithHeight(float height) { Height = height; return this; }
    public TextButton WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public TextButton WithAutoSize(bool autoSize) { AutoSize = autoSize; return this; }
    public TextButton WithPadding(float x, float y) { Padding = new SKPoint(x, y); return this; }
    public TextButton WithText(string text) { Text = text; return this; }
    public TextButton WithTextSize(float size) { TextSize = size; return this; }
    public TextButton WithTextColor(SKColor color) { TextColor = color; return this; }
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
    public TextButton WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public TextButton WithParent(UIControlBase? parent) { Parent = parent; return this; }
}