using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageToggleButton : UIControlBase
{
    private static readonly SKSamplingOptions SamplingOptions = new(SKCubicResampler.Mitchell);

    private float _height;
    private float _width;
    private bool _isToggled;
    private SKImage? _offImage;
    private SKImage? _onImage;

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

    public bool IsToggled
    {
        get => _isToggled;
        set { if (_isToggled != value) { _isToggled = value; Invalidate(); } }
    }

    public SKImage? OffImage
    {
        get => _offImage;
        set { if (_offImage != value) { _offImage = value; Invalidate(); } }
    }

    public SKImage? OnImage
    {
        get => _onImage;
        set { if (_onImage != value) { _onImage = value; Invalidate(); } }
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

    public SKColor HoverColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#45475A");

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

    public Action<bool>? OnToggleChanged { get; set; }

    private SKColor _animatedColor;

    private readonly SKPaint _backgroundPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };

    public ImageToggleButton(SKImage? offImage = null, SKImage? onImage = null, float width = 40f, float height = 40f, bool isToggled = false)
    {
        _offImage = offImage;
        _onImage = onImage;
        _width = width;
        _height = height;
        _isToggled = isToggled;
        _animatedColor = isToggled ? OnColor : OffColor;
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
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);
        }

        var currentImage = IsToggled ? (OnImage ?? OffImage) : OffImage;

        if (currentImage != null)
        {
            var destRect = SKRect.Create(
                Padding,
                Padding,
                Math.Max(0, _width - (Padding * 2)),
                Math.Max(0, _height - (Padding * 2))
            );

            canvas.DrawImage(currentImage, destRect, SamplingOptions, _imagePaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetColor = IsToggled ? OnColor : IsHovered ? HoverColor : OffColor;
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
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            IsToggled = !IsToggled;
            OnToggleChanged?.Invoke(IsToggled);
            return true;
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