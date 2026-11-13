# `UIRenderer`

`HandyUI.Core.Components.UIRenderer`

The render loop, input dispatcher, focus manager, and module host. It
targets [`IHostWindow`](ihostwindow.md) rather than a concrete window - the
Silk.NET package wires it up via [`RendererHelper.Attach`](silknet-helpers.md#rendererhelper).

```csharp
public class UIRenderer : IDisposable
```

## Constructor

```csharp
public UIRenderer(IHostWindow host);
```

The constructor creates the shared [`TweenManager`](tween-manager.md), assigns
it to `host.TweenManager`, and subscribes to `ThemeManager.ThemeChanged` so
`BackgroundColor` follows the active theme.

## Properties

### `BackgroundColor`

```csharp
public SKColor BackgroundColor { get; set; }
```

Defaults to `ThemeManager.Instance.BackgroundPrimary`. Assigning a color that
differs from the theme marks the value as *custom* - subsequent theme
changes will no longer overwrite it.

### `TweenManager`

```csharp
public TweenManager TweenManager { get; }
```

The shared animation engine. Registered as a module automatically.

### `FocusedControl`

```csharp
public IControl? FocusedControl { get; }
```

The currently focused control, or `null`.

### `UseProfiler`

```csharp
public bool UseProfiler { get; set; }
```

Enables the built-in debug overlay (see [`ProfilerControl`](profiler-control.md)).
Defaults to `true` in Debug builds, `false` in Release. Can be toggled at
runtime with `Ctrl+Shift+G`.

### `TotalTime`

```csharp
public double TotalTime { get; }
```

Total accumulated seconds of `UpdateModules` time.

## Events

```csharp
public event Action? WhenInvalidated;
```

Raised on every `Invalidate()` call. The Silk.NET adapter subscribes to
this to wake the pump thread.

## Methods

### `Invalidate()`

```csharp
public void Invalidate();
```

Marks the renderer dirty for two frames and raises `WhenInvalidated`.
Also calls `_hostWindow.Invalidate()`.

### `AddControl(IControl)` / `RemoveControl(IControl)`

```csharp
public void AddControl(IControl control);
public void RemoveControl(IControl control);
```

Queue a control for attach/detach. Attachment wires `WhenInvalidated` and
`WhenFocusRequested`. Removal disposes the control and clears focus / press
state.

### `RenderControls(SKCanvas, SKPoint, float)`

```csharp
public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime);
```

Processes pending controls and draws the tree. Returns `false` if the dirty
renderer skipped the pass (based on `host.UseDirtyRendering`).

### `UpdateModules(float)`

```csharp
public bool UpdateModules(float deltaTime);
```

Ticks every enabled module with a fresh `UpdateContext`. Runs regardless
of dirty-rendering state (the dirty-skip line is commented out) so
animations and logic always tick.

### `GetControlAt(SKPoint)`

```csharp
public IControl? GetControlAt(SKPoint clientPosition);
```

Hit-tests the tree without dispatching an event. Respects `Visible`,
`Enabled`, and `ClipChildren`.

### `IsPointOverAnyControl(SKPoint)`

```csharp
public bool IsPointOverAnyControl(SKPoint clientPosition);
```

Convenience wrapper around `GetControlAt(...) != null`.

### `FocusNext()` / `FocusPrevious()`

```csharp
public void FocusNext();
public void FocusPrevious();
```

Cycles through `Focusable` controls in `ZIndex` order. Called automatically
by `ProcessKeyEvent` on **Tab** (Shift+Tab = previous).

### `ProcessMouseEvent(MouseEventContext)`

```csharp
public void ProcessMouseEvent(MouseEventContext context);
```

Order of processing:

1. Returns early if `context.MousePosition == null`.
2. Calls `ProcessPendingControls()`.
3. Runs module input pipeline (stops on first module that returns `true`).
4. Hit-tests controls (highest `ZIndex` first).
5. On `MouseDownEvent`: sets `_pressedControl`, sets focus (or clears it).
6. Dispatches recursively into the tree.
7. On `MouseUpEvent`: if the pressed control is still under the cursor,
   synthesizes a `ClickEvent` with the local position.

### `ProcessKeyEvent(KeyEventContext)`

```csharp
public bool ProcessKeyEvent(KeyEventContext context);
```

Order of processing:

1. If `UseProfiler && Ctrl+Shift+G` toggles the profiler.
2. Runs module input pipeline.
3. If a control is focused and it handles the key, returns `true`.
4. **Tab** → `FocusNext()` / `FocusPrevious()`.
5. Returns `false`.

### `Dispose()`

```csharp
public void Dispose();
```

Disposes every control recursively, every module, drains pending queues, and
unsubscribes from theme changes.

## Behavior

### Focus rectangles

When a control is focused **via keyboard** (`FocusNext`/Tab), `UIRenderer`
calls `DrawFocusRect()` on it after drawing its subtree. Clicking a control
switches to non-keyboard focus and the rectangle disappears.

### Dirty rendering

Two-frame invalidation. `RenderControls` early-outs when `host.UseDirtyRendering`
is `true` and the frame counter is `≤ 0`.

### Module pipeline

Modules **do not** get automatic focus. To participate in input, they must
`AcceptsInput = true` (default) and return `true` from
`ProcessMouseEvent`/`ProcessKeyEvent` to consume the event.

## Example

```csharp
var (window, renderer) =
    WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

renderer.AddControl(
    new Button()
        .WithText("Save")
        .WithWhenClicked(() => Save()));

window.Run();
```