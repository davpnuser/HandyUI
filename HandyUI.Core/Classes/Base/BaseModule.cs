using HandyUI.Core.Components;
using HandyUI.Core.Interfaces;

namespace HandyUI.Core.Classes.Base;

public abstract class BaseModule : IModule
{
    #region Properties

    public bool IsEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            OnStateChanged?.Invoke(value);
            field = value;
        }
    }

    #endregion

    #region Events

    public event Action<float>? OnUpdate;
    public event Action<bool>? OnStateChanged;

    #endregion

    #region Internal Fields

    private bool _isDisposed;

    #endregion

    #region Methods

    public void Start()
    {
        IsEnabled = true;
    }

    public void Stop()
    {
        IsEnabled = false;
    }

    #endregion

    #region Abstract/Virtual Methods

    public abstract void Update(float deltaTime);

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

    public TSelf WithOnUpdate(Action<float>? action)
    {
        OnUpdate += action;
        return Self;
    }

    public TSelf WithOnStateChanged(Action<bool>? action)
    {
        OnStateChanged += action;
        return Self;
    }

    public TSelf WithAddToRenderer(UIRenderer renderer)
    {
        renderer.AddModule(Self);
        return Self;
    }
}

#endregion