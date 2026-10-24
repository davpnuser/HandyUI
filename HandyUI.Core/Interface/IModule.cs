using HandyUI.Core.Components;

namespace HandyUI.Core.Interface;

public interface IModule : IDisposable
{
    #region Properties

    bool IsEnabled { get; }

    #endregion

    #region Methods

    void Start();
    void Stop();

    #endregion

    #region Abstract/Virtual Methods

    void OnUpdate(float deltaTime);

    #endregion
}

#region Fluent APIs

public interface IModule<TSelf> : IModule where TSelf : IModule<TSelf>
{
    TSelf WithIsEnabled(bool enabled);
    TSelf WithAddToRenderer(UIRenderer renderer);
}

#endregion