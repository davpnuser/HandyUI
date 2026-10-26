using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Interface;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class ScrollFrame : BaseControl<ScrollFrame>
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
    } = 200f;

    public float ScrollSpeed { get; set; } = 40f;
    public float ScrollbarWidth { get; set; } = 4f;
    public float ScrollbarMinThumbHeight { get; set; } = 24f;

    #endregion

    #region Internal Fields

    private readonly SKPaint _scrollbarPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private float _scrollOffset;
    private float _contentHeight;
    private float _scrollbarAlpha;
    private TweenHandle _scrollbarTween;

    #endregion

    #region Events

    public event Action<float>? WhenScrollChanged;

    #endregion

    #region Constructor

    public ScrollFrame()
    {
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
        if (_contentHeight <= Bounds.Height) return;
        if (_scrollbarAlpha <= 0.01f) return;

        var trackHeight = Bounds.Height;
        var thumbHeight = Math.Max(ScrollbarMinThumbHeight, trackHeight * (Bounds.Height / _contentHeight));
        var maxScroll = _contentHeight - Bounds.Height;
        var thumbY = maxScroll > 0 ? _scrollOffset / maxScroll * (trackHeight - thumbHeight) : 0;

        var thumbX = Bounds.Width - ScrollbarWidth - 2f;

        _scrollbarPaint.Color = ThemeManager.Instance.TextPrimary.WithAlpha((byte)(80 * _scrollbarAlpha));

        var thumbRect = new SKRect(
            thumbX, thumbY,
            thumbX + ScrollbarWidth, thumbY + thumbHeight);

        canvas.DrawRoundRect(thumbRect, ScrollbarWidth * 0.5f, ScrollbarWidth * 0.5f, _scrollbarPaint);
    }

    protected override void OnChildAdded(IControl control)
    {
        UpdateContentHeight();
    }

    protected override void OnChildRemoved(IControl control)
    {
        UpdateContentHeight();
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Wheel && IsHovered)
        {
            var delta = -ctx.WheelDelta / 120f * ScrollSpeed;
            ScrollTo(_scrollOffset + delta);
            return true;
        }
        return false;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        Bounds = new SKRect(0, 0, Width, Height);
    }

    private void UpdateContentHeight()
    {
        var max = 0f;
        foreach (var child in Children)
        {
            var bottom = child.Location.Y + child.Bounds.Height;
            if (bottom > max) max = bottom;
        }
        _contentHeight = max;
    }

    private void ScrollTo(float newOffset)
    {
        var maxOffset = Math.Max(0f, _contentHeight - Bounds.Height);
        newOffset = Math.Clamp(newOffset, 0f, maxOffset);
        if (Math.Abs(newOffset - _scrollOffset) < 0.001f) return;

        var delta = newOffset - _scrollOffset;
        _scrollOffset = newOffset;

        foreach (var child in Children)
            child.Location = new SKPoint(child.Location.X, child.Location.Y - delta);

        WhenScrollChanged?.Invoke(_scrollOffset);
        ShowScrollbar();
        Invalidate();
    }

    private void ShowScrollbar()
    {
        _scrollbarAlpha = 1f;
        if (Tweener.Current is not { } tweener) return;
        _scrollbarTween.Cancel();
        _scrollbarTween = tweener.To(1f, 0f, 0.8f,
            v => { _scrollbarAlpha = v; Invalidate(); }, EaseType.SineOut);
    }

    #endregion

    #region Fluent APIs

    public ScrollFrame WithWidth(float width) { Width = width; return Self; }
    public ScrollFrame WithHeight(float height) { Height = height; return Self; }
    public ScrollFrame WithScrollSpeed(float speed) { ScrollSpeed = speed; return Self; }
    public ScrollFrame WithScrollbarWidth(float width) { ScrollbarWidth = width; return Self; }

    public ScrollFrame OnScrollChanged(Action<float> handler) { WhenScrollChanged += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _scrollbarTween.Cancel();
        _scrollbarPaint.Dispose();
    }

    #endregion
}