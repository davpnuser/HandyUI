using HandyUI.Core.Components;
using HandyUI.Core.Interfaces;

namespace HandyUI.Core.Classes.Base;

public abstract class BaseModule : IModule
{
    #region Properties

    public bool IsEnabled { get; protected set; }

    #endregion

    #region Internal Fields

    private bool _isDisposed;

    #endregion

    #region Methods

    public void Start()
    {
        IsEnabled = true;
        OnStart();
    }

    public void Stop()
    {
        IsEnabled = false;
        OnStop();
    }

    #endregion

    #region Abstract/Virtual Methods

    public abstract void Update(float deltaTime);
    protected virtual void OnStart() { }
    protected virtual void OnStop() { }

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

public abstract class BaseModule<TSelf> : BaseModule where TSelf : BaseModule<TSelf>
{
    protected TSelf Self => (TSelf)this;

    public TSelf WithIsEnabled(bool enabled)
    {
        IsEnabled = enabled;
        return Self;
    }

    public TSelf WithAddToRenderer(UIRenderer renderer)
    {
        renderer.AddModule(Self);
        return Self;
    }
}

#endregion