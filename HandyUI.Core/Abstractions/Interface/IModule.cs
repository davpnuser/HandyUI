using HandyUI.Core.Data;

namespace HandyUI.Core.Abstractions.Interface;

public interface IModule : IDisposable
{
    #region Properties

    string? Name { get; set; }
    bool Enabled { get; set; }
    bool IsDisposed { get; }

    #endregion

    #region Overrideables

    abstract void OnUpdate(UpdateContext updateContext);

    #endregion

    #region Methods

    void Start();
    void Stop();

    bool ProcessMouseEvent(MouseEventContext eventContext);
    bool ProcessKeyEvent(KeyEventContext eventContext);

    #endregion
}