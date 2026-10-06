using HandyUI.Core.Components;

namespace HandyUI.Core.Interfaces;

public interface IModule : IDisposable
{
    #region Properties

    bool IsEnabled { get; set; }

    #endregion

    #region Events

    event Action<float>? OnUpdate;
    event Action<bool>? OnStateChanged;

    #endregion

    #region Methods

    void Start();
    void Stop();

    #endregion

    #region Abstract/Virtual Methods

    void Update(float deltaTime);

    #endregion
}

#region Fluent APIs

public interface IModule<TSelf> : IModule where TSelf : IModule<TSelf>
{
    TSelf WithIsEnabled(bool enabled);
    TSelf WithOnUpdate(Action<float>? action);
    TSelf WithOnStateChanged(Action<bool>? action);
    TSelf WithAddToRenderer(UIRenderer renderer);
}

#endregion