using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Slider : UIControlBase
{
    private bool _isDragging;

    public float Width
    {
        get;
        set
        {
            field = Math.Max(0f, value);
            RecalculateBounds();
        }
    } = 200f;

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
        set
        {
            var clamped = Math.Clamp(value, Minimum, Maximum);
            if (Math.Abs(field - clamped) > 0.0001f)
            {
                field = clamped;
                OnValueChanged?.Invoke(field);
            }
        }
    }

    public float TrackHeight { get; set; } = 6f;
    public float ThumbRadius { get; set; } = 10f;

    public SKColor TrackColor { get; set; } = SKColor.Parse("#7abdff");
    public SKColor ProgressColor { get; set; } = SKColor.Parse("#1d72eb");
    public SKColor ThumbColor { get; set; } = SKColor.Parse("#1d72eb");
    public SKColor ThumbHoverColor { get; set; } = SKColor.Parse("#3b82f6");

    public Action<float>? OnValueChanged { get; set; }

    private float _animatedThumbRadius = 10f;
    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _progressPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _thumbPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, Width, Math.Max(ThumbRadius * 2f, TrackHeight));
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var trackY = Bounds.Height / 2f;
        var padding = ThumbRadius;
        var availableWidth = Math.Max(1f, Bounds.Width - (padding * 2f));
        var normalized = (Maximum > Minimum) ? (Value - Minimum) / (Maximum - Minimum) : 0f;
        var thumbX = padding + (normalized * availableWidth);

        var trackRect = SKRect.Create(padding, trackY - (TrackHeight / 2f), availableWidth, TrackHeight);
        _trackPaint.Color = TrackColor;
        canvas.DrawRoundRect(trackRect, TrackHeight / 2f, TrackHeight / 2f, _trackPaint);

        if (thumbX > padding)
        {
            var progressRect = SKRect.Create(padding, trackY - (TrackHeight / 2f), thumbX - padding, TrackHeight);
            _progressPaint.Color = ProgressColor;
            canvas.DrawRoundRect(progressRect, TrackHeight / 2f, TrackHeight / 2f, _progressPaint);
        }

        _thumbPaint.Color = (_isDragging || IsHovered) ? ThumbHoverColor : ThumbColor;
        canvas.DrawCircle(thumbX, trackY, _animatedThumbRadius, _thumbPaint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetRadius = (_isDragging || IsHovered) ? ThumbRadius * 1.25f : ThumbRadius;
        _animatedThumbRadius += (targetRadius - _animatedThumbRadius) * deltaTime * 15f;

        if (_isDragging)
        {
            UpdateValueFromMouseX(clientMousePosition.X);
        }
    }

    private void UpdateValueFromMouseX(float mouseX)
    {
        var padding = ThumbRadius;
        var availableWidth = Math.Max(1f, Bounds.Width - (padding * 2f));
        var relativeX = mouseX - padding;
        var normalized = Math.Clamp(relativeX / availableWidth, 0f, 1f);
        Value = Minimum + (normalized * (Maximum - Minimum));
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseDown && mouseContext.Button == MouseButton.Left && IsHovered && IsEnabled)
        {
            _isDragging = true;
            UpdateValueFromMouseX(mouseContext.ClientPosition.X);
            return true;
        }

        if (mouseContext.Type == MouseEventType.MouseUp && mouseContext.Button == MouseButton.Left)
        {
            _isDragging = false;
        }

        if (mouseContext.Type == MouseEventType.Move && _isDragging)
        {
            UpdateValueFromMouseX(mouseContext.ClientPosition.X);
            return true;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _trackPaint.Dispose();
        _progressPaint.Dispose();
        _thumbPaint.Dispose();
        base.OnDispose();
    }
}