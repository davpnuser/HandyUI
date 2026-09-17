using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Slider : UIControlBase
{
    private float _height = 20f;
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
        set
        {
            var clamped = Math.Clamp(value, MinValue, MaxValue);
            if (_value != clamped)
            {
                _value = clamped;
                OnValueChanged?.Invoke(_value);
                Invalidate();
            }
        }
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

    public float TrackHeight
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 6f;

    public float ThumbRadius
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 8f;

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

    public SKColor ThumbColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#F5E0DC");

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

    public Action<float>? OnValueChanged { get; set; }

    private bool _isDragging;

    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _thumbPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public Slider(float width = 200f, float height = 20f, float minValue = 0f, float maxValue = 100f, float initialValue = 0f)
    {
        _width = width;
        _height = height;
        _minValue = minValue;
        _maxValue = maxValue;
        _value = Math.Clamp(initialValue, minValue, maxValue);
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

        var trackY = (Bounds.Height - TrackHeight) / 2f;
        var trackRect = SKRect.Create(ThumbRadius, trackY, _width - (ThumbRadius * 2), TrackHeight);

        _trackPaint.Color = TrackColor;
        canvas.DrawRoundRect(trackRect, TrackHeight / 2f, TrackHeight / 2f, _trackPaint);

        var fillWidth = trackRect.Width * NormalizedValue;
        if (fillWidth > 0f)
        {
            var fillRect = SKRect.Create(ThumbRadius, trackY, fillWidth, TrackHeight);
            _fillPaint.Color = FillColor;
            canvas.DrawRoundRect(fillRect, TrackHeight / 2f, TrackHeight / 2f, _fillPaint);
        }

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(trackRect, TrackHeight / 2f, TrackHeight / 2f, _borderPaint);
        }

        var thumbX = trackRect.Left + fillWidth;
        var thumbY = Bounds.Height / 2f;

        _thumbPaint.Color = ThumbColor;
        canvas.DrawCircle(thumbX, thumbY, ThumbRadius, _thumbPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (_isDragging)
        {
            var trackWidth = _width - (ThumbRadius * 2);
            var relativeX = Math.Clamp(clientMousePosition.X - ThumbRadius, 0f, trackWidth);
            var pct = relativeX / trackWidth;
            Value = MinValue + (pct * (MaxValue - MinValue));
        }
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseDown && IsHovered && IsEnabled)
        {
            _isDragging = true;
            return true;
        }

        if (mouseContext.Type == MouseEventType.MouseUp)
        {
            _isDragging = false;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _trackPaint.Dispose();
        _fillPaint.Dispose();
        _thumbPaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }
}