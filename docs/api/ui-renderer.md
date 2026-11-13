# `UIRenderer`

`HandyUI.Core.Components.UIRenderer`

The render loop, input dispatcher, focus manager, and module host. Targets
[`IHostWindow`](ihostwindow.md) instead of a concrete window.

```csharp
public class UIRenderer : IDisposable
```

## Constructor

```csharp
public UIRenderer(IHostWindow host);
```

Creates the shared [`TweenManager`](tween-manager.md), assigns it to
`host.TweenManager`, subscribes to `ThemeManager.ThemeChanged`, starts the
internal `TotalTime` stopwatch, and detects debug builds to enable the
profiler by default.

## Properties

### `BackgroundColor`

```csharp
public SKColor BackgroundColor { get; set; }
```

Defaults to `ThemeManager.Instance.BackgroundPrimary`. Assigning a color
different from the theme marks the value as *custom* - subsequent theme
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

Toggles the built-in debug overlay. Defaults to `true` in Debug builds and
`false` in Release. Can be toggled at runtime with **`Ctrl+Shift+G`**.

### `TotalTime`

```csharp
public double TotalTime { get; }
```

Total elapsed seconds since the `UIRenderer` was constructed. Backed by a
`Stopwatch` that starts immediately and is **not** reset - it advances even
while dirty rendering skips paint frames. This makes it suitable for
long-running animations or session timers.

!!! info "Wall-clock time, not tick time"
    Earlier versions accumulated `TotalTime` only inside `UpdateModules`.
    The current implementation uses a monotonic `Stopwatch`, so
    `TotalTime` keeps ticking during idle frames of the dirty renderer.

    ### `KeyboardFocus`

```csharp
public bool KeyboardFocus { get; set; } = false;
```

Whether the renderer treats Tab / Shift+Tab as focus-cycling shortcuts.
When `false` (the default), Tab is delivered to the focused control (or
modules) just like any other key. Set `KeyboardFocus = true` to enable
keyboard navigation:

```csharp
renderer.KeyboardFocus = true;
```

!!! tip "Why it's opt-in"
    Many apps want Tab to mean "insert a tab character" (e.g. code editors
    with a focused `MultilineTextInput`). Leaving `KeyboardFocus = false`
    by default lets apps keep the standard tab-in-text behavior. Toggle it
    when the user enters a form or navigation context.

## Events

```csharp
public event Action? WhenInvalidated;
```

Raised on every `Invalidate()`. Any adapters using HandyUI should subscribe to this event so that invalidation actually works in dirty rendering mode.

## Methods

### `Invalidate()`

```csharp
public void Invalidate();
```

Marks the renderer dirty for two frames and raises `WhenInvalidated`. Also
calls `_hostWindow.Invalidate()`.

### `AddControl(IControl)` / `RemoveControl(IControl)`

```csharp
public void AddControl(IControl control);
public void RemoveControl(IControl control);
```

Queue a control for attach/detach. Attachment wires `WhenInvalidated` and
`WhenFocusRequested`. Removal clears focus / press state and disposes the
control.

### `RenderControls(SKCanvas, SKPoint, float)`

```csharp
public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime);
```

Processes pending controls and draws the tree. Returns `false` if the
dirty renderer skipped the pass (`host.UseDirtyRendering && frames ≤ 0`).

### `UpdateModules(float)`

```csharp
public bool UpdateModules(float deltaTime);
```

Ticks every enabled module with a fresh `UpdateContext`. Modules always run
regardless of dirty-rendering state - animations and logic tick every frame.

### `GetControlAt(SKPoint)`

```csharp
public IControl? GetControlAt(SKPoint clientPosition);
```

Hit-tests without dispatching. Respects `Visible`, `Enabled`, and
`ClipChildren`.

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

Cycles focus through `Focusable` controls in `ZIndex` order. Called
automatically by `ProcessKeyEvent` on **Tab** (Shift+Tab = previous).

### `ProcessMouseEvent(MouseEventContext)`

```csharp
public void ProcessMouseEvent(MouseEventContext context);
```

1. Returns early if `context.MousePosition == null`.
2. Processes pending controls.
3. Runs the module input pipeline. Modules **observe** each event but cannot
   consume it - the pipeline always continues to the controls below.
4. Hit-tests controls (highest `ZIndex` first).
5. On `MouseDownEvent`: tracks `_pressedControl` and sets or clears focus.
6. Dispatches recursively into the tree.
7. On `MouseUpEvent`: synthesizes a `ClickEvent` to `_pressedControl` if it's
   still under the cursor.

### `ProcessKeyEvent(KeyEventContext)`

```csharp
public bool ProcessKeyEvent(KeyEventContext context);
```

1. **Ctrl+Shift+G** toggles the profiler (Debug builds).
2. Runs the module input pipeline (observe-only).
3. Forwards to the focused control.
4. If `KeyboardFocus` is `true` and the key is **Tab**:
   - **Shift+Tab** → `FocusPrevious()`
   - **Tab** → `FocusNext()`
5. Returns `true` if any step handled the event; otherwise `false`.

!!! warning "Modules cannot consume input"
    Unlike controls, modules never block propagation. Their
    `ProcessMouseEvent` / `ProcessKeyEvent` return `void`, so every event
    reaches both modules and controls. If you need a component that can
    swallow input, use an `IControl` with `Focusable = true` (for keys) or a
    control whose `OnMouse` returns `true`.

## Focus behavior

### Focus rectangles

When focus was set by keyboard (`FocusNext` / Tab), the renderer draws a
focus rectangle on the focused control after its subtree. Clicking a control
switches to non-keyboard focus and the rectangle disappears.

### Synthetic events on focus change

When the focused control changes, `SetFocus` dispatches two synthetic
`MoveEvent`s to keep `IsHovered` responsive:

| Target | `MousePosition` | Effect |
| --- | --- | --- |
| Old focused control | `null` | `IsHovered` → `false` (blur) |
| New focused control | `SKPoint.Empty` | `IsHovered` → `Bounds.Contains(0, 0)` |

The events are typed `MoveEvent` purely as a state-ping - no press or click
logic fires. Controls can override `OnMouse` to react to focus state changes
if they need to.

!!! tip "Why this exists"
    Without it, a control losing focus could keep rendering its hover
    highlight if the cursor happened to still be over it. Sending a `null`
    move clears that state deterministically.

## Behavior

### Dirty rendering

Two-frame invalidation. `RenderControls` early-outs when
`host.UseDirtyRendering` is `true` and the frame counter is `≤ 0`.

### Module pipeline

Modules **do not** get input by default. To participate in input, they must
have `AcceptsInput = true` *(default)*

Modules also **cant** consume inputs.

### Profiler overlay

When `UseProfiler = true`, a [`ProfilerControl`](profiler-control.md) is
created on the first render and assigned `ZIndex = 10000` so it renders on
top of every other control.

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

## Dispose

```csharp
public void Dispose();
```

Disposes every control recursively, every module, drains the pending queues,
and unsubscribes from `ThemeManager.ThemeChanged`.