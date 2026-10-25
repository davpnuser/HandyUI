using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Helper;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class Button : BaseControl<Button>
{
    #region Properties

    public float Width
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public float Height
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public string Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            ResizeToContent();
            Invalidate();
        }
    } = string.Empty;

    public float CornerRadius
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            _clipPathDirty = true;
            Invalidate();
        }
    } = 2f;

    public int Elevation
    {
        get;
        set
        {
            var v = Math.Max(0, value);
            if (field == v) return;
            field = v;
            Invalidate();
        }
    } = 2;

    public float FontSize
    {
        get;
        set
        {
            var v = Math.Max(1f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            _font.Size = v;
            ResizeToContent();
            Invalidate();
        }
    } = 14f;

    public bool IsBold
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _font.Embolden = value;
            ResizeToContent();
            Invalidate();
        }
    }

    public float HorizontalPadding
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 16f;

    public float VerticalPadding
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 8f;

    #endregion

    #region Internal Fields

    private readonly SKFont _font;
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _textPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _ripplePaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _shadowPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    private SKPath? _clipPath;
    private SKRect _clipRect;
    private float _clipRadius = -1f;
    private bool _clipPathDirty = true;

    private float _hoverAmount;
    private float _rippleProgress;
    private SKPoint _rippleOrigin = SKPoint.Empty;
    private bool _isRippleActive;
    private bool _lastHovered;
    private TweenHandle _hoverTween;
    private TweenHandle _rippleTween;

    #endregion

    #region Events

    public event Action? WhenClicked;

    #endregion

    #region Constructor

    public Button()
    {
        _font = new SKFont(FontManager.DefaultFont, FontSize)
        {
            Edging = SKFontEdging.Antialias,
            Subpixel = true
        };
        ResizeToContent();
    }

    #endregion

    #region Overrides

    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return clientPoint is { } p && Bounds.Contains(p);
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition)
    {
        if (_lastHovered == IsHovered) return;
        _lastHovered = IsHovered;
        StartHoverAnimation(IsHovered ? 1f : 0f);
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var rect = new SKRect(0, 0, w, h);
        var elevate = Elevation + (_hoverAmount * 2f);

        if (Elevation > 0)
        {
            _shadowPaint.Color = ThemeManager.Instance.Shadow.WithAlpha((byte)(50 + (_hoverAmount * 40)));
            var shadowRect = new SKRect(0, elevate * 0.5f, w, h + (elevate * 0.5f));
            canvas.DrawRoundRect(shadowRect, CornerRadius, CornerRadius, _shadowPaint);
        }

        var accent = ThemeManager.Instance.Accent;
        if (_hoverAmount > 0.001f)
            accent = accent.Lerp(ThemeManager.Instance.AccentHover, _hoverAmount);

        _fillPaint.Color = accent;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);

        if (_isRippleActive && _rippleProgress is > 0f and < 1f)
        {
            var maxRadius = MathF.Sqrt((w * w) + (h * h)) * 0.5f;
            var radius = maxRadius * _rippleProgress;
            _ripplePaint.Color = SKColors.White.WithAlpha((byte)(70 * (1f - _rippleProgress)));

            if (CornerRadius > 0.01f)
            {
                var path = GetClipPath();
                canvas.Save();
                canvas.ClipPath(path, antialias: true);
                canvas.DrawCircle(_rippleOrigin.X, _rippleOrigin.Y, radius, _ripplePaint);
                canvas.Restore();
            }
            else
            {
                canvas.Save();
                canvas.ClipRect(rect);
                canvas.DrawCircle(_rippleOrigin.X, _rippleOrigin.Y, radius, _ripplePaint);
                canvas.Restore();
            }
        }

        if (!string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = IsEnabled
                ? ThemeManager.Instance.GetTone(50)
                : ThemeManager.Instance.GetTone(400);

            var fm = _font.Metrics;
            var baseline = (h * 0.5f) - ((fm.Ascent + fm.Descent) * 0.5f);
            canvas.DrawText(Text, w * 0.5f, baseline, SKTextAlign.Center, _font, _textPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Click)
        {
            WhenClicked?.Invoke();
            return true;
        }

        if (ctx.Type == MouseEventType.MouseDown && IsHovered && ctx.ClientPosition is { } p)
        {
            _rippleOrigin = p;
            _isRippleActive = true;
            _rippleProgress = 0f;
            _rippleTween.Cancel();
            if (Tweener.Current is { } tweener)
            {
                _rippleTween = tweener.To(0f, 1f, 0.4f,
                    v => { _rippleProgress = v; Invalidate(); },
                    EaseType.SineOut,
                    () => { _isRippleActive = false; Invalidate(); });
            }
            return true;
        }

        return false;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var fm = _font.Metrics;
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : _font.MeasureText(Text);
        var contentW = textWidth + (HorizontalPadding * 2f);
        var contentH = fm.Descent - fm.Ascent + (VerticalPadding * 2f);
        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    private void StartHoverAnimation(float target)
    {
        if (Tweener.Current is not { } tweener) return;
        var current = _hoverAmount;
        _hoverTween.Cancel();
        _hoverAmount = current;
        if (Math.Abs(current - target) < 0.001f) return;
        _hoverTween = tweener.To(current, target, 0.15f,
            v => { _hoverAmount = v; Invalidate(); }, EaseType.SineOut);
    }

    private SKPath GetClipPath()
    {
        if (_clipPath != null && !_clipPathDirty && _clipRect == Bounds && Math.Abs(_clipRadius - CornerRadius) < 0.01f)
            return _clipPath;

        _clipPath?.Dispose();
        using var builder = new SKPathBuilder();
        builder.AddRoundRect(new SKRect(0, 0, Bounds.Width, Bounds.Height), CornerRadius, CornerRadius);
        _clipPath = builder.Detach();

        _clipRect = Bounds;
        _clipRadius = CornerRadius;
        _clipPathDirty = false;
        return _clipPath;
    }

    #endregion

    #region Fluent APIs

    public Button WithWidth(float width) { Width = width; return Self; }
    public Button WithHeight(float height) { Height = height; return Self; }
    public Button WithText(string text) { Text = text; return Self; }
    public Button WithCornerRadius(float radius) { CornerRadius = radius; return Self; }
    public Button WithElevation(int elevation) { Elevation = elevation; return Self; }
    public Button WithFontSize(float size) { FontSize = size; return Self; }
    public Button WithIsBold(bool bold) { IsBold = bold; return Self; }
    public Button WithPadding(float horizontal, float vertical) { HorizontalPadding = horizontal; VerticalPadding = vertical; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _hoverTween.Cancel();
        _rippleTween.Cancel();
        _font.Dispose();
        _fillPaint.Dispose();
        _textPaint.Dispose();
        _ripplePaint.Dispose();
        _shadowPaint.Dispose();
        _clipPath?.Dispose();
        _clipPath = null;
    }

    #endregion
}