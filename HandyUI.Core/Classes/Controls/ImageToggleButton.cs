using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageToggleButton : UIControlBase
{
    private float _width;
    private float _height;
    private float? _explicitImageWidth;
    private float? _explicitImageHeight;

    public float Width
    {
        get => _width;
        set { _width = value; RecalculateBounds(); }
    }

    public float Height
    {
        get => _height;
        set { _height = value; RecalculateBounds(); }
    }

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

    // Legacy fallback property that sets both dimensions simultaneously
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

    public SKColor OffColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor OnColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor TextOffColor { get; set; } = SKColor.Parse("#CDD6F4");
    public SKColor TextOnColor { get; set; } = SKColor.Parse("#11111B");

    public Action<bool>? OnToggled { get; set; }

    private SKColor _animatedFillColor;
    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public ImageToggleButton(SKImage? offImage = null, SKImage? onImage = null, string text = "", bool isChecked = false, float width = 120f, float height = 36f)
    {
        OffImage = offImage;
        OnImage = onImage;
        Text = text;
        IsChecked = isChecked;
        _width = width;
        _height = height;
        _animatedFillColor = isChecked ? OnColor : OffColor;
        RecalculateBounds();
    }

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
        var startX = RetainedModePositioning ? 0 : Location.X;
        var startY = RetainedModePositioning ? 0 : Location.Y;

        Bounds = SKRect.Create(startX, startY, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var renderX = RetainedModePositioning ? 0f : Location.X;
        var renderY = RetainedModePositioning ? 0f : Location.Y;
        var rect = SKRect.Create(renderX, renderY, _width, _height);

        _fillPaint.Color = _animatedFillColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var currentImage = IsChecked ? (OnImage ?? OffImage) : OffImage;
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : _font.MeasureText(Text);
        var (targetImgWidth, targetImgHeight) = GetTargetImageSize();

        var totalWidth = (currentImage != null ? targetImgWidth + (textWidth > 0 ? Spacing : 0f) : 0f) + textWidth;
        var currentX = rect.Left + ((rect.Width - totalWidth) / 2f);

        if (currentImage != null)
        {
            SKRect destRect;
            var imageContainerX = currentX;
            var imageContainerY = rect.Top + ((rect.Height - targetImgHeight) / 2f);

            // Replicate ImageLabel aspect ratio scaling logic when explicit bounds exist
            if (AutoSizeImage && _explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            {
                var imgWidth = (float)currentImage.Width;
                var imgHeight = (float)currentImage.Height;

                var scale = Math.Min(targetImgWidth / imgWidth, targetImgHeight / imgHeight);

                var fitWidth = imgWidth * scale;
                var fitHeight = imgHeight * scale;

                var offsetX = imageContainerX + ((targetImgWidth - fitWidth) / 2f);
                var offsetY = rect.Top + ((rect.Height - fitHeight) / 2f);

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
            var textY = rect.Top + ((rect.Height + textHeight) / 2f) - metrics.Descent;

            canvas.DrawText(Text, currentX, textY, SKTextAlign.Left, _font, _textPaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        if (RetainedModePositioning)
        {
            var localRect = SKRect.Create(0, 0, _width, _height);
            return localRect.Contains(clientPoint);
        }

        var rect = SKRect.Create(Location.X, Location.Y, _width, _height);
        return rect.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!RetainedModePositioning && (Bounds.Left != Location.X || Bounds.Top != Location.Y))
        {
            RecalculateBounds();
        }

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