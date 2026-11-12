using HandyUI.Core.Components;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
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
    DeviceType DeviceType { get; }
    DisplayOrientation Orientation { get; }
    float DpiScale { get; }
    float DisplayDist { get; }

    TweenManager TweenManager { get; set; }
    UIRenderer? Renderer { get; }

    double TotalTime { get; }

    #endregion

    #region Events

    event Action<DisplayOrientation>? WhenOrientationChanged;

    #endregion

    #region Methods

    void Invalidate();

    #endregion
}