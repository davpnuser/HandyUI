using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class Label : BaseControl<Label>
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

    public SKTextAlign TextAlign
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = SKTextAlign.Left;

    #endregion

    #region Internal Fields

    private readonly SKFont _font;
    private readonly SKPaint _textPaint = new() { IsAntialias = true };

    #endregion

    #region Constructor

    public Label()
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

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition) { }

    public override void OnDraw(SKCanvas canvas)
    {
        if (string.IsNullOrEmpty(Text)) return;

        _textPaint.Color = IsEnabled
            ? ThemeManager.Instance.TextPrimary
            : ThemeManager.Instance.TextDisabled;

        var fm = _font.Metrics;
        var baseline = (Bounds.Height * 0.5f) - ((fm.Ascent + fm.Descent) * 0.5f);

        var x = TextAlign switch
        {
            SKTextAlign.Center => Bounds.Width * 0.5f,
            SKTextAlign.Right => Bounds.Width,
            _ => 0f
        };

        canvas.DrawText(Text, x, baseline, TextAlign, _font, _textPaint);
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var fm = _font.Metrics;
        var contentW = string.IsNullOrEmpty(Text) ? 0f : _font.MeasureText(Text);
        var contentH = fm.Descent - fm.Ascent;
        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    #endregion

    #region Fluent APIs

    public Label WithWidth(float width) { Width = width; return Self; }
    public Label WithHeight(float height) { Height = height; return Self; }
    public Label WithText(string text) { Text = text; return Self; }
    public Label WithFontSize(float size) { FontSize = size; return Self; }
    public Label WithIsBold(bool bold) { IsBold = bold; return Self; }
    public Label WithTextAlign(SKTextAlign align) { TextAlign = align; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _font.Dispose();
        _textPaint.Dispose();
    }

    #endregion
}