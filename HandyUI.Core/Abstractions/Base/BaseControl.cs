using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.v1.Data;
using HandyUI.Core.v1.Enums;
using HandyUI.Core.v1.Helper;
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
            {
                oldBase.RemoveChildInternal(this);
            }

            field = value;

            if (field is BaseControl newBase)
            {
                newBase.AddChildInternal(this);
            }

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
    public bool Focusable { get; set; }

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

    public IReadOnlyList<IControl> Children => _children.AsReadOnly();

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

    #endregion

    #region Events

    public event Action? WhenInvalidated;
    public event Action<IControl?>? WhenFocusRequested;

    #endregion

    #region Overrideables

    public abstract void OnDraw(SKCanvas canvas);
    public abstract void OnUpdate(UpdateContext updateContext);

    public virtual bool OnMouse(MouseEventContext eventContext) => false;

    public virtual bool OnKey(KeyEventContext eventContext) => false;

    public virtual void OnChildAdded(IControl child) { }
    public virtual void OnChildRemoved(IControl child) { }

    #endregion

    #region Methods

    public bool ProcessKeyEvent(KeyEventContext eventContext) => Visible && Enabled && IsFocused && OnKey(eventContext);

    public bool ProcessMouseEvent(MouseEventContext eventContext)
    {
        if (!Visible || !Enabled)
            return false;

        IsHovered = Bounds.Contains(eventContext.Position);

        if (eventContext.Type == MouseEventType.MouseDown && IsHovered)
        {
            IsMouseDown = true;
        }
        else if (eventContext.Type == MouseEventType.MouseUp)
        {
            IsMouseDown = false;
        }

        return OnMouse(eventContext);
    }

    public SKPoint GetGlobalLocation()
    {
        var currentOffset = Location;

        if (Parent != null)
            currentOffset += Parent.GetGlobalLocation();

        return currentOffset;
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
        if (IsDisposed || !Enabled || !Visible)
            return;

        WhenFocusRequested?.Invoke(this);
    }

    public void AddChildInternal(IControl child)
    {
        if (child.IsDisposed)
            return;

        if (!Children.Contains(child))
        {
            _children.Add(child);
            _childrenDirty = true;
            OnChildAdded(child);
            Invalidate();
        }
    }

    public void RemoveChildInternal(IControl child)
    {
        if (child.IsDisposed)
            return;

        if (_children.Remove(child))
        {
            _childrenDirty = true;
            OnChildRemoved(child);
            Invalidate();
        }
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
        => DoAndReturn(c => c.Name = name);

    public TSelf WithParent(IControl parent)
        => DoAndReturn(c => c.Parent = parent);

    public TSelf WithParent(BaseControl parent)
        => DoAndReturn(c => c.Parent = parent);

    public TSelf WithLocation(SKPoint location)
        => DoAndReturn(c => c.Location = location);

    public TSelf WithLocation(float x, float y)
        => DoAndReturn(c => c.Location = new(x, y));

    public TSelf WithBounds(SKRect bounds)
        => DoAndReturn(c => c.Bounds = bounds);

    public TSelf WithBounds(SKSize boundsSize)
        => DoAndReturn(c => c.Bounds = boundsSize.ToSKRect());

    public TSelf WithBounds(float x, float y, float w, float h)
        => DoAndReturn(c => c.Bounds = new(x, y, w, h));

    public TSelf WithOpacity(float opacity)
        => DoAndReturn(c => c.Opacity = opacity);

    public TSelf WithVisible(bool visibility)
        => DoAndReturn(c => c.Visible = visibility);

    public TSelf WithEnabled(bool enabled)
        => DoAndReturn(c => c.Enabled = enabled);

    public TSelf WithZIndex(int zIndex)
        => DoAndReturn(c => c.ZIndex = zIndex);

    // TODO: Uncomment this once the UI Renderer is done.

    //public TSelf WithAddToRenderer(UIRenderer renderer)
    //    => DoAndReturn(c => renderer.AddRootControl(this));
}

#endregion