using HandyUI.Core.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Interface;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class ListBox : BaseControl<ListBox>
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

    public float ItemSpacing
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            RecalculateLayout();
        }
    }

    public float ScrollSpeed { get; set; } = 40f;

    #endregion

    #region Internal Fields

    private float _scrollOffset;
    private float _contentHeight;
    private bool _lastHovered;

    #endregion

    #region Events

    public event Action? WhenClicked;
    public event Action? WhenHoverEntered;
    public event Action? WhenHoverExited;
    public event Action<float>? WhenScrollChanged;

    #endregion

    #region Constructor

    public ListBox()
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
        if (IsHovered) WhenHoverEntered?.Invoke();
        else WhenHoverExited?.Invoke();
    }

    public override void OnDraw(SKCanvas canvas) { }

    protected override void OnChildAdded(IControl control)
    {
        RecalculateLayout();
    }

    protected override void OnChildRemoved(IControl control)
    {
        RecalculateLayout();
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Wheel && IsHovered)
        {
            var delta = -ctx.WheelDelta / 120f * ScrollSpeed;
            ScrollTo(_scrollOffset + delta);
            return true;
        }

        if (ctx.Type == MouseEventType.Click && IsHovered)
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
        Bounds = new SKRect(0, 0, Width, Height);
    }

    private void RecalculateLayout()
    {
        var y = 0f;
        foreach (var child in Children)
        {
            child.Location = new SKPoint(0, y);
            y += child.Bounds.Height + ItemSpacing;
        }
        _contentHeight = y;
        Invalidate();
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
        Invalidate();
    }

    #endregion

    #region Fluent APIs

    public ListBox WithWidth(float width) { Width = width; return Self; }
    public ListBox WithHeight(float height) { Height = height; return Self; }
    public ListBox WithItemSpacing(float spacing) { ItemSpacing = spacing; return Self; }
    public ListBox WithScrollSpeed(float speed) { ScrollSpeed = speed; return Self; }

    public ListBox OnClicked(Action handler) { WhenClicked += handler; return Self; }
    public ListBox OnHoverEntered(Action handler) { WhenHoverEntered += handler; return Self; }
    public ListBox OnHoverExited(Action handler) { WhenHoverExited += handler; return Self; }
    public ListBox OnScrollChanged(Action<float> handler) { WhenScrollChanged += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose() { }

    #endregion
}