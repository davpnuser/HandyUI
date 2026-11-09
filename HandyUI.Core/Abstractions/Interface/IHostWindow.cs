using HandyUI.Core.Data.Enums;
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

    bool IsForeground { get; }
    DeviceType DeviceType { get; }
    DisplayOrientation Orientation { get; }
    float DpiScale { get; }
    float DisplayDist { get; }

    #endregion

    #region Events

    event Action<bool>? WhenFocusChanged;
    event Action<DisplayOrientation>? WhenOrientationChanged;
    event Action? WhenInvalidated;
    event Action? WhenHidden;
    event Action? WhenShown;

    #endregion

    #region Methods

    void Invalidate();
    bool TryShow();
    bool TryHide();

    #endregion
}