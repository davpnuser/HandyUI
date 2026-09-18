using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageButton : UIControlBase
{
    private SKTypeface? _cachedTypeface;
    private float? _explicitImageWidth;
    private float? _explicitImageHeight;
    private bool _isPressed;
    private SKColor _animatedFillColor;
    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    public float Width
    {
        get => Bounds.Width;
        set
        {
            if (Math.Abs(Bounds.Width - value) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, value, Bounds.Height);
            Invalidate();
        }
    }

    public float Height
    {
        get => Bounds.Height;
        set
        {
            if (Math.Abs(Bounds.Height - value) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, Bounds.Width, value);
            Invalidate();
        }
    }

    public SKSize Size
    {
        get => new(Bounds.Width, Bounds.Height);
        set
        {
            if (Math.Abs(Bounds.Width - value.Width) < 0.001f && Math.Abs(Bounds.Height - value.Height) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, value.Width, value.Height);
            Invalidate();
        }
    }

    public SKImage? Image
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    }

    public string Text
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = string.Empty;

    public bool AutoSizeImage
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = true;

    public float ImageWidth
    {
        get => GetTargetImageSize().Width;
        set { _explicitImageWidth = value; Invalidate(); }
    }

    public float ImageHeight
    {
        get => GetTargetImageSize().Height;
        set { _explicitImageHeight = value; Invalidate(); }
    }

    public float ImageSize
    {
        get => ImageWidth;
        set
        {
            _explicitImageWidth = value;
            _explicitImageHeight = value;
            Invalidate();
        }
    }

    public float CornerRadius
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 6f;

    public float Spacing
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 6f;

    public float TextSize
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 14f;

    public string FontFamily
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = "Segoe UI";

    public SKFontStyleWeight FontWeight
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = SKFontStyleWeight.Normal;

    public SKFontStyleWidth FontWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = SKFontStyleWidth.Normal;

    public SKFontStyleSlant FontSlant
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = SKFontStyleSlant.Upright;

    public SKColor BackgroundColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor HoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor PressedColor { get; set; } = SKColor.Parse("#45475A");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor TextColor { get; set; } = SKColor.Parse("#CDD6F4");

    public Action? OnClick { get; set; }
    public event Action? Clicked;

    public ImageButton(SKImage? image = null, string text = "", float width = 120f, float height = 36f)
    {
        Image = image;
        Text = text;
        Bounds = SKRect.Create(0, 0, width, height);
        _animatedFillColor = BackgroundColor;
    }

    private SKTypeface GetOrCreateTypeface() => _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);

    private void InvalidateTypeface()
    {
        _cachedTypeface?.Dispose();
        _cachedTypeface = null;
    }

    public void ClearExplicitImageSize()
    {
        _explicitImageWidth = null;
        _explicitImageHeight = null;
        Invalidate();
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

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetColor = _isPressed ? PressedColor : IsHovered ? HoverColor : BackgroundColor;

        var colorDiff = Math.Abs(_animatedFillColor.Red - targetColor.Red) +
                        Math.Abs(_animatedFillColor.Green - targetColor.Green) +
                        Math.Abs(_animatedFillColor.Blue - targetColor.Blue) +
                        Math.Abs(_animatedFillColor.Alpha - targetColor.Alpha);

        if (colorDiff > 0)
        {
            _animatedFillColor = LerpColor(_animatedFillColor, targetColor, Math.Min(1f, deltaTime * 12f));
            Invalidate();
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        using var fillPaint = new SKPaint { Color = _animatedFillColor, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var borderPaint = new SKPaint { Color = BorderColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };

        canvas.DrawRoundRect(Bounds, CornerRadius, CornerRadius, fillPaint);
        canvas.DrawRoundRect(Bounds, CornerRadius, CornerRadius, borderPaint);

        using var font = new SKFont(GetOrCreateTypeface(), TextSize) { Subpixel = true };
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : font.MeasureText(Text);
        var (targetImgWidth, targetImgHeight) = GetTargetImageSize();

        var totalWidth = (Image != null ? targetImgWidth + (textWidth > 0 ? Spacing : 0f) : 0f) + textWidth;
        var currentX = Bounds.Left + ((Bounds.Width - totalWidth) / 2f);

        if (Image != null)
        {
            SKRect destRect;
            var imageContainerX = currentX;
            var imageContainerY = Bounds.Top + ((Bounds.Height - targetImgHeight) / 2f);

            if (AutoSizeImage && _explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            {
                var imgWidth = (float)Image.Width;
                var imgHeight = (float)Image.Height;

                var scale = Math.Min(targetImgWidth / imgWidth, targetImgHeight / imgHeight);

                var fitWidth = imgWidth * scale;
                var fitHeight = imgHeight * scale;

                var offsetX = imageContainerX + ((targetImgWidth - fitWidth) / 2f);
                var offsetY = Bounds.Top + ((Bounds.Height - fitHeight) / 2f);

                destRect = SKRect.Create(offsetX, offsetY, fitWidth, fitHeight);
            }
            else
            {
                destRect = SKRect.Create(imageContainerX, imageContainerY, targetImgWidth, targetImgHeight);
            }

            using var imagePaint = new SKPaint { IsAntialias = true };
            canvas.DrawImage(Image, destRect, HighSampling, imagePaint);
            currentX += targetImgWidth + Spacing;
        }

        if (!string.IsNullOrEmpty(Text))
        {
            using var textPaint = new SKPaint { Color = TextColor, IsAntialias = true };
            var metrics = font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var textY = Bounds.Top + ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

            canvas.DrawText(Text, currentX, textY, SKTextAlign.Left, font, textPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (!IsEnabled) return base.OnMouse(mouseContext);

        if (mouseContext.Type == MouseEventType.MouseDown && IsHovered)
        {
            _isPressed = true;
            Invalidate();
            return true;
        }

        if (mouseContext.Type == MouseEventType.MouseUp)
        {
            if (_isPressed && IsHovered)
            {
                Clicked?.Invoke();
                OnClick?.Invoke();
            }
            _isPressed = false;
            Invalidate();
            return true;
        }

        return base.OnMouse(mouseContext);
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

    protected override void OnDispose()
    {
        InvalidateTypeface();
        base.OnDispose();
    }

    public ImageButton WithWidth(float width) { Width = width; return this; }
    public ImageButton WithHeight(float height) { Height = height; return this; }
    public ImageButton WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public ImageButton WithImage(SKImage? image) { Image = image; return this; }
    public ImageButton WithText(string text) { Text = text; return this; }
    public ImageButton WithAutoSizeImage(bool autoSize) { AutoSizeImage = autoSize; return this; }
    public ImageButton WithImageSize(float width, float height)
    {
        _explicitImageWidth = width;
        _explicitImageHeight = height;
        Invalidate();
        return this;
    }
    public ImageButton WithFont(string family, float size = 14f, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        FontFamily = family;
        TextSize = size;
        FontWeight = weight;
        FontSlant = slant;
        return this;
    }
    public ImageButton WithColors(SKColor bg, SKColor hover, SKColor pressed, SKColor border, SKColor text)
    {
        BackgroundColor = bg;
        HoverColor = hover;
        PressedColor = pressed;
        BorderColor = border;
        TextColor = text;
        return this;
    }
    public ImageButton WithOnClick(Action onClick) { Clicked += onClick; OnClick = onClick; return this; }
    public ImageButton WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public ImageButton WithParent(UIControlBase? parent) { Parent = parent; return this; }
}