using HandyUI.Core.Enums;

namespace HandyUI.Core.Data;

public record KeyEventContext(
    KeyEventType Type,
    int KeyCode,
    char Character = '\0',
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false
);
