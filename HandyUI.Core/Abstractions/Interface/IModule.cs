using HandyUI.Core.Data;

namespace HandyUI.Core.Abstractions.Interface;

public interface IModule : IDisposable
{
    #region Properties

    string? Name { get; set; }
    bool AcceptsInput { get; set; }
    bool Enabled { get; set; }
    bool IsDisposed { get; }

    #endregion

    #region Overrideables

    abstract void OnUpdate(UpdateContext updateContext);

    #endregion

    #region Methods

    void Start();
    void Stop();

    void ProcessMouseEvent(MouseEventContext eventContext);
    void ProcessKeyEvent(KeyEventContext eventContext);

    #endregion
}