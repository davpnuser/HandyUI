using SkiaSharp;

namespace HandyUI.Core.Data.Input;

public enum MouseEventType
{
    None = 0,
    ClickEvent = 1,
    DownEvent = 2,
    UpEvent = 3,
    ScrollEvent = 4,
    MoveEvent = 5
}

public enum MouseButton
{
    None = 0,
    Left = 1,
    Right = 2,
    Wheel = 3,
    Generic = 4,
}

public readonly record struct MouseEventContext(
    EventData EventData,
    MouseEventType Type,
    ModifierKeys ModifierKeys,
    MouseButton Button,
    SKPoint MousePosition,
    int WheelDelta = 0,
    int ClickCount = 1
);