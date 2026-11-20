namespace HandyUI.Core.Data.Input;

public enum KeyEventType
{
    None = 0,
    KeyDown = 1,
    KeyUp = 2,
    CharInput = 3
}

[Flags]
public enum ModifierKeys
{
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4,
    Super = 8,
    CapsLock = 16,
}

public readonly record struct KeyEventContext(
    EventData EventData,
    KeyEventType Type,
    ModifierKeys ModifierKeys,
    int? KeyCode = null,
    char? Character = null
);