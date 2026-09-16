using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextButton : UIControlBase
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

    public string Text { get; set; } = "Button";
    public float CornerRadius { get; set; } = 6f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor NormalColor { get; set; } = SKColor.Parse("#150d0c");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#922a1e");
    public SKColor PressedColor { get; set; } = SKColor.Parse("#541c15");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#e19090");
    public SKColor TextColor { get; set; } = SKColor.Parse("#eeebeb");

    public Action? OnClick { get; set; }

    private SKColor _animatedColor = SKColor.Parse("#150d0c");
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

        _fillPaint.Color = _animatedColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        _textPaint.Color = TextColor;

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
        var targetColor = IsMouseDown && IsHovered ? PressedColor : IsHovered ? HoverColor : NormalColor;
        _animatedColor = LerpColor(_animatedColor, targetColor, deltaTime * 12f);
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
            OnClick?.Invoke();
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