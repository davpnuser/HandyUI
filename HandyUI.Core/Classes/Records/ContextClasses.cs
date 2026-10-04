using SkiaSharp;

namespace HandyUI.Core.Classes.Records;

#region Mouse Context Enums

public enum MouseButton
{
    None,
    Left,
    Right,
    Middle
}

public enum MouseEventType
{
    Move,
    MouseDown,
    MouseUp,
    Click,
    Wheel
}

public record MouseEventContext(
    SKPoint ClientPosition,
    MouseEventType Type,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0
);

#endregion

#region Keyboard Context Enums

public enum KeyEventType
{
    KeyDown,
    KeyUp,
    CharInput
}

public record KeyEventContext(
    KeyEventType Type,
    int KeyCode,
    char Character = '\0',
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false
);

#endregion