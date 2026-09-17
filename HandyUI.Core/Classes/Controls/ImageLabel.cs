using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageLabel : UIControlBase
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
        set { if (_width != value) { _width = value; RecalculateBounds(); Invalidate(); } }
    }

    public SKImage? Image
    {
        get => _image;
        set { if (_image != value) { _image = value; Invalidate(); } }
    }

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

    public float CornerRadius
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 0f;

    private readonly SKPaint _imagePaint = new() { IsAntialias = true };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

    public ImageLabel(SKImage? image = null, float width = 100f, float height = 100f)
    {
        _image = image;
        _width = width;
        _height = height;
        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var destRect = SKRect.Create(0, 0, _width, _height);

        if (_image != null)
        {
            canvas.DrawImage(_image, destRect, SamplingOptions, _imagePaint);
        }

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(destRect, CornerRadius, CornerRadius, _borderPaint);
        }
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
        _imagePaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }
}