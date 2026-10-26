# Input Contexts

Records describing mouse and keyboard events passed to controls.

## Mouse

### `MouseEventContext`

`HandyUI.Core.Data.MouseEventContext`

```csharp
public record MouseEventContext(
    SKPoint? ClientPosition,
    MouseEventType Type,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0);
```

| Field | Description |
| --- | --- |
| `ClientPosition` | Cursor position in the receiving control's local space, or **`null`** when clipped out of this subtree. |
| `Type` | Event kind (`Move`, `MouseDown`, `MouseUp`, `Click`, `Wheel`). |
| `Button` | Which mouse button triggered the event. |
| `WheelDelta` | Wheel delta in "clicks" (±120 per notch). |

!!! info "Null means clipped"
    `ClientPosition` is `null` - not a sentinel - when the control is
    outside its parent's clip region. Treat null as *"not hovered"* and
    return `false` from `DoesIntersect`.

### Enums

`HandyUI.Core.Enums.MouseEventType`

```csharp
public enum MouseEventType { Move, MouseDown, MouseUp, Click, Wheel }
```

`HandyUI.Core.Enums.MouseButton`

```csharp
public enum MouseButton { None, Left, Right, Middle }
```

## Keyboard

### `KeyEventContext`

`HandyUI.Core.Data.KeyEventContext`

```csharp
public record KeyEventContext(
    KeyEventType Type,
    int KeyCode,
    char Character = '\0',
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false);
```

| Field | Description |
| --- | --- |
| `Type` | `KeyDown`, `KeyUp`, or `CharInput`. |
| `KeyCode` | Virtual key code (Win32-style). |
| `Character` | Printable char for `CharInput`; `'\0'` otherwise. |
| Modifiers | Current modifier state. |

!!! tip "Insert text on `CharInput`"
    Text editors should insert characters on `CharInput`, and use `KeyDown`
    for navigation (arrows, tab, enter, …). This separates layout behavior
    from OS-specific key codes.

### Enum

`HandyUI.Core.Enums.KeyEventType`

```csharp
public enum KeyEventType { KeyDown, KeyUp, CharInput }
```