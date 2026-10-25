using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class Slider : BaseControl<Slider>
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
    } = 200f;

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

    public float Minimum
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.0001f) return;
            field = value;
            if (Maximum < field) Maximum = field;
            Invalidate();
        }
    }

    public float Maximum
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.0001f) return;
            field = value;
            if (Minimum > field) Minimum = field;
            Invalidate();
        }
    } = 100f;

    public float Value
    {
        get;
        set
        {
            var v = Math.Clamp(value, Minimum, Maximum);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            Invalidate();
        }
    }

    public float TrackHeight
    {
        get;
        set
        {
            var v = Math.Max(1f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            Invalidate();
        }
    } = 4f;

    public float ThumbRadius
    {
        get;
        set
        {
            var v = Math.Max(2f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 7f;

    #endregion

    #region Internal Fields

    private readonly SKPaint _trackBgPaint = new() { IsAntialias = true };
    private readonly SKPaint _trackFgPaint = new() { IsAntialias = true };
    private readonly SKPaint _thumbPaint = new() { IsAntialias = true };
    private readonly SKPaint _thumbShadowPaint = new() { IsAntialias = true };

    private float _hoverAmount;
    private bool _lastHovered;
    private bool _isDragging;
    private TweenHandle _hoverTween;

    #endregion

    #region Events

    public event Action<float>? WhenValueChanged;

    #endregion

    #region Constructor

    public Slider()
    {
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
        StartHoverAnimation(IsHovered || _isDragging ? 1f : 0f);
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        if (w <= 0) return;

        var thumbR = ThumbRadius + (_hoverAmount * 1.5f);
        var trackLeft = thumbR;
        var trackRight = w - thumbR;
        var trackW = trackRight - trackLeft;
        if (trackW <= 0) return;

        var cy = Bounds.Height * 0.5f;
        var trackH = TrackHeight;
        var trackY = cy - (trackH * 0.5f);

        var bgRect = new SKRect(trackLeft, trackY, trackRight, trackY + trackH);
        _trackBgPaint.Color = ThemeManager.Instance.BackgroundTertiary;
        canvas.DrawRoundRect(bgRect, trackH * 0.5f, trackH * 0.5f, _trackBgPaint);

        var range = Maximum - Minimum;
        var t = range > 0 ? (Value - Minimum) / range : 0f;
        var thumbX = trackLeft + (trackW * t);

        if (thumbX > trackLeft)
        {
            var fgRect = new SKRect(trackLeft, trackY, thumbX, trackY + trackH);
            _trackFgPaint.Color = IsEnabled ? ThemeManager.Instance.Accent : ThemeManager.Instance.TextDisabled;
            canvas.DrawRoundRect(fgRect, trackH * 0.5f, trackH * 0.5f, _trackFgPaint);
        }

        _thumbShadowPaint.Color = ThemeManager.Instance.Shadow;
        canvas.DrawCircle(thumbX, cy + 1f, thumbR, _thumbShadowPaint);

        _thumbPaint.Color = IsEnabled ? ThemeManager.Instance.Accent : ThemeManager.Instance.TextDisabled;
        canvas.DrawCircle(thumbX, cy, thumbR, _thumbPaint);
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (!IsEnabled) return false;

        if (ctx.Type == MouseEventType.MouseDown && IsHovered)
        {
            _isDragging = true;
            UpdateValueFromPoint(ctx.ClientPosition);
            return true;
        }

        if (ctx.Type == MouseEventType.Move && _isDragging)
        {
            UpdateValueFromPoint(ctx.ClientPosition);
            return true;
        }

        if (ctx.Type == MouseEventType.MouseUp && _isDragging)
        {
            _isDragging = false;
            if (!IsHovered) StartHoverAnimation(0f);
            return true;
        }

        return false;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var h = Height > 0f ? Height : ThumbRadius * 2f;
        Bounds = new SKRect(0, 0, Width, h);
    }

    private void UpdateValueFromPoint(SKPoint? point)
    {
        if (point is not { } p) return;
        var w = Bounds.Width;
        var thumbR = ThumbRadius;
        var trackLeft = thumbR;
        var trackW = w - (thumbR * 2f);
        if (trackW <= 0) return;

        var t = Math.Clamp((p.X - trackLeft) / trackW, 0f, 1f);
        var newValue = Minimum + (t * (Maximum - Minimum));
        if (Math.Abs(newValue - Value) < 0.0001f) return;
        Value = newValue;
        WhenValueChanged?.Invoke(Value);
    }

    private void StartHoverAnimation(float target)
    {
        if (Tweener.Current is not { } tweener) return;
        var current = _hoverAmount;
        _hoverTween.Cancel();
        _hoverAmount = current;
        if (Math.Abs(current - target) < 0.001f) return;
        _hoverTween = tweener.To(current, target, 0.12f,
            v => { _hoverAmount = v; Invalidate(); }, EaseType.SineOut);
    }

    #endregion

    #region Fluent APIs

    public Slider WithWidth(float width) { Width = width; return Self; }
    public Slider WithHeight(float height) { Height = height; return Self; }
    public Slider WithMin(float min) { Minimum = min; return Self; }
    public Slider WithMax(float max) { Maximum = max; return Self; }
    public Slider WithValue(float value) { Value = value; return Self; }
    public Slider WithTrackHeight(float height) { TrackHeight = height; return Self; }
    public Slider WithThumbRadius(float radius) { ThumbRadius = radius; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _hoverTween.Cancel();
        _trackBgPaint.Dispose();
        _trackFgPaint.Dispose();
        _thumbPaint.Dispose();
        _thumbShadowPaint.Dispose();
    }

    #endregion
}