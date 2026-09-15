using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageLabel : UIControlBase
{
    private SKImage? _image;
    private float? _explicitWidth;
    private float? _explicitHeight;
    private bool _autoSize = true;

    public SKImage? Image
    {
        get => _image;
        set
        {
            _image = value;
            RecalculateBounds();
        }
    }

    public bool AutoSize
    {
        get => _autoSize;
        set
        {
            _autoSize = value;
            RecalculateBounds();
        }
    }

    public float Width
    {
        get => Bounds.Width;
        set
        {
            _explicitWidth = value;
            RecalculateBounds();
        }
    }

    public float Height
    {
        get => Bounds.Height;
        set
        {
            _explicitHeight = value;
            RecalculateBounds();
        }
    }

    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };

    public ImageLabel(SKImage? image = null, bool autoSize = true)
    {
        _image = image;
        _autoSize = autoSize;
        RecalculateBounds();
    }

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["AutoSize"] = AutoSize,
            ["Width"] = _explicitWidth ?? -1f,
            ["Height"] = _explicitHeight ?? -1f
        };
    }

    public override void Deserialize(Dictionary<string, object> values)
    {
        if (values.TryGetValue("AutoSize", out var auto) && bool.TryParse(auto.ToString(), out var autoVal)) AutoSize = autoVal;

        if (values.TryGetValue("Width", out var w) && float.TryParse(w.ToString(), out var wVal) && wVal >= 0) Width = wVal;
        if (values.TryGetValue("Height", out var h) && float.TryParse(h.ToString(), out var hVal) && hVal >= 0) Height = hVal;
    }

    public void ClearExplicitSize()
    {
        _explicitWidth = null;
        _explicitHeight = null;
        RecalculateBounds();
    }

    private void RecalculateBounds()
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

        Bounds = SKRect.Create(0, 0, targetWidth, targetHeight);
    }

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

            var offsetX = (Bounds.Width - fitWidth) / 2f;
            var offsetY = (Bounds.Height - fitHeight) / 2f;

            destRect = SKRect.Create(offsetX, offsetY, fitWidth, fitHeight);
        }
        else
        {
            destRect = SKRect.Create(0, 0, Bounds.Width, Bounds.Height);
        }

        canvas.DrawImage(_image, destRect, HighSampling, _imagePaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        ;
    }

    protected override void OnDispose()
    {
        _imagePaint.Dispose();
        base.OnDispose();
    }
}