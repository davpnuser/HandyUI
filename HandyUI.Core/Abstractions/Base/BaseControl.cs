using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using HandyUI.Core.Helpers.Extensions;
using SkiaSharp;

namespace HandyUI.Core.Abstractions.Base;

public abstract class BaseControl : IControl
{
    #region Properties

    public string? Name { get; set; }

    public IControl? Parent
    {
        get;
        set
        {
            if (field == value) return;

            if (field is BaseControl oldBase)
                oldBase.RemoveChildInternal(this);

            field = value;

            if (field is BaseControl newBase)
                newBase.AddChildInternal(this);

            Invalidate();
        }
    }

    public SKPoint Location
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = SKPoint.Empty;

    public SKRect Bounds
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    }

    public int ZIndex
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    }

    public bool Enabled
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    public bool Visible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    public bool ClipChildren { get; set; }
    public bool Focusable { get; set; } = true;

    public float Opacity
    {
        get;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(field - clamped) < 0.0001f) return;
            field = clamped;
            AlphaPaint.Color = SKColors.White.WithAlpha((byte)(255 * field));
            Invalidate();
        }
    } = 1.0f;

    #endregion

    #region Managed Properties

    public IReadOnlyList<IControl> Children => _children;

    public bool IsDisposed { get; private set; }

    public bool IsHovered
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    }

    public bool IsFocused
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    }

    public bool IsMouseDown
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    }

    public SKPaint AlphaPaint { get; } = new();

    #endregion

    #region Internal Fields

    private readonly List<IControl> _children = [];
    private bool _childrenDirty = false;

    private readonly SKPaint _focusRectPaint = new()
    {
        IsAntialias = false,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
    };

    #endregion

    #region Events

    public event Action? WhenInvalidated;
    public event Action<IControl?, bool>? WhenFocusRequested;

    #endregion

    #region Overrideables

    public abstract bool DoesIntersect(SKPoint point);
    public abstract void OnDraw(SKCanvas canvas);
    public virtual void OnUpdate(UpdateContext updateContext) { }

    protected virtual bool OnMouse(MouseEventContext eventContext)
    {
        return false;
    }

    protected virtual bool OnKey(KeyEventContext eventContext)
    {
        return false;
    }

    protected virtual void OnChildAdded(IControl child) { }
    protected virtual void OnChildRemoved(IControl child) { }

    protected virtual void OnDrawFocusRect(SKCanvas canvas)
    {
        var rect = Bounds;
        rect.Inflate(-1.5f, -1.5f);
        _focusRectPaint.Color = ThemeManager.Instance.Accent;
        canvas.DrawRect(rect, _focusRectPaint);
    }

    #endregion

    #region Methods

    public bool ProcessKeyEvent(KeyEventContext eventContext)
    {
        return Visible && Enabled && IsFocused && OnKey(eventContext);
    }

    public bool ProcessMouseEvent(MouseEventContext eventContext)
    {
        if (!Visible || !Enabled)
            return false;

        IsHovered = eventContext.MousePosition != null
            && Bounds.Contains((SKPoint)eventContext.MousePosition);

        if (eventContext.Type == MouseEventType.MouseDownEvent && IsHovered)
            IsMouseDown = true;
        else if (eventContext.Type == MouseEventType.MouseUpEvent)
            IsMouseDown = false;

        return OnMouse(eventContext);
    }

    public SKPoint GetGlobalLocation()
    {
        var currentOffset = Location;

        if (Parent != null)
            currentOffset += Parent.GetGlobalLocation();

        return currentOffset;
    }

    public SKPoint GetLocalPosition(SKPoint point)
    {
        var globalPosition = GetGlobalLocation();
        return point - globalPosition;
    }

    public void InvalidateChildrenOrder()
    {
        _childrenDirty = true;
        Invalidate();
    }

    internal void EnsureChildrenSorted()
    {
        if (_childrenDirty)
        {
            _children.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            _childrenDirty = false;
        }
    }

    public void Invalidate()
    {
        if (IsDisposed || !Visible)
            return;

        WhenInvalidated?.Invoke();
        Parent?.Invalidate();
    }

    public void RequestFocus()
    {
        if (IsDisposed || !Enabled || !Visible || !Focusable)
            return;

        WhenFocusRequested?.Invoke(this, false);
    }

    private void AddChildInternal(BaseControl child)
    {
        if (child.IsDisposed) return;

        if (!Children.Contains(child))
        {
            _children.Add(child);
            _childrenDirty = true;
            OnChildAdded(child);
            Invalidate();
        }
    }

    private void RemoveChildInternal(BaseControl child)
    {
        if (child.IsDisposed) return;

        if (_children.Remove(child))
        {
            _childrenDirty = true;
            OnChildRemoved(child);
            Invalidate();
        }
    }

    public void DrawFocusRect(SKCanvas canvas)
    {
        if (!IsFocused) return;
        OnDrawFocusRect(canvas);
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            OnDispose();
            _focusRectPaint.Dispose();
        }

        IsDisposed = true;
    }

    protected virtual void OnDispose() { }

    #endregion
}

#region Fluent APIs

public abstract class BaseControl<TSelf> : BaseControl where TSelf : BaseControl<TSelf>
{
    protected TSelf Self => (TSelf)this;

    public TSelf DoAndReturn(Action<TSelf> action)
    {
        action?.Invoke(Self);
        return Self;
    }

    public TSelf WithName(string name)
    {
        return DoAndReturn(c => c.Name = name);
    }

    public TSelf WithParent(IControl parent)
    {
        return DoAndReturn(c => c.Parent = parent);
    }

    public TSelf WithParent(BaseControl parent)
    {
        return DoAndReturn(c => c.Parent = parent);
    }

    public TSelf WithLocation(SKPoint location)
    {
        return DoAndReturn(c => c.Location = location);
    }

    public TSelf WithLocation(float x, float y)
    {
        return DoAndReturn(c => c.Location = new(x, y));
    }

    public TSelf WithBounds(SKRect bounds)
    {
        return DoAndReturn(c => c.Bounds = bounds);
    }

    public TSelf WithBounds(SKSize boundsSize)
    {
        return DoAndReturn(c => c.Bounds = boundsSize.ToSKRect());
    }

    public TSelf WithBounds(float x, float y, float w, float h)
    {
        return DoAndReturn(c => c.Bounds = new(x, y, w, h));
    }

    public TSelf WithOpacity(float opacity)
    {
        return DoAndReturn(c => c.Opacity = opacity);
    }

    public TSelf WithVisible(bool visibility)
    {
        return DoAndReturn(c => c.Visible = visibility);
    }

    public TSelf WithEnabled(bool enabled)
    {
        return DoAndReturn(c => c.Enabled = enabled);
    }

    public TSelf WithZIndex(int zIndex)
    {
        return DoAndReturn(c => c.ZIndex = zIndex);
    }

    public TSelf WithFocusable(bool focusable)
    {
        return DoAndReturn(c => c.Focusable = focusable);
    }

    public TSelf WithAddToRenderer(UIRenderer renderer)
    {
        return DoAndReturn(c => renderer.AddControl(this));
    }
}

#endregion