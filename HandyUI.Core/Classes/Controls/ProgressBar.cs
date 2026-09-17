using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ProgressBar : UIControlBase
{
    private float _height = 8f;
    private float _width = 200f;
    private float _value;
    private float _minValue;
    private float _maxValue = 100f;

    public float Height
    {
        get => _height;
        set { if (_height != value) { _height = value; RecalculateBounds(); Invalidate(); } }
    }

    public float Width
    {
        get => _width;
        set { if (_width != value) { _width = value; RecalculateBounds(); Invalidate(); } }
    }

    public float Value
    {
        get => _value;
        set { var clamped = Math.Clamp(value, MinValue, MaxValue); if (_value != clamped) { _value = clamped; Invalidate(); } }
    }

    public float MinValue
    {
        get => _minValue;
        set { if (_minValue != value) { _minValue = value; Value = Math.Clamp(_value, _minValue, _maxValue); Invalidate(); } }
    }

    public float MaxValue
    {
        get => _maxValue;
        set { if (_maxValue != value) { _maxValue = value; Value = Math.Clamp(_value, _minValue, _maxValue); Invalidate(); } }
    }

    public float CornerRadius
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 4f;

    public SKColor TrackColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#1E1E2E");

    public SKColor FillColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#CBA6F7");

    public SKColor BorderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#313244");

    public float BorderWidth
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 1f;

    private float _animatedProgress;

    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public ProgressBar(float width = 200f, float height = 8f, float minValue = 0f, float maxValue = 100f, float initialValue = 0f)
    {
        _width = width;
        _height = height;
        _minValue = minValue;
        _maxValue = maxValue;
        _value = Math.Clamp(initialValue, minValue, maxValue);
        _animatedProgress = NormalizedValue;
        RecalculateBounds();
    }

    private float NormalizedValue => (MaxValue - MinValue) > 0 ? (Value - MinValue) / (MaxValue - MinValue) : 0f;

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var trackRect = SKRect.Create(0, 0, _width, _height);

        _trackPaint.Color = TrackColor;
        canvas.DrawRoundRect(trackRect, CornerRadius, CornerRadius, _trackPaint);

        var fillWidth = _width * _animatedProgress;
        if (fillWidth > 0f)
        {
            var fillRect = SKRect.Create(0, 0, fillWidth, _height);
            _fillPaint.Color = FillColor;
            canvas.DrawRoundRect(fillRect, CornerRadius, CornerRadius, _fillPaint);
        }

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(trackRect, CornerRadius, CornerRadius, _borderPaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetProgress = NormalizedValue;
        var oldProgress = _animatedProgress;

        _animatedProgress += (targetProgress - _animatedProgress) * deltaTime * 10f;

        if (Math.Abs(_animatedProgress - oldProgress) > 0.0001f)
        {
            Invalidate();
        }
    }

    protected override void OnDispose()
    {
        _trackPaint.Dispose();
        _fillPaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }
}