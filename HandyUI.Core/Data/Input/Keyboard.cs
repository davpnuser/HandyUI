namespace HandyUI.Core.Data.Input;

public enum KeyEventType
{
    None = 0,
    Down = 1,
    Up = 2,
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
}

public readonly record struct KeyEventContext(
    EventData EventData,
    KeyEventType Type,
    ModifierKeys ModifierKeys,
    int KeyCode = 0,
    char Character = '\0'
);