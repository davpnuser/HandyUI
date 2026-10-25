using HandyUI.Controls.Enums;
using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class PictureButton : BaseControl<PictureButton>
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

    public SKImage? Image
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    }

    public StretchMode StretchMode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = StretchMode.Uniform;

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
    } = 4f;

    #endregion

    #region Internal Fields

    private readonly SKPaint _imagePaint = new() { IsAntialias = true };
    private readonly SKPaint _hoverPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _ripplePaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKSamplingOptions _sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    private SKPath? _clipPath;
    private SKRect _clipRect;
    private float _clipRadius = -1f;
    private bool _clipPathDirty = true;

    private float _hoverAmount;
    private bool _lastHovered;
    private TweenHandle _hoverTween;

    private float _rippleProgress;
    private SKPoint _rippleOrigin = SKPoint.Empty;
    private bool _isRippleActive;
    private TweenHandle _rippleTween;

    #endregion

    #region Events

    public event Action? WhenClicked;

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
        if (Image is null) return;
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var rect = new SKRect(0, 0, w, h);
        var srcW = (float)Image.Width;
        var srcH = (float)Image.Height;
        if (srcW <= 0 || srcH <= 0) return;

        var destRect = StretchMode switch
        {
            StretchMode.None => new SKRect(0, 0, srcW, srcH),
            StretchMode.Fill => rect,
            StretchMode.Uniform => UniformRect(srcW, srcH, w, h),
            StretchMode.UniformToFill => UniformToFillRect(srcW, srcH, w, h),
            _ => rect
        };

        if (CornerRadius > 0.01f)
        {
            var path = GetClipPath();
            canvas.Save();
            canvas.ClipPath(path, antialias: true);
            canvas.DrawImage(Image, destRect, _sampling, _imagePaint);
            canvas.Restore();
        }
        else if (StretchMode == StretchMode.UniformToFill)
        {
            canvas.Save();
            canvas.ClipRect(rect);
            canvas.DrawImage(Image, destRect, _sampling, _imagePaint);
            canvas.Restore();
        }
        else
        {
            canvas.DrawImage(Image, destRect, _sampling, _imagePaint);
        }

        if (_isRippleActive && _rippleProgress is > 0f and < 1f)
        {
            canvas.Save();
            if (CornerRadius > 0.01f)
                canvas.ClipPath(GetClipPath(), antialias: true);
            else
                canvas.ClipRect(rect);

            var maxRadius = MathF.Sqrt((w * w) + (h * h)) * 0.5f;
            var radius = maxRadius * _rippleProgress;
            _ripplePaint.Color = SKColors.White.WithAlpha((byte)(70 * (1f - _rippleProgress)));
            canvas.DrawCircle(_rippleOrigin.X, _rippleOrigin.Y, radius, _ripplePaint);
            canvas.Restore();
        }

        if (_hoverAmount > 0.001f)
        {
            _hoverPaint.Color = ThemeManager.Instance.TextPrimary.WithAlpha((byte)(30 * _hoverAmount));
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _hoverPaint);
        }

        if (!IsEnabled)
        {
            _hoverPaint.Color = ThemeManager.Instance.BackgroundPrimary.WithAlpha(140);
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _hoverPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
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

        if (ctx.Type == MouseEventType.Click)
        {
            WhenClicked?.Invoke();
            return true;
        }

        return false;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var contentW = Image?.Width ?? 0f;
        var contentH = Image?.Height ?? 0f;
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

    private static SKRect UniformRect(float srcW, float srcH, float dstW, float dstH)
    {
        var scale = Math.Min(dstW / srcW, dstH / srcH);
        var w = srcW * scale;
        var h = srcH * scale;
        return new SKRect((dstW - w) * 0.5f, (dstH - h) * 0.5f, (dstW + w) * 0.5f, (dstH + h) * 0.5f);
    }

    private static SKRect UniformToFillRect(float srcW, float srcH, float dstW, float dstH)
    {
        var scale = Math.Max(dstW / srcW, dstH / srcH);
        var w = srcW * scale;
        var h = srcH * scale;
        return new SKRect((dstW - w) * 0.5f, (dstH - h) * 0.5f, (dstW + w) * 0.5f, (dstH + h) * 0.5f);
    }

    #endregion

    #region Fluent APIs

    public PictureButton WithWidth(float width) { Width = width; return Self; }
    public PictureButton WithHeight(float height) { Height = height; return Self; }
    public PictureButton WithImage(SKImage? image) { Image = image; return Self; }
    public PictureButton WithStretchMode(StretchMode mode) { StretchMode = mode; return Self; }
    public PictureButton WithCornerRadius(float radius) { CornerRadius = radius; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _hoverTween.Cancel();
        _rippleTween.Cancel();
        _imagePaint.Dispose();
        _hoverPaint.Dispose();
        _ripplePaint.Dispose();
        _clipPath?.Dispose();
        _clipPath = null;
    }

    #endregion
}