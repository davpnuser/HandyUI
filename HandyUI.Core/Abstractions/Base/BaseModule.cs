using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Components;
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

    protected virtual void OnStart() { }
    protected virtual void OnStop() { }

    protected virtual void OnMouse(MouseEventContext eventContext) { }
    protected virtual void OnKey(KeyEventContext eventContext) { }

    #endregion

    #region Methods

    public void Start()
    {
        Enabled = true;
        OnStart();
    }

    public void Stop()
    {
        Enabled = false;
        OnStop();
    }

    public void ProcessMouseEvent(MouseEventContext eventContext)
    {
        if (Enabled)
            OnMouse(eventContext);
    }

    public void ProcessKeyEvent(KeyEventContext eventContext)
    {
        if (Enabled)
            OnKey(eventContext);
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

public abstract class BaseModule<TSelf> : BaseModule where TSelf : BaseModule<TSelf>
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

    public TSelf WithEnabled(bool enabled)
    {
        return DoAndReturn(c => c.Enabled = enabled);
    }

    public TSelf WithAddToRenderer(UIRenderer renderer)
    {
        return DoAndReturn(c => renderer.AddModule(this));
    }
}

#endregion