using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Enums;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Frame : UIControlBase<Frame>
{
    private bool _customBackgroundColorSet;
    private bool _customBorderColorSet;
    private bool _colorsInitialized;

    private readonly SKPaint _backgroundPaint = new() { Style = SKPaintStyle.Fill, IsAntialias = false };
    private readonly SKPaint _borderPaint = new() { Style = SKPaintStyle.Stroke, IsAntialias = false };

    public Frame()
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
            if (!_customBackgroundColorSet) BackgroundColor = value.BackgroundColor;
            if (!_customBorderColorSet) BorderColor = value.BorderColor;
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

    public SKColor BackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customBackgroundColorSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BackgroundColor;

    public SKColor BorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customBorderColorSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BorderColor;

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

    private void UpdateBrushes()
    {
        _colorsInitialized = true;

        if (IsEnabled)
        {
            _backgroundPaint.Color = BackgroundColor;
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderThickness;
        }
        else
        {
            _backgroundPaint.Color = Theme.DarkerBackgroundColor;
            _borderPaint.Color = Theme.DisabledColor;
            _borderPaint.StrokeWidth = BorderThickness;
        }
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
        if (!IsVisible) return;

        if (_backgroundPaint.Color.Alpha > 0)
        {
            canvas.DrawRect(Bounds, _backgroundPaint);
        }

        if (BorderThickness > 0 && _borderPaint.Color.Alpha > 0)
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
    }

    protected override void OnDispose()
    {
        _backgroundPaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }

    public Frame WithIsEnabled(bool isEnabled) { IsEnabled = isEnabled; return this; }
    public Frame WithTheme(ThemeRecord theme) { Theme = theme; return this; }
    public Frame WithWidth(float width) { Width = width; return this; }
    public Frame WithHeight(float height) { Height = height; return this; }
    public Frame WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
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