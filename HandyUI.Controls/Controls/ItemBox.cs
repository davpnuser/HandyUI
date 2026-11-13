using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class ItemBox : BaseControl<ItemBox>
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

    public bool IsSelected
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _pendingSelectTarget = value ? 1f : 0f;
            WhenSelectionChanged?.Invoke(value);
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
    private readonly SKPaint _bgPaint = new() { IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };

    private float _hoverAmount;
    private float _selectAmount;
    private bool _lastHovered;
    private float? _pendingSelectTarget;
    private Tween? _hoverTween;
    private Tween? _selectTween;

    #endregion

    #region Events

    public event Action<ItemBox>? WhenClicked;
    public event Action<bool>? WhenSelectionChanged;
    public event Action? WhenHoverEntered;
    public event Action? WhenHoverExited;

    #endregion

    #region Constructor

    public ItemBox()
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
            if (IsHovered) WhenHoverEntered?.Invoke();
            else WhenHoverExited?.Invoke();
        }

        if (_pendingSelectTarget is { } target)
        {
            _pendingSelectTarget = null;
            var current = _selectAmount;
            if (_selectTween is { IsValid: true }) _selectTween.Cancel();
            _selectAmount = current;
            if (Math.Abs(current - target) > 0.001f)
            {
                _selectTween = updateContext.TweenManager.To(current, target, 0.15f,
                    v => { _selectAmount = v; Invalidate(); }, EaseType.SineOut);
            }
        }
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var rect = new SKRect(0, 0, w, h);

        if (_selectAmount > 0.001f)
        {
            _bgPaint.Color = ThemeManager.Instance.Accent.WithAlpha((byte)(60 * _selectAmount));
            canvas.DrawRect(rect, _bgPaint);

            _bgPaint.Color = ThemeManager.Instance.Accent.WithAlpha((byte)(255 * _selectAmount));
            canvas.DrawRect(new SKRect(0, 0, 3f, h), _bgPaint);
        }

        if (_hoverAmount > 0.001f)
        {
            _bgPaint.Color = ThemeManager.Instance.TextPrimary.WithAlpha((byte)(14 * _hoverAmount));
            canvas.DrawRect(rect, _bgPaint);
        }

        if (string.IsNullOrEmpty(Text)) return;

        _textPaint.Color = Enabled
            ? ThemeManager.Instance.TextPrimary
            : ThemeManager.Instance.TextDisabled;

        var fm = _font.Metrics;
        var lineHeight = fm.Descent - fm.Ascent;
        var lines = Text.Split('\n');
        var totalH = lines.Length * lineHeight;
        var topY = (h - totalH) * 0.5f;
        var baseline = topY - fm.Ascent;

        for (var i = 0; i < lines.Length; i++)
            canvas.DrawText(lines[i], HorizontalPadding, baseline + (i * lineHeight), SKTextAlign.Left, _font, _textPaint);
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (eventContext.Type != MouseEventType.MouseDownEvent || !IsHovered) return false;

        var newState = !IsSelected;
        if (newState)
        {
            DeselectSiblings();
            IsSelected = true;
        }
        else
        {
            IsSelected = false;
        }

        WhenClicked?.Invoke(this);
        return true;
    }

    #endregion

    #region Internal Helpers

    private void DeselectSiblings()
    {
        if (Parent is not BaseControl parent) return;

        foreach (var sibling in parent.Children)
        {
            if (sibling is ItemBox other && other != this && other.IsSelected)
                other.IsSelected = false;
        }
    }

    private void ResizeToContent()
    {
        var fm = _font.Metrics;
        var lineHeight = fm.Descent - fm.Ascent;
        var lines = Text.Split('\n');

        var maxTextWidth = 0f;
        foreach (var line in lines)
        {
            var lw = _font.MeasureText(line);
            if (lw > maxTextWidth) maxTextWidth = lw;
        }

        var contentW = maxTextWidth + (HorizontalPadding * 2f);
        var contentH = (lines.Length * lineHeight) + (VerticalPadding * 2f);
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
        _hoverTween = tweener.To(current, target, 0.12f,
            v => { _hoverAmount = v; Invalidate(); }, EaseType.SineOut);
    }

    #endregion

    #region Fluent APIs

    public ItemBox WithWidth(float width) { Width = width; return Self; }
    public ItemBox WithHeight(float height) { Height = height; return Self; }
    public ItemBox WithText(string text) { Text = text; return Self; }
    public ItemBox WithFontSize(float size) { FontSize = size; return Self; }
    public ItemBox WithTypeface(SKTypeface? typeface) { Typeface = typeface!; return Self; }
    public ItemBox WithIsSelected(bool selected) { IsSelected = selected; return Self; }
    public ItemBox WithPadding(float horizontal, float vertical) { HorizontalPadding = horizontal; VerticalPadding = vertical; return Self; }
    public ItemBox WithWhenClicked(Action<ItemBox> handler) { WhenClicked += handler; return Self; }
    public ItemBox WithWhenSelectionChanged(Action<bool> handler) { WhenSelectionChanged += handler; return Self; }
    public ItemBox WithWhenHoverEntered(Action handler) { WhenHoverEntered += handler; return Self; }
    public ItemBox WithWhenHoverExited(Action handler) { WhenHoverExited += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        if (_hoverTween is { IsValid: true }) _hoverTween.Cancel();
        if (_selectTween is { IsValid: true }) _selectTween.Cancel();
        _font.Dispose();
        _bgPaint.Dispose();
        _textPaint.Dispose();
    }

    #endregion
}