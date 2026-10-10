using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Enums;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ImageButton : UIControlBase<ImageButton>
{
    private SKTypeface? _cachedTypeface;
    private float? _explicitImageWidth;
    private float? _explicitImageHeight;
    private bool _colorsInitialized;

    private bool _customNormalBgSet;
    private bool _customHoverBgSet;
    private bool _customPressedBgSet;
    private bool _customNormalBorderSet;
    private bool _customActiveBorderSet;
    private bool _customTextSet;

    private static readonly SKSamplingOptions HighSampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private readonly SKPaint _bgPaint = new() { Style = SKPaintStyle.Fill, IsAntialias = false };
    private readonly SKPaint _borderPaint = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 1.0f, IsAntialias = false };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _imagePaint = new() { IsAntialias = true };

    public ImageButton(SKImage? image = null, string text = "", float width = 120f, float height = 36f)
    {
        Image = image;
        Text = text;
        Bounds = SKRect.Create(0, 0, width, height);
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
            if (!_customNormalBgSet) NormalBackgroundColor = value.BackgroundColor;
            if (!_customHoverBgSet) HoverBackgroundColor = value.SemiActiveColor;
            if (!_customPressedBgSet) PressedBackgroundColor = value.ActiveColor;
            if (!_customNormalBorderSet) NormalBorderColor = value.BorderColor;
            if (!_customActiveBorderSet) ActiveBorderColor = value.ActiveColor;
            if (!_customTextSet) TextColor = value.TextColor;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme();

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

    public float Spacing
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 6f;

    public float TextSize
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 13f;

    public SKTypeface? Typeface
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    }

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

    public SKColor NormalBackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customNormalBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BackgroundColor;

    public SKColor HoverBackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customHoverBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().SemiActiveColor;

    public SKColor PressedBackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customPressedBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().ActiveColor;

    public SKColor NormalBorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customNormalBorderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BorderColor;

    public SKColor ActiveBorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customActiveBorderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().ActiveColor;

    public SKColor TextColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customTextSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().TextColor;

    public SKColor BackgroundColor { get => NormalBackgroundColor; set => NormalBackgroundColor = value; }
    public SKColor HoverColor { get => HoverBackgroundColor; set => HoverBackgroundColor = value; }
    public SKColor PressedColor { get => PressedBackgroundColor; set => PressedBackgroundColor = value; }
    public SKColor BorderColor { get => NormalBorderColor; set => NormalBorderColor = value; }

    public float BorderThickness
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = 1.0f;

    public BorderDirection BorderDirection
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = BorderDirection.Inside;

    public Action? OnClick { get; set; }
    public event Action? Clicked;

    private SKTypeface GetEffectiveTypeface()
    {
        return Typeface ?? GetOrCreateTypeface();
    }

    private SKTypeface GetOrCreateTypeface()
    {
        return _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);
    }

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
        if (_explicitImageWidth.HasValue && _explicitImageHeight.HasValue)
            return (_explicitImageWidth.Value, _explicitImageHeight.Value);

        if (Image == null)
            return (20f, 20f);

        if (AutoSizeImage)
        {
            var maxBox = Math.Max(8f, Bounds.Height);
            return (maxBox, maxBox);
        }

        return (Image.Width, Image.Height);
    }

    private void UpdateBrushes()
    {
        _colorsInitialized = true;

        if (!IsEnabled)
        {
            _bgPaint.Color = Theme.DarkerBackgroundColor;
            _borderPaint.Color = Theme.DisabledColor;
            _textPaint.Color = Theme.DisabledTextColor;
            _imagePaint.Color = Theme.DisabledColor;
        }
        else
        {
            if (IsMouseDown && IsHovered)
            {
                _bgPaint.Color = PressedBackgroundColor;
                _borderPaint.Color = ActiveBorderColor;
            }
            else if (IsHovered)
            {
                _bgPaint.Color = HoverBackgroundColor;
                _borderPaint.Color = ActiveBorderColor;
            }
            else
            {
                _bgPaint.Color = NormalBackgroundColor;
                _borderPaint.Color = NormalBorderColor;
            }

            _borderPaint.StrokeWidth = BorderThickness;
            _textPaint.Color = TextColor;
            _imagePaint.Color = SKColors.White;
        }
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
        if (!IsVisible) return;

        UpdateBrushes();

        canvas.DrawRect(Bounds, _bgPaint);

        if (BorderThickness > 0)
        {
            var borderRect = BorderDirection switch
            {
                BorderDirection.Inside => SKRect.Create(
                    Bounds.Left + (BorderThickness / 2f),
                    Bounds.Top + (BorderThickness / 2f),
                    Bounds.Width - BorderThickness,
                    Bounds.Height - BorderThickness),
                BorderDirection.Outside => SKRect.Create(
                    Bounds.Left - (BorderThickness / 2f),
                    Bounds.Top - (BorderThickness / 2f),
                    Bounds.Width + BorderThickness,
                    Bounds.Height + BorderThickness),
                _ => Bounds
            };

            canvas.DrawRect(borderRect, _borderPaint);
        }

        using var font = new SKFont(GetEffectiveTypeface(), TextSize) { Subpixel = true };
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : font.MeasureText(Text);
        var (targetImgWidth, targetImgHeight) = GetTargetImageSize();

        var totalWidth = (Image != null ? targetImgWidth + (textWidth > 0 ? Spacing : 0f) : 0f) + textWidth;
        var currentX = Bounds.Left + ((Bounds.Width - totalWidth) / 2f);

        if (Image != null)
        {
            var imageContainerX = currentX;

            SKRect destRect;

            if (AutoSizeImage)
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
                var imageContainerY = Bounds.Top + ((Bounds.Height - targetImgHeight) / 2f);
                destRect = SKRect.Create(imageContainerX, imageContainerY, targetImgWidth, targetImgHeight);
            }

            canvas.DrawImage(Image, destRect, HighSampling, _imagePaint);
            currentX += targetImgWidth + Spacing;
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var metrics = font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var textY = Bounds.Top + ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

            canvas.DrawText(Text, currentX, textY, SKTextAlign.Left, font, _textPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            Clicked?.Invoke();
            OnClick?.Invoke();
            return true;
        }

        return false;
    }

    protected override void OnDispose()
    {
        InvalidateTypeface();
        _bgPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _imagePaint.Dispose();
        base.OnDispose();
    }

    public ImageButton WithTheme(ThemeRecord theme) { Theme = theme; return this; }
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
    public ImageButton WithFont(string family, float size = 13f, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        FontFamily = family;
        TextSize = size;
        FontWeight = weight;
        FontSlant = slant;
        return this;
    }
    public ImageButton WithTypeface(SKTypeface? typeface) { Typeface = typeface; return this; }
    public ImageButton WithFontWeight(SKFontStyleWeight weight) { FontWeight = weight; return this; }
    public ImageButton WithFontSlant(SKFontStyleSlant slant) { FontSlant = slant; return this; }
    public ImageButton WithColors(SKColor normal, SKColor hover, SKColor pressed)
    {
        NormalBackgroundColor = normal;
        HoverBackgroundColor = hover;
        PressedBackgroundColor = pressed;
        return this;
    }
    public ImageButton WithColors(SKColor bg, SKColor hover, SKColor pressed, SKColor border, SKColor text)
    {
        NormalBackgroundColor = bg;
        HoverBackgroundColor = hover;
        PressedBackgroundColor = pressed;
        NormalBorderColor = border;
        TextColor = text;
        return this;
    }
    public ImageButton WithBorder(SKColor normalColor, SKColor activeColor, float thickness = 1.0f, BorderDirection direction = BorderDirection.Inside)
    {
        NormalBorderColor = normalColor;
        ActiveBorderColor = activeColor;
        BorderThickness = thickness;
        BorderDirection = direction;
        return this;
    }
    public ImageButton WithBorderThickness(float thickness) { BorderThickness = thickness; return this; }
    public ImageButton WithBorderDirection(BorderDirection direction) { BorderDirection = direction; return this; }
    public ImageButton WithOnClick(Action onClick) { Clicked += onClick; OnClick += onClick; return this; }
}