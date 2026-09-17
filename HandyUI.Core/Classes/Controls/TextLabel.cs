using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextLabel : UIControlBase
{
    private string _text = "Label";

    public string Text
    {
        get => _text;
        set { if (_text != value) { _text = value; RecalculateBounds(); Invalidate(); } }
    }

    public float TextSize
    {
        get => _font.Size;
        set { if (_font.Size != value) { _font.Size = value; RecalculateBounds(); Invalidate(); } }
    }

    public SKColor TextColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#CDD6F4");

    public SKColor BorderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Empty;

    public float BorderWidth
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 0f;

    public SKTextAlign TextAlign
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKTextAlign.Left;

    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public TextLabel(string text = "Label")
    {
        _text = text;
        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        var textWidth = string.IsNullOrEmpty(_text) ? 0f : _font.MeasureText(_text);
        var metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        Bounds = SKRect.Create(0, 0, textWidth, textHeight);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRect(Bounds, _borderPaint);
        }

        if (string.IsNullOrEmpty(_text)) return;

        _textPaint.Color = TextColor;
        var metrics = _font.Metrics;
        var textY = -metrics.Ascent;

        var textX = TextAlign switch
        {
            SKTextAlign.Center => Bounds.Width / 2f,
            SKTextAlign.Right => Bounds.Width,
            _ => 0f,
        };

        canvas.DrawText(_text, textX, textY, TextAlign, _font, _textPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
    }

    protected override void OnDispose()
    {
        _textPaint.Dispose();
        _borderPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}