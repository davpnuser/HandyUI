using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.Core.Data;

public readonly record struct MouseEventContext(
    TweenManager TweenManager,
    SKPoint? MousePosition,
    MouseEventType Type = MouseEventType.None,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0,
    int ClickCount = 1,
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false
);