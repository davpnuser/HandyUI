using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageLabel : UIControlBase<ImageLabel>
{
    private float? _explicitWidth;
    private float? _explicitHeight;
    private bool _colorsInitialized;

    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };

    public ImageLabel(SKImage? image = null, bool autoSize = true)
    {
        Image = image;
        AutoSize = autoSize;
        RecalculateBounds();
        UpdateBrushes();
    }

    public new bool IsEnabled
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = true;

    public ThemeRecord Theme
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme();

    public SKImage? Image
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    }

    public bool AutoSize
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = true;

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

    public void ClearExplicitSize()
    {
        _explicitWidth = null;
        _explicitHeight = null;
        if (!RecalculateBounds()) Invalidate();
    }

    private void UpdateBrushes()
    {
        _colorsInitialized = true;
        _imagePaint.Color = IsEnabled ? SKColors.White : Theme.DisabledColor;
    }

    public bool RecalculateBounds()
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

        if (Math.Abs(Width - targetWidth) < 0.001f && Math.Abs(Height - targetHeight) < 0.001f)
            return false;

        Bounds = SKRect.Create(Location.X, Location.Y, targetWidth, targetHeight);
        return true;
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint.X, clientPoint.Y);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!_colorsInitialized)
            UpdateBrushes();
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible || Image == null) return;

        SKRect destRect;

        if (AutoSize && _explicitWidth.HasValue && _explicitHeight.HasValue)
        {
            var imgWidth = (float)Image.Width;
            var imgHeight = (float)Image.Height;

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

        canvas.DrawImage(Image, destRect, HighSampling, _imagePaint);
    }

    protected override void OnDispose()
    {
        _imagePaint.Dispose();
        base.OnDispose();
    }

    public ImageLabel WithIsEnabled(bool isEnabled) { IsEnabled = isEnabled; return this; }
    public ImageLabel WithTheme(ThemeRecord theme) { Theme = theme; return this; }
    public ImageLabel WithImage(SKImage? image) { Image = image; return this; }
    public ImageLabel WithAutoSize(bool autoSize) { AutoSize = autoSize; return this; }
    public ImageLabel WithWidth(float width) { Width = width; return this; }
    public ImageLabel WithHeight(float height) { Height = height; return this; }
    public ImageLabel WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public ImageLabel WithParent(UIControlBase? parent) { Parent = parent; return this; }
}