using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextButton : UIControlBase
{
    private SKPoint _padding = new(12f, 6f);

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
            Bounds = SKRect.Create(Location.X, Location.Y, value.Width, value.Height);
            Invalidate();
        }
    }

    public bool AutoSize
    {
        get;
        set { if (field == value) return; field = value; RecalculateBounds(); }
    } = false;

    public SKPoint Padding
    {
        get => _padding;
        set { if (_padding == value) return; _padding = value; RecalculateBounds(); }
    }

    public string Text
    {
        get;
        set { if (field == value) return; field = value; RecalculateBounds(); Invalidate(); }
    } = "Button";

    public SKColor TextColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.TextPrimary;

    public float TextSize
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; RecalculateBounds(); Invalidate(); }
    } = 13.0f;

    public string FontFamily
    {
        get;
        set { if (field == value) return; field = value; RecalculateBounds(); Invalidate(); }
    } = "Segoe UI";

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

    public event Action? Clicked;

    public void RecalculateBounds()
    {
        if (!AutoSize || string.IsNullOrEmpty(Text)) return;

        using var font = new SKFont(SKTypeface.FromFamilyName(FontFamily), TextSize);
        font.MeasureText(Text, out var textBounds);

        Width = textBounds.Width + (Padding.X * 2f);
        Height = font.Metrics.CapHeight + (Padding.Y * 2f);
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

            var halfBorder = BorderThickness / 2f;
            var insetBorderRect = SKRect.Create(
                Bounds.Left + halfBorder,
                Bounds.Top + halfBorder,
                Bounds.Width - BorderThickness,
                Bounds.Height - BorderThickness
            );

            canvas.DrawRect(insetBorderRect, borderPaint);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(SKTypeface.FromFamilyName(FontFamily), TextSize);
            using var textPaint = new SKPaint { Color = IsEnabled ? TextColor : VS2017Theme.TextDisabled, IsAntialias = true };

            var x = Bounds.MidX;
            var y = Bounds.MidY + (font.Metrics.CapHeight / 2f);

            canvas.DrawText(Text, x, y, SKTextAlign.Center, font, textPaint);
        }
    }

    public TextButton WithWidth(float width) { Width = width; return this; }
    public TextButton WithHeight(float height) { Height = height; return this; }
    public TextButton WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public TextButton WithAutoSize(bool autoSize) { AutoSize = autoSize; return this; }
    public TextButton WithPadding(float x, float y) { Padding = new SKPoint(x, y); return this; }
    public TextButton WithText(string text) { Text = text; return this; }
    public TextButton WithTextSize(float size) { TextSize = size; return this; }
    public TextButton WithTextColor(SKColor color) { TextColor = color; return this; }
    public TextButton WithOnClick(Action onClick) { Clicked += onClick; return this; }
    public TextButton WithColors(SKColor normal, SKColor hover, SKColor pressed)
    {
        NormalBackgroundColor = normal;
        HoverBackgroundColor = hover;
        PressedBackgroundColor = pressed;
        return this;
    }
    public TextButton WithBorderColors(SKColor normalBorder, SKColor activeBorder)
    {
        NormalBorderColor = normalBorder;
        ActiveBorderColor = activeBorder;
        return this;
    }
    public TextButton WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public TextButton WithParent(UIControlBase? parent) { Parent = parent; return this; }
}