using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ToggleSwitch : UIControlBase
{
    private float _height = 24f;
    private float _width = 44f;
    private bool _isOn;

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

    public bool IsOn
    {
        get => _isOn;
        set { if (_isOn != value) { _isOn = value; Invalidate(); } }
    }

    public SKColor OffColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#313244");

    public SKColor OnColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#CBA6F7");

    public SKColor ThumbColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#11111B");

    public SKColor BorderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#45475A");

    public float BorderWidth
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 1f;

    public Action<bool>? OnToggled { get; set; }

    private float _animProgress;
    private SKColor _animatedTrackColor;

    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _thumbPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public ToggleSwitch(float width = 44f, float height = 24f, bool isOn = false)
    {
        _width = width;
        _height = height;
        _isOn = isOn;
        _animProgress = isOn ? 1f : 0f;
        _animatedTrackColor = isOn ? OnColor : OffColor;
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
        var radius = _height / 2f;

        _trackPaint.Color = _animatedTrackColor;
        canvas.DrawRoundRect(rect, radius, radius, _trackPaint);

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(rect, radius, radius, _borderPaint);
        }

        var padding = 3f;
        var thumbRadius = radius - padding;
        var minX = radius;
        var maxX = _width - radius;
        var thumbX = minX + ((maxX - minX) * _animProgress);
        var thumbY = radius;

        _thumbPaint.Color = ThumbColor;
        canvas.DrawCircle(thumbX, thumbY, thumbRadius, _thumbPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetProgress = IsOn ? 1f : 0f;
        var targetTrackColor = IsOn ? OnColor : OffColor;

        var oldProgress = _animProgress;
        var oldColor = _animatedTrackColor;

        _animProgress += (targetProgress - _animProgress) * deltaTime * 14f;
        _animatedTrackColor = LerpColor(_animatedTrackColor, targetTrackColor, deltaTime * 12f);

        if (Math.Abs(_animProgress - oldProgress) > 0.0001f || _animatedTrackColor != oldColor)
        {
            Invalidate();
        }
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
            IsOn = !IsOn;
            OnToggled?.Invoke(IsOn);
            return true;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _trackPaint.Dispose();
        _thumbPaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }
}