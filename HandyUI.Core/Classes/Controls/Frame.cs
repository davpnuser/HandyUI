using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Enums;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Frame : UIControlBase
{
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

    public SKColor BackgroundColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.PanelBackground;

    public SKColor BorderColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ControlBorder;

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

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    public override void Update(float deltaTime, SKPoint clientMousePosition) { }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        if (BackgroundColor.Alpha > 0)
        {
            using var bgPaint = new SKPaint { Color = BackgroundColor, Style = SKPaintStyle.Fill, IsAntialias = false };
            canvas.DrawRect(Bounds, bgPaint);
        }

        if (BorderThickness > 0 && BorderColor.Alpha > 0)
        {
            using var borderPaint = new SKPaint { Color = BorderColor, Style = SKPaintStyle.Stroke, StrokeWidth = BorderThickness, IsAntialias = false };

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
    }

    protected override void OnDispose()
    {
        base.OnDispose();
    }

    public Frame WithWidth(float width) { Width = width; return this; }
    public Frame WithHeight(float height) { Height = height; return this; }
    public Frame WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public Frame WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public Frame WithBackgroundColor(SKColor color) { BackgroundColor = color; return this; }
    public Frame WithBorder(SKColor color, float thickness = 1.0f, BorderDirection direction = BorderDirection.Inside)
    {
        BorderColor = color;
        BorderThickness = thickness;
        BorderDirection = direction;
        return this;
    }
    public Frame WithBorderColor(SKColor color) { BorderColor = color; return this; }
    public Frame WithBorderThickness(float thickness) { BorderThickness = thickness; return this; }
    public Frame WithBorderDirection(BorderDirection direction) { BorderDirection = direction; return this; }
    public Frame WithParent(UIControlBase? parent) { Parent = parent; return this; }
}