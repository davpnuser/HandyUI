# `UIRenderer`

`HandyUI.Core.Components.UIRenderer`

The heart of HandyUI. It owns root controls and modules, dispatches mouse and
keyboard events into the control tree, and runs the render pass.

```csharp
public class UIRenderer : IDisposable
```

!!! tip "Thread safety"
    `UIRenderer` uses internal locks and pending-queue lists. `AddRootControl`,
    `RemoveRootControl`, `AddModule`, `RemoveModule`, and `Invalidate` are all
    safe to call from any thread.

## Constructor

```csharp
public UIRenderer(bool useDirtyRendering = true)
```

| Parameter | Default | Description |
| --- | --- | --- |
| `useDirtyRendering` | `true` | When `true`, `RenderControls` returns early until the renderer is invalidated. Set to `false` for continuous repaint. |

!!! info "Two-frame invalidation"
    `Invalidate()` queues **two** repaint frames internally. Many layout
    systems need one frame to apply a change and a second to render it.

## Properties

### `BackgroundColor`

```csharp
public SKColor BackgroundColor { get; set; }
```

The canvas clear color for each render pass. Defaults to
`Theme.Default.DarkerBackgroundColor`.

```csharp
renderer.BackgroundColor = SKColors.Black;
```

## Methods

### `Invalidate()`

```csharp
public void Invalidate()
```

Marks the renderer dirty for two frames. Call after mutating control state
outside of the built-in property setters (which already invalidate).

### `AddRootControl(IControl control)`

```csharp
public void AddRootControl(IControl control)
```

Queues `control` for addition as a root control. Attachment happens on the
next render/update tick.

!!! note
    The renderer subscribes to the control's `WhenFocusRequested` and
    `WhenInvalidated` events on attach. Do not attach the same instance to
    two renderers.

### `RemoveRootControl(IControl control)`

```csharp
public void RemoveRootControl(IControl control)
```

Queues `control` for removal. On the next tick the renderer unsubscribes
from events, clears focus/press tracking, and calls `Dispose()` on the
control.

!!! warning "Ownership"
    `RemoveRootControl` **disposes** the control. If you want to hide it
    temporarily, set `IsVisible = false` instead.

### `AddModule(IModule)` / `RemoveModule(IModule)`

```csharp
public void AddModule(IModule module)
public void RemoveModule(IModule module)
```

Queue a module for addition/removal. On removal, `Dispose()` is called if
the module implements `IDisposable`.

### `ProcessMouseEvent(MouseEventContext context)`

```csharp
public void ProcessMouseEvent(MouseEventContext context)
```

Runs a full mouse pass:

1. Hit-tests root controls (highest `ZIndex` first).
2. Updates `_pressedControl` on `MouseDown` and sets focus on the hit control.
3. Dispatches the raw event recursively.
4. On `MouseUp`, synthesizes a `Click` to `_pressedControl` if it's still
   under the cursor.

!!! info "Nullable `ClientPosition`"
    `MouseEventContext.ClientPosition` is `SKPoint?`. A `null` value means
    *"the cursor is not over this subtree"*. `ProcessMouseEvent` returns
    early if the incoming event has a null position; the recursive dispatch
    substitutes `null` for clipped descendants.

### `ProcessKeyEvent(KeyEventContext context)`

```csharp
public bool ProcessKeyEvent(KeyEventContext context)
```

Forwards the event to the currently focused control. Returns the control's
return value, or `false` when no control is focused / the control is
invisible / disabled.

### `RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime)`

```csharp
public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime)
```

Processes pending control changes, then draws the whole tree into `canvas`.
Returns `true` if a render actually happened.

### `UpdateModules(float deltaTime)`

```csharp
public bool UpdateModules(float deltaTime)
```

Ticks every enabled module by calling `module.OnUpdate(deltaTime)`. Returns
`true` if modules were actually updated.

## Example

```csharp
var renderer = new UIRenderer(useDirtyRendering: false);
renderer.AddRootControl(myButton);
renderer.AddModule(mySpinner);

while (running)
{
    renderer.UpdateModules(deltaTime);
    renderer.RenderControls(canvas, mousePos, deltaTime);
}
```

## Dispose

```csharp
public void Dispose()
```

Disposes every root control (recursively), every module, and drains the
pending queues. Safe to call multiple times.

!!! warning
    After `Dispose`, the renderer cannot be reused. Recreate it instead.