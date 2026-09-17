using HandyUI.Core.Classes.Records;
using HandyUI.Core.Interfaces;
using SkiaSharp;

namespace HandyUI.Core.Classes.Base;

public abstract class UIControlBase : IUIControl
{
    private bool _isDisposed;

    private readonly List<IUIControl> _children = [];
    private bool _childrenDirty = false;

    public event Action? Invalidated;

    public void Invalidate()
    {
        Invalidated?.Invoke();
        Parent?.Invalidate();
    }

    public IReadOnlyList<IUIControl> Children => _children.AsReadOnly();

    public UIControlBase? Parent
    {
        get;
        set
        {
            if (field == value) return;

            if (field is UIControlBase oldBase)
            {
                oldBase.RemoveChildInternal(this);
            }

            field = value;

            if (field is UIControlBase newBase)
            {
                newBase.AddChildInternal(this);
            }

            Invalidate();
        }
    }

    internal void AddChildInternal(IUIControl child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
            _childrenDirty = true;
            OnChildAdded(child);
            Invalidate();
        }
    }

    internal void RemoveChildInternal(IUIControl child)
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

    protected virtual void OnChildAdded(IUIControl control) { }
    protected virtual void OnChildRemoved(IUIControl control) { }

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

    public bool InheritedPositioningEnabled { get; set; } = true;
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

    public UIControlBase WithLocation(SKPoint location)
    {
        Location = location;
        return this;
    }

    public UIControlBase WithLocation(float x, float y)
    {
        Location = new SKPoint(x, y);
        return this;
    }

    public event Action<IUIControl>? FocusRequested;

    public void RequestFocus()
    {
        if (!IsEnabled || !IsVisible) return;
        FocusRequested?.Invoke(this);
    }

    public bool ProcessMouseEvent(MouseEventContext mouseContext)
    {
        if (!IsVisible || !IsEnabled)
            return false;

        IsHovered = Intersects(mouseContext.ClientPosition);

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

    protected virtual bool OnMouse(MouseEventContext mouseContext) => false;
    protected virtual bool OnKey(KeyEventContext keyContext) => false;

    public abstract bool Intersects(SKPoint clientPoint);
    public abstract void Update(float deltaTime, SKPoint clientMousePosition);
    public abstract void Draw(SKCanvas canvas);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            OnDispose();
        }

        _isDisposed = true;
    }

    protected virtual void OnDispose() { }
}