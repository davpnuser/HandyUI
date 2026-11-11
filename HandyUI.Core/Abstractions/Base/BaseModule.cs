using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Data;

namespace HandyUI.Core.Abstractions.Base;

public abstract class BaseModule : IModule
{
    #region Properties

    public string? Name { get; set; }
    public bool Enabled { get; set; } = true;
    public bool AcceptsInput { get; set; } = true;

    public bool IsDisposed { get; private set; }

    #endregion

    #region Overrideables

    public abstract void OnUpdate(UpdateContext updateContext);

    protected virtual bool OnMouse(MouseEventContext eventContext) => false;
    protected virtual bool OnKey(KeyEventContext eventContext) => false;

    #endregion

    #region Methods

    public bool ProcessMouseEvent(MouseEventContext eventContext) => Enabled && OnMouse(eventContext);
    public bool ProcessKeyEvent(KeyEventContext eventContext) => Enabled && OnKey(eventContext);

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

public abstract class BaseModule<TSelf> : BaseModule where TSelf : BaseModule<TSelf>
{
    protected TSelf Self => (TSelf)this;

    public TSelf DoAndReturn(Action<TSelf> action)
    {
        action?.Invoke(Self);
        return Self;
    }

    public TSelf WithName(string name)
        => DoAndReturn(c => c.Name = name);

    public TSelf WithEnabled(bool enabled)
        => DoAndReturn(c => c.Enabled = enabled);

    // TODO: Uncomment this once the UI Renderer is done.

    //public TSelf WithAddToRenderer(UIRenderer renderer)
    //    => DoAndReturn(c => renderer.AddRootControl(this));
}

#endregion