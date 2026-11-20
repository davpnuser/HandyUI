using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Data.Input;
using SkiaSharp;

namespace HandyUI.Core.Abstractions.Interface;

public interface IHostWindow
{
    #region Properties

    string Title { get; set; }
    SKImage? Icon { get; set; }
    SKSize Size { get; set; }

    bool UseDirtyRendering { get; set; }
    bool VSync { get; set; }
    int FramesPerSecond { get; set; }
    CursorType Cursor { get; set; }

    bool IsForeground { get; }
    DeviceInfo DeviceInfo { get; }

    UIRenderer? Renderer { get; }

    double TotalTime { get; }

    #endregion

    #region Events

    event Action? WhenLoaded;
    event Action? WhenClosing;

    #endregion

    #region Methods

    void Invalidate();
    ModifierKeys GetModifierKeys();
    void CleanMemory(bool allResources = true);

    #endregion
}