using HandyUI.Core.Classes.Records;
using HandyUI.Core.Interfaces;
using SkiaSharp;

namespace HandyUI.Core.Classes.Base;

public abstract class UIControlBase : IUIControl
{
    private bool _isDisposed;
    private readonly List<IUIControl> _children = [];

    public IUIControl? Parent
    {
        get;
        set
        {
            if (field == value) return;

            var oldParent = field;
            field = value;

            oldParent?.RemoveChild(this);
            field?.AddChild(this);
        }
    }

    public IReadOnlyList<IUIControl> Children => _children.AsReadOnly();

    public SKPoint Location { get; set; } = SKPoint.Empty;
    public SKRect Bounds { get; set; }

    public virtual float Width
    {
        get => Bounds.Width;
        set => Bounds = SKRect.Create(Bounds.Left, Bounds.Top, value, Bounds.Height);
    }

    public virtual float Height
    {
        get => Bounds.Height;
        set => Bounds = SKRect.Create(Bounds.Left, Bounds.Top, Bounds.Width, value);
    }

    public int ZIndex { get; set; }
    public bool RetainedModePositioning { get; set; } = true;
    public bool ClipChildren { get; set; } = true;

    public bool IsHovered { get; protected set; }
    public bool IsMouseDown { get; protected set; }
    public bool IsFocused { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;

    public event Action<IUIControl>? FocusRequested;
    public event Action<IUIControl>? ChildAdded;
    public event Action<IUIControl>? ChildRemoved;

    public void AddChild(IUIControl child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (!_children.Contains(child))
        {
            _children.Add(child);
            child.Parent = this;
            ChildAdded?.Invoke(child);
        }
    }

    public void RemoveChild(IUIControl child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (_children.Remove(child))
        {
            if (child.Parent == this)
            {
                child.Parent = null;
            }
            ChildRemoved?.Invoke(child);
        }
    }

    public virtual IUIControl? HitTest(SKPoint localPoint)
    {
        if (!IsVisible || !IsEnabled) return null;

        if (ClipChildren && !Intersects(localPoint))
        {
            return null;
        }

        var sortedChildren = _children.OrderByDescending(c => c.ZIndex).ToList();
        foreach (var child in sortedChildren)
        {
            var childLocalPoint = new SKPoint(localPoint.X - child.Location.X, localPoint.Y - child.Location.Y);
            var hit = child.HitTest(childLocalPoint);
            if (hit != null)
            {
                return hit;
            }
        }

        return Intersects(localPoint) ? this : (IUIControl?)null;
    }

    public void RequestFocus()
    {
        if (!IsEnabled || !IsVisible) return;
        FocusRequested?.Invoke(this);
    }

    public bool ProcessMouseEvent(MouseEventContext mouseContext)
    {
        if (!IsVisible || !IsEnabled)
        {
            IsMouseDown = false;
            return false;
        }

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

    public virtual void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!IsVisible || !IsEnabled)
        {
            IsHovered = false;
            return;
        }

        IUIControl root = this;
        var rootPoint = clientMousePosition;

        IUIControl? current = this;
        while (current.Parent != null)
        {
            rootPoint = new SKPoint(rootPoint.X + current.Location.X, rootPoint.Y + current.Location.Y);
            current = current.Parent;
            root = current;
        }

        var hitTarget = root.HitTest(rootPoint);
        IsHovered = hitTarget == this;
    }

    protected virtual bool OnMouse(MouseEventContext mouseContext) => false;
    protected virtual bool OnKey(KeyEventContext keyContext) => false;

    public abstract bool Intersects(SKPoint clientPoint);
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
            foreach (var child in _children.ToArray())
            {
                child.Dispose();
            }
            _children.Clear();

            OnDispose();
        }

        _isDisposed = true;
    }

    protected virtual void OnDispose() { }
}