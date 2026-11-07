using HandyUI.Core.v1.Components;
using HandyUI.Core.v1.Enums;

namespace HandyUI.Core.v1.Data;

public record KeyEventContext(
    KeyEventType Type,
    int KeyCode,
    char Character = '\0',
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false,
    Tweener Tweener = null!);