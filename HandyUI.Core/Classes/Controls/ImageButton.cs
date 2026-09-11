using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageButton : UIControlBase
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

    public float ImageSize
    {
        get => ImageWidth;
        set
        {
            _explicitImageWidth = value;
            _explicitImageHeight = value;
        }
    }

    public SKImage? Image { get; set; }
    public string Text { get; set; } = string.Empty;
    public float CornerRadius { get; set; } = 6f;
    public float Spacing { get; set; } = 6f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor BackgroundColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor PressedColor { get; set; } = SKColor.Parse("#45475A");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor TextColor { get; set; } = SKColor.Parse("#CDD6F4");

    public Action? OnClicked { get; set; }

    private bool _isPressed;
    private SKColor _animatedFillColor;
    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public ImageButton(SKImage? image = null, string text = "", float width = 120f, float height = 36f)
    {
        Image = image;
        Text = text;
        _width = width;
        _height = height;
        _animatedFillColor = BackgroundColor;
        RecalculateBounds();
    }

    public void ClearExplicitImageSize()
    {
        _explicitImageWidth = null;
        _explicitImageHeight = null;
    }

    private (float Width, float Height) GetTargetImageSize()
    {
        var targetWidth = 20f;
        var targetHeight = 20f;

        if (AutoSizeImage)
        {
            if (_explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            {
                targetWidth = _explicitImageWidth.Value;
                targetHeight = _explicitImageHeight.Value;
            }
            else if (Image != null)
            {
                targetWidth = Image.Width;
                targetHeight = Image.Height;
            }
        }
        else
        {
            targetWidth = _explicitImageWidth ?? Image?.Width ?? 20f;
            targetHeight = _explicitImageHeight ?? Image?.Height ?? 20f;
        }

        return (targetWidth, targetHeight);
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, _width, _height);

        _fillPaint.Color = _animatedFillColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var textWidth = string.IsNullOrEmpty(Text) ? 0f : _font.MeasureText(Text);
        var (targetImgWidth, targetImgHeight) = GetTargetImageSize();

        var totalWidth = (Image != null ? targetImgWidth + (textWidth > 0 ? Spacing : 0f) : 0f) + textWidth;
        var currentX = (rect.Width - totalWidth) / 2f;

        if (Image != null)
        {
            SKRect destRect;
            var imageContainerX = currentX;
            var imageContainerY = (rect.Height - targetImgHeight) / 2f;

            if (AutoSizeImage && _explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            {
                var imgWidth = (float)Image.Width;
                var imgHeight = (float)Image.Height;

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

            canvas.DrawImage(Image, destRect, HighSampling, _imagePaint);
            currentX += targetImgWidth + Spacing;
        }

        if (!string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = TextColor;
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
        var targetColor = _isPressed ? PressedColor : IsHovered ? HoverColor : BackgroundColor;
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
        if (!IsEnabled) return base.OnMouse(mouseContext);

        if (mouseContext.Type == MouseEventType.MouseDown && IsHovered)
        {
            _isPressed = true;
            return true;
        }

        if (mouseContext.Type == MouseEventType.MouseUp)
        {
            if (_isPressed && IsHovered)
            {
                OnClicked?.Invoke();
            }
            _isPressed = false;
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