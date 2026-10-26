using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Helper;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class CheckBox : BaseControl<CheckBox>
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

    public bool IsChecked
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            StartCheckAnimation(value ? 1f : 0f);
            Invalidate();
        }
    }

    public float BoxSize
    {
        get;
        set
        {
            var v = Math.Max(4f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 18f;

    public float BorderWidth
    {
        get;
        set
        {
            var v = Math.Max(0.5f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            Invalidate();
        }
    } = 2f;

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
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _checkPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };

    private float _checkAmount;
    private float _hoverAmount;
    private bool _lastHovered;
    private TweenHandle _checkTween;
    private TweenHandle _hoverTween;

    #endregion

    #region Events

    public event Action<bool>? WhenToggled;

    #endregion

    #region Constructor

    public CheckBox()
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
        var size = Math.Min(BoxSize, Bounds.Height);
        var x = 0f;
        var y = (Bounds.Height - size) * 0.5f;
        var rect = new SKRect(x, y, x + size, y + size);

        if (_hoverAmount > 0.001f)
        {
            _fillPaint.Color = ThemeManager.Instance.Accent.WithAlpha((byte)(20 * _hoverAmount));
            var hoverRect = new SKRect(x - 6, y - 6, x + size + 6, y + size + 6);
            canvas.DrawRoundRect(hoverRect, 4, 4, _fillPaint);
        }

        var borderColor = ThemeManager.Instance.TextSecondary;
        if (_checkAmount > 0.001f)
            borderColor = borderColor.Lerp(ThemeManager.Instance.Accent, _checkAmount);

        _borderPaint.Color = IsEnabled ? borderColor : ThemeManager.Instance.TextDisabled;
        _borderPaint.StrokeWidth = BorderWidth;
        canvas.DrawRect(rect, _borderPaint);

        if (_checkAmount > 0.001f)
        {
            _fillPaint.Color = ThemeManager.Instance.Accent.WithAlpha((byte)(255 * _checkAmount));
            canvas.DrawRect(rect, _fillPaint);

            _checkPaint.Color = ThemeManager.Instance.GetTone(50).WithAlpha((byte)(255 * _checkAmount));
            _checkPaint.StrokeWidth = Math.Max(2f, size * 0.14f);

            var cx = rect.MidX;
            var cy = rect.MidY;
            var sz = size * 0.28f;
            var p1 = new SKPoint(cx - sz, cy);
            var p2 = new SKPoint(cx - (sz * 0.2f), cy + (sz * 0.7f));
            var p3 = new SKPoint(cx + sz, cy - (sz * 0.7f));

            if (_checkAmount < 0.5f)
            {
                var t = _checkAmount * 2f;
                var mid = new SKPoint(p1.X + ((p2.X - p1.X) * t), p1.Y + ((p2.Y - p1.Y) * t));
                canvas.DrawLine(p1, mid, _checkPaint);
            }
            else
            {
                var t = (_checkAmount - 0.5f) * 2f;
                var end = new SKPoint(p2.X + ((p3.X - p2.X) * t), p2.Y + ((p3.Y - p2.Y) * t));
                canvas.DrawLine(p1, p2, _checkPaint);
                canvas.DrawLine(p2, end, _checkPaint);
            }
        }

        if (!string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = IsEnabled
                ? ThemeManager.Instance.TextPrimary
                : ThemeManager.Instance.TextDisabled;

            var fm = _font.Metrics;
            var baseline = (Bounds.Height * 0.5f) - ((fm.Ascent + fm.Descent) * 0.5f);
            canvas.DrawText(Text, size + TextGap, baseline, SKTextAlign.Left, _font, _textPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Click && IsHovered)
        {
            IsChecked = !IsChecked;
            WhenToggled?.Invoke(IsChecked);
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
        var contentH = Math.Max(BoxSize, textHeight);
        var contentW = string.IsNullOrEmpty(Text)
            ? BoxSize
            : BoxSize + TextGap + _font.MeasureText(Text);
        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    private void StartCheckAnimation(float target)
    {
        if (Tweener.Current is not { } tweener)
        {
            _checkAmount = target;
            Invalidate();
            return;
        }

        var current = _checkAmount;
        _checkTween.Cancel();
        _checkAmount = current;
        if (Math.Abs(current - target) < 0.001f) return;
        _checkTween = tweener.To(current, target, 0.2f,
            v => { _checkAmount = v; Invalidate(); }, EaseType.CubicOut);
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

    public CheckBox WithWidth(float width) { Width = width; return Self; }
    public CheckBox WithHeight(float height) { Height = height; return Self; }
    public CheckBox WithText(string text) { Text = text; return Self; }
    public CheckBox WithFontSize(float size) { FontSize = size; return Self; }
    public CheckBox WithIsChecked(bool isChecked) { IsChecked = isChecked; return Self; }
    public CheckBox WithBoxSize(float size) { BoxSize = size; return Self; }
    public CheckBox WithBorderWidth(float width) { BorderWidth = width; return Self; }
    public CheckBox WithTextGap(float gap) { TextGap = gap; return Self; }

    public CheckBox OnToggled(Action<bool> handler) { WhenToggled += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _checkTween.Cancel();
        _hoverTween.Cancel();
        _font.Dispose();
        _borderPaint.Dispose();
        _fillPaint.Dispose();
        _checkPaint.Dispose();
        _textPaint.Dispose();
    }

    #endregion
}