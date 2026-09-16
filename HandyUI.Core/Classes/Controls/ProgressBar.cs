using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ProgressBar : UIControlBase
{
    private float _animatedValue;

    public float Width
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 200f;

    public float Height
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 20f;

    public float Minimum
    {
        get;
        set
        {
            field = value;
            Value = Math.Clamp(Value, field, Maximum);
        }
    }

    public float Maximum
    {
        get;
        set
        {
            field = value;
            Value = Math.Clamp(Value, Minimum, field);
        }
    } = 100f;

    public float Value
    {
        get;
        set => field = Math.Clamp(value, Minimum, Maximum);
    }

    public float CornerRadius { get; set; } = 6f;
    public bool ShowPercentage { get; set; } = false;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor TrackColor { get; set; } = SKColor.Parse("#7abdff");
    public SKColor ProgressColor { get; set; } = SKColor.Parse("#1d72eb");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#3b82f6");
    public SKColor TextColorOnTrack { get; set; } = SKColor.Parse("#040316");
    public SKColor TextColorOnProgress { get; set; } = SKColor.Parse("#fbfbfe");

    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _progressPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 12f) { Subpixel = true };

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, Width, Height);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        _animatedValue += (Value - _animatedValue) * deltaTime * 12f;
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var rect = SKRect.Create(0, 0, Bounds.Width, Bounds.Height);

        _trackPaint.Color = TrackColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _trackPaint);

        var normalized = (Maximum > Minimum) ? (_animatedValue - Minimum) / (Maximum - Minimum) : 0f;
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