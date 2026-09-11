using HandyUI.Core.Classes.Records;
using HandyUI.Core.Interfaces;
using SkiaSharp;

namespace HandyUI.Core.Classes.Base;

public abstract class UIControlBase : IUIControl
{
    private bool _isDisposed;

    public IUIControl? Parent { get; set; }
    public SKPoint Location { get; set; } = SKPoint.Empty;
    public SKRect Bounds { get; set; }
    public int ZIndex { get; set; }
    public bool RetainedModePositioning { get; set; } = true;

    public bool IsHovered { get; private set; }
    public bool IsMouseDown { get; private set; }
    public bool IsFocused { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;

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