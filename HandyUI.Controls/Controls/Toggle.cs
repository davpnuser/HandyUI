using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Helper;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class Toggle : BaseControl<Toggle>
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

    public bool IsOn
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            StartToggleAnimation(value ? 1f : 0f);
            Invalidate();
        }
    }

    public float TrackWidth
    {
        get;
        set
        {
            var v = Math.Max(20f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 36f;

    public float TrackHeight
    {
        get;
        set
        {
            var v = Math.Max(8f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 14f;

    public float ThumbSize
    {
        get;
        set
        {
            var v = Math.Max(6f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 20f;

    public float TextGap
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
    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _thumbPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _thumbShadowPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };

    private float _toggleAmount;
    private float _hoverAmount;
    private bool _lastHovered;
    private TweenHandle _toggleTween;
    private TweenHandle _hoverTween;

    #endregion

    #region Events

    public event Action<bool>? WhenToggled;

    #endregion

    #region Constructor

    public Toggle()
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
        var trackW = Math.Min(TrackWidth, Bounds.Width);
        var trackH = Math.Min(TrackHeight, Bounds.Height);
        var thumbD = Math.Min(ThumbSize, Bounds.Height);

        var trackX = 0f;
        var trackY = (Bounds.Height - trackH) * 0.5f;
        var trackRect = new SKRect(trackX, trackY, trackX + trackW, trackY + trackH);

        var trackColor = ThemeManager.Instance.TextTertiary;
        if (_toggleAmount > 0.001f)
            trackColor = trackColor.Lerp(ThemeManager.Instance.Accent.WithAlpha(140), _toggleAmount);
        if (!IsEnabled) trackColor = ThemeManager.Instance.DisabledSurface;

        _trackPaint.Color = trackColor;
        canvas.DrawRoundRect(trackRect, trackH * 0.5f, trackH * 0.5f, _trackPaint);

        var thumbStartX = trackX + (thumbD * 0.5f);
        var thumbEndX = trackX + trackW - (thumbD * 0.5f);
        var thumbCX = thumbStartX + ((thumbEndX - thumbStartX) * _toggleAmount);
        var thumbCY = Bounds.Height * 0.5f;
        var thumbR = (thumbD * 0.5f) + (_hoverAmount * 1.5f);

        _thumbShadowPaint.Color = ThemeManager.Instance.Shadow;
        canvas.DrawCircle(thumbCX, thumbCY + 1f, thumbR, _thumbShadowPaint);

        _thumbPaint.Color = _toggleAmount < 0.5f
            ? ThemeManager.Instance.GetTone(200)
            : ThemeManager.Instance.Accent;
        if (!IsEnabled) _thumbPaint.Color = ThemeManager.Instance.GetTone(400);

        canvas.DrawCircle(thumbCX, thumbCY, thumbR, _thumbPaint);

        if (!string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = IsEnabled
                ? ThemeManager.Instance.TextPrimary
                : ThemeManager.Instance.TextDisabled;

            var fm = _font.Metrics;
            var baseline = (Bounds.Height * 0.5f) - ((fm.Ascent + fm.Descent) * 0.5f);
            canvas.DrawText(Text, trackW + TextGap, baseline, SKTextAlign.Left, _font, _textPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Click && IsHovered)
        {
            IsOn = !IsOn;
            WhenToggled?.Invoke(IsOn);
            return true;
        }
        return false;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var fm = _font.Metrics;
        var textHeight = fm.Descent - fm.Ascent;
        var contentH = Math.Max(ThumbSize, Math.Max(TrackHeight, textHeight));
        var contentW = string.IsNullOrEmpty(Text)
            ? TrackWidth
            : TrackWidth + TextGap + _font.MeasureText(Text);
        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    private void StartToggleAnimation(float target)
    {
        if (Tweener.Current is not { } tweener)
        {
            _toggleAmount = target;
            Invalidate();
            return;
        }

        var current = _toggleAmount;
        _toggleTween.Cancel();
        _toggleAmount = current;
        if (Math.Abs(current - target) < 0.001f) return;
        _toggleTween = tweener.To(current, target, 0.2f,
            v => { _toggleAmount = v; Invalidate(); }, EaseType.CubicOut);
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

    #endregion

    #region Fluent APIs

    public Toggle WithWidth(float width) { Width = width; return Self; }
    public Toggle WithHeight(float height) { Height = height; return Self; }
    public Toggle WithText(string text) { Text = text; return Self; }
    public Toggle WithFontSize(float size) { FontSize = size; return Self; }
    public Toggle WithIsOn(bool isOn) { IsOn = isOn; return Self; }
    public Toggle WithTrackWidth(float width) { TrackWidth = width; return Self; }
    public Toggle WithTrackHeight(float height) { TrackHeight = height; return Self; }
    public Toggle WithThumbSize(float size) { ThumbSize = size; return Self; }
    public Toggle WithTextGap(float gap) { TextGap = gap; return Self; }

    public Toggle OnToggled(Action<bool> handler) { WhenToggled += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _toggleTween.Cancel();
        _hoverTween.Cancel();
        _font.Dispose();
        _trackPaint.Dispose();
        _thumbPaint.Dispose();
        _thumbShadowPaint.Dispose();
        _textPaint.Dispose();
    }

    #endregion
}