# Input Contexts

Records describing mouse and keyboard events passed to controls.

## Mouse

```csharp
public enum MouseButton { None, Left, Right, Middle }

public enum MouseEventType { Move, MouseDown, MouseUp, Click, Wheel }
```

### `MouseEventContext`

```csharp
public record MouseEventContext(
    SKPoint ClientPosition,
    MouseEventType Type,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0);
```

| Field | Description |
| --- | --- |
| `ClientPosition` | Cursor position **in the receiving control's local space**. When clipped, the renderer substitutes `(-99999, -99999)`. |
| `Type` | Which kind of event this is. |
| `Button` | Which mouse button triggered the event (for button events). |
| `WheelDelta` | Wheel delta in "clicks". Typically ±120 per notch. |

!!! info "Synthetic clicks"
    The renderer synthesizes a `Click` event on `MouseUp` when the same
    control was pressed and is still under the cursor.

## Keyboard

```csharp
public enum KeyEventType { KeyDown, KeyUp, CharInput }
```

### `KeyEventContext`

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
| `Type` | Key down / up / printable char. |
| `KeyCode` | Virtual key code (Win32‑style). |
| `Character` | Printable char for `CharInput`, otherwise `'\0'`. |
| `IsControlPressed` / `IsShiftPressed` / `IsAltPressed` | Modifier state. |

!!! tip "Use `CharInput` for text"
    Text boxes should only insert characters on `CharInput`, not on `KeyDown`.
    `KeyDown` is for navigation (arrows, tab, enter, …).

## Example

```csharp
protected override bool OnMouse(MouseEventContext ctx)
{
    switch (ctx.Type)
    {
        case MouseEventType.Click:
            if (ctx.Button == MouseButton.Left) HandleLeftClick();
            return true;
        case MouseEventType.Wheel:
            Scroll(ctx.WheelDelta);
            return true;
    }
    return false;
}

protected override bool OnKey(KeyEventContext ctx)
{
    if (ctx.Type == KeyEventType.CharInput)
    {
        _buffer.Append(ctx.Character);
        return true;
    }
    return false;
}
```