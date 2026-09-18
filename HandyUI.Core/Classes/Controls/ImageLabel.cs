using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageLabel : UIControlBase
{
    private SKImage? _image;
    private float? _explicitWidth;
    private float? _explicitHeight;
    private bool _autoSize = true;
    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    public SKImage? Image
    {
        get => _image;
        set
        {
            if (_image == value) return;
            _image = value;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize == value) return;
            _autoSize = value;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public float Width
    {
        get => Bounds.Width;
        set
        {
            if (Math.Abs(Bounds.Width - value) < 0.001f) return;
            _explicitWidth = value;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public float Height
    {
        get => Bounds.Height;
        set
        {
            if (Math.Abs(Bounds.Height - value) < 0.001f) return;
            _explicitHeight = value;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public SKSize Size
    {
        get => new(Bounds.Width, Bounds.Height);
        set
        {
            if (Math.Abs(Bounds.Width - value.Width) < 0.001f && Math.Abs(Bounds.Height - value.Height) < 0.001f) return;
            _explicitWidth = value.Width;
            _explicitHeight = value.Height;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public ImageLabel(SKImage? image = null, bool autoSize = true)
    {
        _image = image;
        _autoSize = autoSize;
        RecalculateBounds();
    }

    public void ClearExplicitSize()
    {
        _explicitWidth = null;
        _explicitHeight = null;
        if (!RecalculateBounds()) Invalidate();
    }

    public bool RecalculateBounds()
    {
        var targetWidth = 24f;
        var targetHeight = 24f;

        if (_autoSize)
        {
            if (_explicitWidth.HasValue && _explicitHeight.HasValue)
            {
                targetWidth = _explicitWidth.Value;
                targetHeight = _explicitHeight.Value;
            }
            else if (_image != null)
            {
                targetWidth = _image.Width;
                targetHeight = _image.Height;
            }
        }
        else
        {
            targetWidth = _explicitWidth ?? _image?.Width ?? 24f;
            targetHeight = _explicitHeight ?? _image?.Height ?? 24f;
        }

        if (Math.Abs(Width - targetWidth) < 0.001f && Math.Abs(Height - targetHeight) < 0.001f)
            return false;

        Bounds = SKRect.Create(Location.X, Location.Y, targetWidth, targetHeight);
        return true;
    }

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    public override void Update(float deltaTime, SKPoint clientMousePosition) { }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible || _image == null) return;

        SKRect destRect;

        if (_autoSize && _explicitWidth.HasValue && _explicitHeight.HasValue)
        {
            var imgWidth = (float)_image.Width;
            var imgHeight = (float)_image.Height;

            var scale = Math.Min(Bounds.Width / imgWidth, Bounds.Height / imgHeight);

            var fitWidth = imgWidth * scale;
            var fitHeight = imgHeight * scale;

            var offsetX = Bounds.Left + ((Bounds.Width - fitWidth) / 2f);
            var offsetY = Bounds.Top + ((Bounds.Height - fitHeight) / 2f);

            destRect = SKRect.Create(offsetX, offsetY, fitWidth, fitHeight);
        }
        else
        {
            destRect = SKRect.Create(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height);
        }

        using var imagePaint = new SKPaint { IsAntialias = true };
        canvas.DrawImage(_image, destRect, HighSampling, imagePaint);
    }

    public ImageLabel WithImage(SKImage? image) { Image = image; return this; }
    public ImageLabel WithAutoSize(bool autoSize) { AutoSize = autoSize; return this; }
    public ImageLabel WithWidth(float width) { Width = width; return this; }
    public ImageLabel WithHeight(float height) { Height = height; return this; }
    public ImageLabel WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public ImageLabel WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public ImageLabel WithParent(UIControlBase? parent) { Parent = parent; return this; }
}