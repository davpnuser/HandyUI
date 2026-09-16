using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageToggleButton : UIControlBase
{
    private float? _explicitImageWidth;
    private float? _explicitImageHeight;

    public float Width
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 120f;

    public float Height
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 36f;

    public bool AutoSizeImage { get; set; } = true;

    public float ImageWidth
    {
        get => GetTargetImageSize().Width;
        set => _explicitImageWidth = value;
    }

    public float ImageHeight
    {
        get => GetTargetImageSize().Height;
        set => _explicitImageHeight = value;
    }

    public float ImageSize
    {
        get => ImageWidth;
        set
        {
            _explicitImageWidth = value;
            _explicitImageHeight = value;
        }
    }

    public SKImage? OffImage { get; set; }
    public SKImage? OnImage { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public float CornerRadius { get; set; } = 6f;
    public float Spacing { get; set; } = 6f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor OffColor { get; set; } = SKColor.Parse("#150d0c");
    public SKColor OnColor { get; set; } = SKColor.Parse("#d79a92");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#922a1e");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#e19090");
    public SKColor TextOffColor { get; set; } = SKColor.Parse("#eeebeb");
    public SKColor TextOnColor { get; set; } = SKColor.Parse("#150d0c");

    public Action<bool>? OnToggled { get; set; }

    private SKColor _animatedFillColor = SKColor.Parse("#150d0c");
    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public void ClearExplicitImageSize()
    {
        _explicitImageWidth = null;
        _explicitImageHeight = null;
    }

    private (float Width, float Height) GetTargetImageSize()
    {
        var currentImage = IsChecked ? (OnImage ?? OffImage) : OffImage;

        var targetWidth = 20f;
        var targetHeight = 20f;

        if (AutoSizeImage)
        {
            if (_explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            {
                targetWidth = _explicitImageWidth.Value;
                targetHeight = _explicitImageHeight.Value;
            }
            else if (currentImage != null)
            {
                targetWidth = currentImage.Width;
                targetHeight = currentImage.Height;
            }
        }
        else
        {
            targetWidth = _explicitImageWidth ?? currentImage?.Width ?? 20f;
            targetHeight = _explicitImageHeight ?? currentImage?.Height ?? 20f;
        }

        return (targetWidth, targetHeight);
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, Width, Height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var rect = SKRect.Create(0, 0, Width, Height);

        _fillPaint.Color = _animatedFillColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var currentImage = IsChecked ? (OnImage ?? OffImage) : OffImage;
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : _font.MeasureText(Text);
        var (targetImgWidth, targetImgHeight) = GetTargetImageSize();

        var totalWidth = (currentImage != null ? targetImgWidth + (textWidth > 0 ? Spacing : 0f) : 0f) + textWidth;
        var currentX = (rect.Width - totalWidth) / 2f;

        if (currentImage != null)
        {
            SKRect destRect;
            var imageContainerX = currentX;
            var imageContainerY = (rect.Height - targetImgHeight) / 2f;

            if (AutoSizeImage && _explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            {
                var imgWidth = (float)currentImage.Width;
                var imgHeight = (float)currentImage.Height;

                var scale = Math.Min(targetImgWidth / imgWidth, targetImgHeight / imgHeight);

                var fitWidth = imgWidth * scale;
                var fitHeight = imgHeight * scale;

                var offsetX = imageContainerX + ((targetImgWidth - fitWidth) / 2f);
                var offsetY = (rect.Height - fitHeight) / 2f;

                destRect = SKRect.Create(offsetX, offsetY, fitWidth, fitHeight);
            }
            else
            {
                destRect = SKRect.Create(imageContainerX, imageContainerY, targetImgWidth, targetImgHeight);
            }

            canvas.DrawImage(currentImage, destRect, HighSampling, _imagePaint);
            currentX += targetImgWidth + Spacing;
        }

        if (!string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = IsChecked ? TextOnColor : TextOffColor;
            var metrics = _font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var textY = ((rect.Height + textHeight) / 2f) - metrics.Descent;

            canvas.DrawText(Text, currentX, textY, SKTextAlign.Left, _font, _textPaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetColor = IsChecked ? OnColor : IsHovered ? HoverColor : OffColor;
        _animatedFillColor = LerpColor(_animatedFillColor, targetColor, deltaTime * 12f);
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
            IsChecked = !IsChecked;
            OnToggled?.Invoke(IsChecked);
            return true;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _fillPaint.Dispose();
        _borderPaint.Dispose();
        _imagePaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}