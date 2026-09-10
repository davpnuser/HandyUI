using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextButton : UIControlBase
{
    public string Text { get; set; } = "Button";
    public float CornerRadius { get; set; } = 6f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor NormalColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor PressedColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor TextColor { get; set; } = SKColor.Parse("#CDD6F4");

    public Action? OnClick { get; set; }

    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public TextButton(string text, float width = 120f, float height = 36f)
    {
        Text = text;
        Bounds = SKRect.Create(Location.X, Location.Y, width, height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(Location.X, Location.Y, Bounds.Width, Bounds.Height);

        _fillPaint.Color = IsMouseDown && IsHovered ? PressedColor : IsHovered ? HoverColor : NormalColor;

        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        _textPaint.Color = TextColor;

        var textWidth = _font.MeasureText(Text);
        SKFontMetrics metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        var textX = rect.Left + ((rect.Width - textWidth) / 2f);
        var textY = rect.Top + ((rect.Height + textHeight) / 2f) - metrics.Descent;

        canvas.DrawText(Text, textX, textY, SKTextAlign.Left, _font, _textPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        var rect = SKRect.Create(Location.X, Location.Y, Bounds.Width, Bounds.Height);
        return rect.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (Bounds.Left != Location.X || Bounds.Top != Location.Y)
        {
            Bounds = SKRect.Create(Location.X, Location.Y, Bounds.Width, Bounds.Height);
        }
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