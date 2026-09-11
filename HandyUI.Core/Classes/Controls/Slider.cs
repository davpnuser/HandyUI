using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Slider : UIControlBase
{
    private float _value;
    private float _minimum;
    private float _maximum = 100f;
    private bool _isDragging;

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
        set
        {
            var clamped = Math.Clamp(value, _minimum, _maximum);
            if (Math.Abs(_value - clamped) > 0.0001f)
            {
                _value = clamped;
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    public float TrackHeight { get; set; } = 6f;
    public float ThumbRadius { get; set; } = 10f;

    public SKColor TrackColor { get; set; } = SKColor.Parse("#313244");
    public SKColor ProgressColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor ThumbColor { get; set; } = SKColor.Parse("#B4BEFE");
    public SKColor ThumbHoverColor { get; set; } = SKColor.Parse("#aab5fa");

    //#aab5fa
    //#89B4FA

    public Action<float>? OnValueChanged { get; set; }

    private float _animatedThumbRadius;
    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _progressPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _thumbPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    public Slider(float width = 200f, float min = 0f, float max = 100f, float value = 0f)
    {
        _minimum = min;
        _maximum = max;
        _value = Math.Clamp(value, min, max);
        Bounds = SKRect.Create(0, 0, width, Math.Max(ThumbRadius * 2, TrackHeight));
        _animatedThumbRadius = ThumbRadius;
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var trackY = Bounds.Height / 2f;
        var padding = ThumbRadius;
        var availableWidth = Math.Max(1f, Bounds.Width - (padding * 2f));
        var normalized = (_maximum > _minimum) ? (_value - _minimum) / (_maximum - _minimum) : 0f;
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
        Value = _minimum + (normalized * (_maximum - _minimum));
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