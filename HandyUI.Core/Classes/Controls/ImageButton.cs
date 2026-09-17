using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageButton : UIControlBase
{
    private static readonly SKSamplingOptions SamplingOptions = new(SKCubicResampler.Mitchell);

    private float _height;
    private float _width;
    private SKImage? _image;

    public float Height
    {
        get => _height;
        set { if (_height != value) { _height = value; RecalculateBounds(); Invalidate(); } }
    }

    public float Width
    {
        get => _width;
        set { if (_width != value) { _width = value; RecalculateBounds(); } }
    }

    public SKImage? Image
    {
        get => _image;
        set { if (_image != value) { _image = value; Invalidate(); } }
    }

    public SKColor NormalColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#313244");

    public SKColor HoverColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#45475A");

    public SKColor PressedColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#585B70");

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

    public float CornerRadius
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 6f;

    public float Padding
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 4f;

    public Action? OnClick { get; set; }

    private SKColor _animatedColor;
    private bool _isPressed;

    private readonly SKPaint _backgroundPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };

    public ImageButton(SKImage? image = null, float width = 40f, float height = 40f)
    {
        _image = image;
        _width = width;
        _height = height;
        _animatedColor = NormalColor;
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

        _backgroundPaint.Color = _animatedColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _backgroundPaint);

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            var halfStroke = BorderWidth / 2f;
            var strokeRect = rect;
            strokeRect.Inflate(-halfStroke, -halfStroke);

            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(strokeRect, Math.Max(0, CornerRadius - halfStroke), Math.Max(0, CornerRadius - halfStroke), _borderPaint);
        }

        if (_image != null)
        {
            var destRect = SKRect.Create(
                Padding,
                Padding,
                Math.Max(0, _width - (Padding * 2)),
                Math.Max(0, _height - (Padding * 2))
            );

            canvas.DrawImage(_image, destRect, SamplingOptions, _imagePaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetColor = _isPressed ? PressedColor : IsHovered ? HoverColor : NormalColor;
        var oldColor = _animatedColor;

        _animatedColor = LerpColor(_animatedColor, targetColor, deltaTime * 12f);

        if (_animatedColor != oldColor)
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
        if (mouseContext.Type == MouseEventType.MouseDown && IsHovered && IsEnabled)
        {
            _isPressed = true;
            Invalidate();
            return true;
        }

        if (mouseContext.Type == MouseEventType.MouseUp)
        {
            if (_isPressed && IsHovered && IsEnabled)
            {
                OnClick?.Invoke();
            }
            _isPressed = false;
            Invalidate();
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _backgroundPaint.Dispose();
        _borderPaint.Dispose();
        _imagePaint.Dispose();
        base.OnDispose();
    }
}