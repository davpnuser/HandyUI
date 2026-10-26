using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Helper;
using HandyUI.Core.Interface;
using SkiaSharp;

namespace HandyUI.Core.Base;

public abstract class BaseControl : IControl
{
    #region Properties

    public void Invalidate()
    {
        WhenInvalidated?.Invoke();
        Parent?.Invalidate();
    }

    public IReadOnlyList<IControl> Children => _children.AsReadOnly();

    public BaseControl? Parent
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

    public bool ParentChildPositioningEnabled { get; set; } = true;
    public bool ScissoringEnabled { get; set; } = true;

    public SKPaint AlphaPaint { get; } = new();

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

    public bool IsVisible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    public bool IsEnabled
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    #region Events

    public event Action? WhenInvalidated;
    public event Action<IControl>? WhenFocusRequested;

    #endregion

    #endregion

    #region Internal Fields

    private bool _isDisposed;

    private readonly List<IControl> _children = [];
    private bool _childrenDirty = false;

    #endregion

    #region Internal Functions

    internal void AddChildInternal(IControl child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
            _childrenDirty = true;
            OnChildAdded(child);
            Invalidate();
        }
    }

    internal void RemoveChildInternal(IControl child)
    {
        if (_children.Remove(child))
        {
            _childrenDirty = true;
            OnChildRemoved(child);
            Invalidate();
        }
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

    protected virtual void OnChildAdded(IControl control) { }
    protected virtual void OnChildRemoved(IControl control) { }

    #endregion

    #region Methods

    public void RequestFocus()
    {
        if (!IsEnabled || !IsVisible) return;
        WhenFocusRequested?.Invoke(this);
    }

    public bool ProcessMouseEvent(MouseEventContext mouseContext)
    {
        if (!IsVisible || !IsEnabled)
            return false;

        IsHovered = DoesIntersect(mouseContext.ClientPosition);

        if (mouseContext.Type == MouseEventType.MouseDown && IsHovered)
        {
            IsMouseDown = true;
        }
        else if (mouseContext.Type == MouseEventType.MouseUp)
        {
            IsMouseDown = false;
        }

        return OnMouse(mouseContext);
    }

    public bool ProcessKeyEvent(KeyEventContext keyContext)
    {
        return IsVisible && IsEnabled && IsFocused && OnKey(keyContext);
    }

    #endregion

    #region Abstract/Virtual Methods

    protected virtual bool OnMouse(MouseEventContext mouseContext)
    {
        return false;
    }

    protected virtual bool OnKey(KeyEventContext keyContext)
    {
        return false;
    }

    public abstract bool DoesIntersect(SKPoint? clientPoint);
    public abstract void OnUpdate(float deltaTime, SKPoint? clientMousePosition);
    public abstract void OnDraw(SKCanvas canvas);

    #endregion

    #region Disposal

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            OnDispose();
        }

        _isDisposed = true;
    }

    protected virtual void OnDispose() { }

    #endregion
}

#region Fluent APIs

public abstract class BaseControl<TSelf> : BaseControl where TSelf : BaseControl<TSelf>
{
    protected TSelf Self => (TSelf)this;

    public TSelf WithLocation(SKPoint location)
    {
        Location = location;
        return Self;
    }

    public TSelf WithLocation(float x, float y)
    {
        Location = new SKPoint(x, y);
        return Self;
    }

    public TSelf WithBounds(SKRect bounds)
    {
        Bounds = bounds;
        return Self;
    }

    public TSelf WithBounds(SKSize boundsSize)
    {
        Bounds = boundsSize.ToSKRect();
        return Self;
    }

    public TSelf WithBounds(float x, float y, float w, float h)
    {
        Bounds = new SKRect(x, y, x + w, y + h);
        return Self;
    }

    public TSelf WithParent(BaseControl parentControl)
    {
        Self.Parent = parentControl;
        return Self;
    }

    public TSelf WithOpacity(float opacity)
    {
        Self.Opacity = opacity;
        return Self;
    }

    public TSelf WithIsVisible(bool visibility)
    {
        Self.IsVisible = visibility;
        return Self;
    }

    public TSelf WithIsEnabled(bool enabled)
    {
        Self.IsEnabled = enabled;
        return Self;
    }

    public TSelf WithParentChildPositioningEnabled(bool parentChildPositioningEnabled)
    {
        Self.ParentChildPositioningEnabled = parentChildPositioningEnabled;
        return Self;
    }

    public TSelf WithZIndex(int zIndex)
    {
        Self.ZIndex = zIndex;
        return Self;
    }

    public TSelf WithAddToRenderer(UIRenderer renderer)
    {
        renderer.AddRootControl(this);
        return Self;
    }
}

#endregion
