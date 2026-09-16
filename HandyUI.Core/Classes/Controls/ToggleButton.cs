using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ToggleButton : UIControlBase
{
    public float Width
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 120f;

    public float Height
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 36f;

    public string Text { get; set; } = "Toggle";
    public bool IsChecked { get; set; }
    public float CornerRadius { get; set; } = 6f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor OffColor { get; set; } = SKColor.Parse("#fbfbfe");
    public SKColor OnColor { get; set; } = SKColor.Parse("#1d72eb");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#7abdff");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#3b82f6");
    public SKColor TextOffColor { get; set; } = SKColor.Parse("#040316");
    public SKColor TextOnColor { get; set; } = SKColor.Parse("#fbfbfe");

    public Action<bool>? OnToggled { get; set; }

    private SKColor _animatedFillColor = SKColor.Parse("#fbfbfe");
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, Width, Height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var rect = SKRect.Create(0, 0, Width, Height);

        _fillPaint.Color = _animatedFillColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        _textPaint.Color = IsChecked ? TextOnColor : TextOffColor;

        var textWidth = _font.MeasureText(Text);
        var metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        var textX = (rect.Width - textWidth) / 2f;
        var textY = ((rect.Height + textHeight) / 2f) - metrics.Descent;

        canvas.DrawText(Text, textX, textY, SKTextAlign.Left, _font, _textPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetColor = IsChecked ? OnColor : IsHovered ? HoverColor : OffColor;
        _animatedFillColor = LerpColor(_animatedFillColor, targetColor, deltaTime * 12f);
    }

    private static SKColor LerpColor(SKColor from, SKColor to, float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        var r = (byte)(from.Red + ((to.Red - from.Red) * progress));
        var g = (byte)(from.Green + ((to.Green - from.Green) * progress));
        var b = (byte)(from.Blue + ((to.Blue - from.Blue) * progress));
        var a = (byte)(from.Alpha + ((to.Alpha - from.Alpha) * progress));
        return new SKColor(r, g, b, a);
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            IsChecked = !IsChecked;
            OnToggled?.Invoke(IsChecked);
            return true;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _fillPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}