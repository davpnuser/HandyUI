using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;

namespace HandyUI.Core.Data;

public readonly record struct KeyEventContext(
    TweenManager Tweener,
    KeyEventType Type = KeyEventType.None,
    int? KeyCode = null,
    char Character = '\0',
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false
);