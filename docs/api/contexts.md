# Input Contexts

`HandyUI.Core.Data.MouseEventContext` and `KeyEventContext`.

Both are `readonly record struct` and carry a reference to the current
[`TweenManager`](tween-manager.md). Controls that need to start a tween from
inside `OnMouse` / `OnKey` can use `eventContext.TweenManager` directly:

```csharp
protected override bool OnMouse(MouseEventContext eventContext)
{
    if (eventContext.Type != MouseEventType.MouseDownEvent) return false;
    eventContext.TweenManager.To(0f, 1f, 0.3f, v => Animate(v));
    return true;
}
```

## `MouseEventContext`

```csharp
public readonly record struct MouseEventContext(
    TweenManager TweenManager,
    SKPoint? MousePosition,
    MouseEventType Type = MouseEventType.None,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0,
    int ClickCount = 1,
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false
);
```

| Field | Meaning |
| --- | --- |
| `TweenManager` | Current animation engine. |
| `MousePosition` | Local cursor position, or `null` when the subtree is clipped. |
| `Type` | See [`MouseEventType`](enums.md#mouseeventtype). |
| `Button` | Which button triggered the event. |
| `WheelDelta` | Wheel delta in clicks (±120 per notch). |
| `ClickCount` | 1, 2 (double), or 3 (triple) - computed by the adapter from timing + distance. |
| Modifiers | State at event time. |

!!! info "Null means clipped"
    `MousePosition` is `null` - not a sentinel coordinate - when the control
    is outside its clip region. `BaseControl.IsHovered` uses
    `MousePosition != null && Bounds.Contains(pos)`.

!!! tip "Synthesized clicks"
    `MouseEventType.ClickEvent` is **never sent by the adapter**. It is
    synthesized by `UIRenderer.ProcessMouseEvent` after a `MouseUpEvent`
    that occurs on the currently pressed control. Custom controls should
    listen for `ClickEvent` (not `MouseUpEvent`) when they want
    click-detection.

## `KeyEventContext`

```csharp
public readonly record struct KeyEventContext(
    TweenManager Tweener,
    KeyEventType Type = KeyEventType.None,
    int? KeyCode = null,
    char Character = '\0',
    bool IsControlPressed = false,
    bool IsShiftPressed = false,
    bool IsAltPressed = false
);
```

| Field | Meaning |
| --- | --- |
| `Tweener` | Current animation engine. |
| `Type` | `KeyDown`, `KeyUp`, or `CharInput`. |
| `KeyCode` | Virtual key code. `null` for `CharInput`. |
| `Character` | Printable character for `CharInput`. |

!!! warning "`KeyCode` is nullable"
    `KeyCode` is `int?` so `CharInput` events can skip it. When working with
    `KeyEventType.KeyDown` / `KeyUp`, cast or pattern-match before comparing:
    `if (ctx.KeyCode == 8) { ... }` still works because `int?` compares to
    `int` implicitly.