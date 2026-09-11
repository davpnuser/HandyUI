using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ToggleButton : UIControlBase
{
    private float _width;
    private float _height;

    public float Width
    {
        get => _width;
        set { _width = value; RecalculateBounds(); }
    }

    public float Height
    {
        get => _height;
        set { _height = value; RecalculateBounds(); }
    }

    public string Text { get; set; } = "Toggle";
    public bool IsChecked { get; set; }
    public float CornerRadius { get; set; } = 6f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor OffColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor OnColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor TextOffColor { get; set; } = SKColor.Parse("#CDD6F4");
    public SKColor TextOnColor { get; set; } = SKColor.Parse("#11111B");

    public Action<bool>? OnToggled { get; set; }

    private SKColor _animatedFillColor;
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public ToggleButton(string text = "Toggle", bool isChecked = false, float width = 120f, float height = 36f)
    {
        Text = text;
        IsChecked = isChecked;
        _width = width;
        _height = height;
        _animatedFillColor = isChecked ? OnColor : OffColor;
        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, _width, _height);

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