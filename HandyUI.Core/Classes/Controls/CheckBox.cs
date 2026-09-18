using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class CheckBox : UIControlBase
{
    private SKTypeface? _cachedTypeface;
    private float _animProgress;
    private SKColor _animatedBoxColor;

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

    public string Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = "CheckBox";

    public bool IsChecked
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    }

    public float BoxSize
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = 20f;

    public float Spacing
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = 8f;

    public float CornerRadius
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 4f;

    public float TextSize
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            if (!RecalculateBounds()) Invalidate();
        }
    } = 14f;

    public string FontFamily
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            if (!RecalculateBounds()) Invalidate();
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
            if (!RecalculateBounds()) Invalidate();
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
            if (!RecalculateBounds()) Invalidate();
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
            if (!RecalculateBounds()) Invalidate();
        }
    } = SKFontStyleSlant.Upright;

    public SKColor BoxOffColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor BoxOnColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor BoxHoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor CheckMarkColor { get; set; } = SKColor.Parse("#11111B");
    public SKColor TextColor { get; set; } = SKColor.Parse("#CDD6F4");

    public Action<bool>? OnCheckChanged { get; set; }
    public event Action<bool>? CheckChanged;

    public CheckBox(string text = "CheckBox", bool isChecked = false)
    {
        Text = text;
        IsChecked = isChecked;
        _animProgress = isChecked ? 1f : 0f;
        _animatedBoxColor = isChecked ? BoxOnColor : BoxOffColor;
        RecalculateBounds();
    }

    private SKTypeface GetOrCreateTypeface() => _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);

    private void InvalidateTypeface()
    {
        _cachedTypeface?.Dispose();
        _cachedTypeface = null;
    }

    public bool RecalculateBounds()
    {
        using var font = new SKFont(GetOrCreateTypeface(), TextSize) { Subpixel = true };
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : font.MeasureText(Text);
        var metrics = font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        var totalWidth = BoxSize + (string.IsNullOrEmpty(Text) ? 0f : Spacing + textWidth);
        var totalHeight = Math.Max(BoxSize, textHeight);

        if (Math.Abs(Width - totalWidth) < 0.001f && Math.Abs(Height - totalHeight) < 0.001f)
            return false;

        Bounds = SKRect.Create(Location.X, Location.Y, totalWidth, totalHeight);
        return true;
    }

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetProgress = IsChecked ? 1f : 0f;
        var targetBoxColor = IsChecked ? BoxOnColor : IsHovered ? BoxHoverColor : BoxOffColor;

        var progressDiff = Math.Abs(targetProgress - _animProgress);
        var colorDiff = Math.Abs(_animatedBoxColor.Red - targetBoxColor.Red) +
                        Math.Abs(_animatedBoxColor.Green - targetBoxColor.Green) +
                        Math.Abs(_animatedBoxColor.Blue - targetBoxColor.Blue) +
                        Math.Abs(_animatedBoxColor.Alpha - targetBoxColor.Alpha);

        if (progressDiff > 0.001f || colorDiff > 0)
        {
            _animProgress += (targetProgress - _animProgress) * Math.Min(1f, deltaTime * 14f);
            _animatedBoxColor = LerpColor(_animatedBoxColor, targetBoxColor, Math.Min(1f, deltaTime * 12f));
            Invalidate();
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var boxY = Bounds.Top + ((Bounds.Height - BoxSize) / 2f);
        var boxRect = SKRect.Create(Bounds.Left, boxY, BoxSize, BoxSize);

        using var boxPaint = new SKPaint { Color = _animatedBoxColor, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var borderPaint = new SKPaint { Color = BorderColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };

        canvas.DrawRoundRect(boxRect, CornerRadius, CornerRadius, boxPaint);
        canvas.DrawRoundRect(boxRect, CornerRadius, CornerRadius, borderPaint);

        if (_animProgress > 0.01f)
        {
            using var checkPaint = new SKPaint
            {
                Color = CheckMarkColor.WithAlpha((byte)(CheckMarkColor.Alpha * _animProgress)),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            };

            var p1 = new SKPoint(boxRect.Left + (boxRect.Width * 0.25f), boxRect.Top + (boxRect.Height * 0.5f));
            var p2 = new SKPoint(boxRect.Left + (boxRect.Width * 0.45f), boxRect.Top + (boxRect.Height * 0.7f));
            var p3 = new SKPoint(boxRect.Left + (boxRect.Width * 0.75f), boxRect.Top + (boxRect.Height * 0.3f));

            using var builder = new SKPathBuilder();
            builder.MoveTo(p1);
            builder.LineTo(p2);
            builder.LineTo(p3);
            using var path = builder.Detach();

            canvas.DrawPath(path, checkPaint);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(GetOrCreateTypeface(), TextSize) { Subpixel = true };
            using var textPaint = new SKPaint { Color = TextColor, IsAntialias = true };

            var metrics = font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var textX = Bounds.Left + BoxSize + Spacing;
            var textY = Bounds.Top + ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

            canvas.DrawText(Text, textX, textY, SKTextAlign.Left, font, textPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            IsChecked = !IsChecked;
            CheckChanged?.Invoke(IsChecked);
            OnCheckChanged?.Invoke(IsChecked);
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

    public CheckBox WithWidth(float width) { Width = width; return this; }
    public CheckBox WithHeight(float height) { Height = height; return this; }
    public CheckBox WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public CheckBox WithText(string text) { Text = text; return this; }
    public CheckBox WithIsChecked(bool isChecked) { IsChecked = isChecked; return this; }
    public CheckBox WithBoxSize(float size) { BoxSize = size; return this; }
    public CheckBox WithSpacing(float spacing) { Spacing = spacing; return this; }
    public CheckBox WithCornerRadius(float radius) { CornerRadius = radius; return this; }
    public CheckBox WithFont(string family, float size = 14f, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        FontFamily = family;
        TextSize = size;
        FontWeight = weight;
        FontSlant = slant;
        return this;
    }
    public CheckBox WithColors(SKColor off, SKColor on, SKColor hover, SKColor border, SKColor checkMark, SKColor text)
    {
        BoxOffColor = off;
        BoxOnColor = on;
        BoxHoverColor = hover;
        BorderColor = border;
        CheckMarkColor = checkMark;
        TextColor = text;
        return this;
    }
    public CheckBox WithOnCheckChanged(Action<bool> onCheckChanged) { CheckChanged += onCheckChanged; OnCheckChanged = onCheckChanged; return this; }
    public CheckBox WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public CheckBox WithParent(UIControlBase? parent) { Parent = parent; return this; }
}