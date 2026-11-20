using SkiaSharp;

namespace HandyUI.Core.Data.Input;

public enum MouseEventType
{
    None = 0,
    ClickEvent = 1,
    MouseDownEvent = 2,
    MouseUpEvent = 3,
    ScrollEvent = 4,
    MoveEvent = 5
}

public enum MouseButton
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 3,
    Wheel = 4,
    Generic = 5,
}

public readonly record struct MouseEventContext(
    EventData EventData,
    ModifierKeys ModifierKeys,
    MouseEventType Type,
    MouseButton Button,
    SKPoint MousePosition,
    int WheelDelta = 0,
    int ClickCount = 1
);