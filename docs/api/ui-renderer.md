# `UIRenderer`

`HandyUI.Core.Components.UIRenderer`

The heart of HandyUI. Owns root controls and modules, dispatches mouse and
keyboard input into the control tree, and runs the render pass. Also hosts
the shared [`Tweener`](tweener.md) used by built-in controls for animations.

```csharp
public class UIRenderer : IDisposable
```

!!! tip "Thread safety"
    `AddRootControl`, `RemoveRootControl`, `AddModule`, `RemoveModule`, and
    `Invalidate` are all thread-safe.

## Constructor

```csharp
public UIRenderer(bool useDirtyRendering = true)
```

| Parameter | Default | Description |
| --- | --- | --- |
| `useDirtyRendering` | `true` | When `true`, `RenderControls` returns early until invalidated. Set to `false` for continuous repaint. |

The constructor also:

- subscribes to `ThemeManager.Instance.ThemeChanged` so `BackgroundColor`
  tracks the theme automatically (unless you've set a custom color);
- creates a `Tweener`, registers it as a module, and starts it so animations
  work out of the box.

## Properties

### `BackgroundColor`

```csharp
public SKColor BackgroundColor { get; set; }
```

The canvas clear color. Defaults to `ThemeManager.Instance.BackgroundPrimary`
and follows theme changes automatically until you assign a different color.

```csharp
renderer.BackgroundColor = SKColors.Black;   // stops tracking the theme
```

### `Tweener`

```csharp
public Tweener Tweener { get; }
```

The animation engine shared with all controls. Built-in controls pull it via
`Tweener.Current` while they're being updated; you can also start your own
tweens directly:

```csharp
renderer.Tweener.To(0f, 1f, 0.3f, v => myOpacity = v, EaseType.SineOut);
```

## Events

### `WhenInvalidated`

```csharp
public event Action? WhenInvalidated;
```

Fires whenever `Invalidate()` is called. `RendererAdapter` uses this to wake
the pump thread when running in dirty-rendering mode.

## Methods

### `Invalidate()`

```csharp
public void Invalidate();
```

Marks the renderer dirty for two frames and raises `WhenInvalidated`. Call
after mutating control state outside the built-in property setters (which
already invalidate).

### `AddRootControl(IControl)` / `RemoveRootControl(IControl)`

```csharp
public void AddRootControl(IControl control);
public void RemoveRootControl(IControl control);
```

Queue a control for addition/removal. Attachment happens on the next tick.
`RemoveRootControl` disposes the control after detaching it.

!!! warning "Ownership"
    `RemoveRootControl` disposes the control. To hide a control temporarily,
    set `IsVisible = false`.

### `AddModule(IModule)` / `RemoveModule(IModule)`

```csharp
public void AddModule(IModule module);
public void RemoveModule(IModule module);
```

Queue a module for addition/removal. Removal disposes the module.

### `ProcessMouseEvent(MouseEventContext)`

```csharp
public void ProcessMouseEvent(MouseEventContext context);
```

1. Hit-tests root controls (highest `ZIndex` first).
2. On `MouseDown`, tracks the pressed control and sets focus.
3. Dispatches the raw event recursively.
4. On `MouseUp`, synthesizes a `Click` to `_pressedControl` if it's still
   under the cursor.

Also sets up the shared `Tweener` so controls can animate in `OnMouse`.

### `ProcessKeyEvent(KeyEventContext)`

```csharp
public bool ProcessKeyEvent(KeyEventContext context);
```

Forwards to the focused control. Returns the control's return value, or
`false` if no control is focused / it's invisible / disabled.

### `RenderControls(SKCanvas, SKPoint, float)`

```csharp
public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime);
```

Processes pending control changes, then draws the tree. Returns `true` if a
render actually happened.

### `UpdateModules(float)`

```csharp
public bool UpdateModules(float deltaTime);
```

Ticks every enabled module (`module.OnUpdate(deltaTime)`) while `Tweener` is
set as current so animations resolve.

### `IsPointOverAnyControl(SKPoint)`

```csharp
public bool IsPointOverAnyControl(SKPoint clientPosition);
```

Runs the hit-test without dispatching an event. Useful for hover detection
from the host window (e.g. to change cursor shape).

## Example

```csharp
var renderer = new UIRenderer(useDirtyRendering: false);

renderer.AddRootControl(new Button().WithText("OK").WithAddToRenderer(renderer));

while (running)
{
    renderer.UpdateModules(deltaTime);
    renderer.RenderControls(canvas, mousePos, deltaTime);
}
```

## Dispose

```csharp
public void Dispose();
```

Disposes every root control recursively, every module, drains pending
queues, and unsubscribes from `ThemeManager.ThemeChanged`. Safe to call
multiple times.