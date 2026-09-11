using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ProgressBar : UIControlBase
{
    private float _value;
    private float _minimum;
    private float _maximum = 100f;
    private float _animatedValue;

    public float Minimum
    {
        get => _minimum;
        set
        {
            _minimum = value;
            Value = Math.Clamp(_value, _minimum, _maximum);
        }
    }

    public float Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            Value = Math.Clamp(_value, _minimum, _maximum);
        }
    }

    public float Value
    {
        get => _value;
        set => _value = Math.Clamp(value, _minimum, _maximum);
    }

    public float CornerRadius { get; set; } = 6f;
    public bool ShowPercentage { get; set; } = false;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor TrackColor { get; set; } = SKColor.Parse("#313244");
    public SKColor ProgressColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#45475A");
    public SKColor TextColorOnTrack { get; set; } = SKColor.Parse("#CDD6F4");
    public SKColor TextColorOnProgress { get; set; } = SKColor.Parse("#11111B");

    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _progressPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 12f) { Subpixel = true };

    public ProgressBar(float width = 200f, float height = 20f, float min = 0f, float max = 100f, float value = 0f)
    {
        _minimum = min;
        _maximum = max;
        _value = Math.Clamp(value, min, max);
        _animatedValue = _value;
        Bounds = SKRect.Create(0, 0, width, height);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        _animatedValue += (_value - _animatedValue) * deltaTime * 12f;
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, Bounds.Width, Bounds.Height);

        _trackPaint.Color = TrackColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _trackPaint);

        var normalized = (_maximum > _minimum) ? (_animatedValue - _minimum) / (_maximum - _minimum) : 0f;
        normalized = Math.Clamp(normalized, 0f, 1f);
        var fillWidth = Bounds.Width * normalized;

        if (fillWidth > 0f)
        {
            var fillRect = SKRect.Create(0, 0, fillWidth, Bounds.Height);

            canvas.Save();
            canvas.ClipRoundRect(new SKRoundRect(rect, CornerRadius, CornerRadius), SKClipOperation.Intersect, true);

            _progressPaint.Color = ProgressColor;
            canvas.DrawRect(fillRect, _progressPaint);

            canvas.Restore();
        }

        if (ShowPercentage)
        {
            var percentageText = $"{Math.Round(normalized * 100)}%";
            var metrics = _font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var textWidth = _font.MeasureText(percentageText);

            var textX = (Bounds.Width - textWidth) / 2f;
            var textY = ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

            _textPaint.Color = TextColorOnTrack;
            canvas.DrawText(percentageText, textX, textY, SKTextAlign.Left, _font, _textPaint);

            if (fillWidth > 0f)
            {
                canvas.Save();

                var fillClipRect = SKRect.Create(0, 0, fillWidth, Bounds.Height);
                canvas.ClipRect(fillClipRect, SKClipOperation.Intersect, true);

                _textPaint.Color = TextColorOnProgress;
                canvas.DrawText(percentageText, textX, textY, SKTextAlign.Left, _font, _textPaint);

                canvas.Restore();
            }
        }

        _borderPaint.Color = BorderColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);
    }

    protected override void OnDispose()
    {
        _trackPaint.Dispose();
        _progressPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}