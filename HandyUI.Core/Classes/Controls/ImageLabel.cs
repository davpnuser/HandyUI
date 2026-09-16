using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageLabel : UIControlBase
{
    private float? _explicitWidth;
    private float? _explicitHeight;

    public SKImage? Image
    {
        get;
        set
        {
            field = value;
            RecalculateBounds();
        }
    }

    public bool AutoSize
    {
        get;
        set
        {
            field = value;
            RecalculateBounds();
        }
    } = true;

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

        if (AutoSize)
        {
            if (_explicitWidth.HasValue && _explicitHeight.HasValue)
            {
                targetWidth = _explicitWidth.Value;
                targetHeight = _explicitHeight.Value;
            }
            else if (Image != null)
            {
                targetWidth = Image.Width;
                targetHeight = Image.Height;
            }
        }
        else
        {
            targetWidth = _explicitWidth ?? Image?.Width ?? 24f;
            targetHeight = _explicitHeight ?? Image?.Height ?? 24f;
        }

        Bounds = SKRect.Create(0, 0, targetWidth, targetHeight);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible || Image == null) return;

        RecalculateBounds();
        SKRect destRect;

        if (AutoSize && _explicitWidth.HasValue && _explicitHeight.HasValue)
        {
            var imgWidth = (float)Image.Width;
            var imgHeight = (float)Image.Height;

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

        canvas.DrawImage(Image, destRect, HighSampling, _imagePaint);
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
        base.OnDispose();
    }
}