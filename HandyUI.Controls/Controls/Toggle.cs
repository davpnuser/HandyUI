using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using HandyUI.Core.Helpers.Extensions;
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

    public SKTypeface Typeface
    {
        get => _font.Typeface ?? FontManager.DefaultFont;
        set
        {
            var v = value ?? FontManager.DefaultFont;
            if (ReferenceEquals(_font.Typeface, v)) return;
            _font.Typeface = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public bool IsOn
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _pendingToggleTarget = value ? 1f : 0f;
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
    private float? _pendingToggleTarget;
    private Tween? _toggleTween;
    private Tween? _hoverTween;

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

    public override bool DoesIntersect(SKPoint point)
    {
        return Bounds.Contains(point);
    }

    public override void OnUpdate(UpdateContext updateContext)
    {
        if (_lastHovered != IsHovered)
        {
            _lastHovered = IsHovered;
            StartHoverAnimation(updateContext.TweenManager, IsHovered ? 1f : 0f);
        }

        if (_pendingToggleTarget is { } target)
        {
            _pendingToggleTarget = null;
            var current = _toggleAmount;
            if (_toggleTween is { IsValid: true }) _toggleTween.Cancel();
            _toggleAmount = current;
            if (Math.Abs(current - target) > 0.001f)
            {
                _toggleTween = updateContext.TweenManager.To(current, target, 0.2f,
                    v => { _toggleAmount = v; Invalidate(); }, EaseType.CubicOut);
            }
        }
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

        if (!Enabled) trackColor = ThemeManager.Instance.DisabledSurface;

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
        if (!Enabled) _thumbPaint.Color = ThemeManager.Instance.GetTone(400);

        canvas.DrawCircle(thumbCX, thumbCY, thumbR, _thumbPaint);

        if (string.IsNullOrEmpty(Text)) return;

        _textPaint.Color = Enabled
            ? ThemeManager.Instance.TextPrimary
            : ThemeManager.Instance.TextDisabled;

        var fm = _font.Metrics;
        var lineHeight = fm.Descent - fm.Ascent;
        var lines = Text.Split('\n');
        var totalH = lines.Length * lineHeight;
        var topY = (Bounds.Height - totalH) * 0.5f;
        var baseline = topY - fm.Ascent;

        for (var i = 0; i < lines.Length; i++)
            canvas.DrawText(lines[i], trackW + TextGap, baseline + (i * lineHeight), SKTextAlign.Left, _font, _textPaint);
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (eventContext.Type != MouseEventType.ClickEvent || !IsHovered) return false;
        IsOn = !IsOn;
        WhenToggled?.Invoke(IsOn);
        return true;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var fm = _font.Metrics;
        var lineHeight = fm.Descent - fm.Ascent;
        var lines = Text.Split('\n');
        var textHeight = lines.Length * lineHeight;

        var maxTextWidth = 0f;
        foreach (var line in lines)
        {
            var lw = _font.MeasureText(line);
            if (lw > maxTextWidth) maxTextWidth = lw;
        }

        var contentH = Math.Max(ThumbSize, Math.Max(TrackHeight, textHeight));
        var contentW = string.IsNullOrEmpty(Text) ? TrackWidth : TrackWidth + TextGap + maxTextWidth;
        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    private void StartHoverAnimation(TweenManager tweener, float target)
    {
        var current = _hoverAmount;
        if (_hoverTween is { IsValid: true }) _hoverTween.Cancel();
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
    public Toggle WithTypeface(SKTypeface? typeface) { Typeface = typeface!; return Self; }
    public Toggle WithIsOn(bool isOn) { IsOn = isOn; return Self; }
    public Toggle WithTrackWidth(float width) { TrackWidth = width; return Self; }
    public Toggle WithTrackHeight(float height) { TrackHeight = height; return Self; }
    public Toggle WithThumbSize(float size) { ThumbSize = size; return Self; }
    public Toggle WithTextGap(float gap) { TextGap = gap; return Self; }
    public Toggle WithWhenToggled(Action<bool> handler) { WhenToggled += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        if (_toggleTween is { IsValid: true }) _toggleTween.Cancel();
        if (_hoverTween is { IsValid: true }) _hoverTween.Cancel();
        _font.Dispose();
        _trackPaint.Dispose();
        _thumbPaint.Dispose();
        _thumbShadowPaint.Dispose();
        _textPaint.Dispose();
    }

    #endregion
}