using HandyUI.Core.v1.Enums;
using SkiaSharp;

namespace HandyUI.Core.v1.Interface;

public interface IWindowHost
{
    #region Properties

    string Title { get; set; }
    SKImage? Icon { get; set; }
    bool UseDirtyRendering { get; set; }
    bool VSync { get; set; }
    int FramesPerSecond { get; set; }

    SKSize Size { get; }
    SKRect WorkArea { get; }
    DeviceType DeviceType { get; }
    float DpiScale { get; }
    bool IsForeground { get; }
    int DisplayDist { get; }
    DisplayOrientation Orientation { get; }

    #endregion

    #region Events

    event Action? WhenResized;
    event Action? WhenForegroundChanged;
    event Action? WhenOrientationChanged;
    event Action? WhenWorkAreaChanged;
    event Action? WhenDpiScaleChanged;

    #endregion
}