# `HandyWindow`

`HandyUI.SilkNet.Components.HandyWindow`

A Silk.NET/GLFW-backed window that runs its own pump thread, marshals calls
with `Invoke`, integrates with [`ThemeHelper`](theme-helper.md) for OS
appearance, and applies Win32 tweaks when available.

```csharp
public class HandyWindow : IDisposable
```

!!! warning "Windows only"
    The Silk.NET adapter currently supports Windows only. Construction throws
    `PlatformNotSupportedException` elsewhere.

## Construction

```csharp
public HandyWindow(WindowConfiguration windowConfiguration);
```

Creates the underlying window on a dedicated thread and returns once the
window handle exists (not necessarily initialized).

The constructor also:

- calls `ThemeHelper.EnsureInitialized()`, which reads the OS accent +
  light/dark mode and pushes them into [`ThemeManager`](theme-manager.md);
- registers the window with `ThemeHelper` for automatic re-invalidation on
  OS theme changes.

```csharp
var window = new HandyWindow(WindowConfiguration.Default with { Title = "Demo" });
```

## Typical usage

Attach a renderer, add controls to it, and run:

```csharp
var (window, renderer) =
    WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

renderer.AddRootControl(
    new Button().WithText("Save").WithLocation(20, 20));

window.Run();
```

!!! info "Add controls to the renderer, not `OnLoad`"
    `UIRenderer.AddRootControl` is thread-safe and queues the control for
    the next tick. There is no reason to wait for the GL context - only
    operations that genuinely need a live context (like the window icon)
    are deferred automatically.

## Properties

| Property | Type | Description |
| --- | --- | --- |
| `IsDisposed` | `bool` | True after `Dispose`. |
| `IsInitialized` | `bool` | True after the GL context exists. |
| `InternalWindow` | `IWindow?` | The underlying Silk.NET window. |
| `Title` | `string` | Window title. |
| `Border` | `WindowBorder` | Resizable / fixed / hidden. |
| `State` | `WindowState` | Normal / maximized / minimized. |
| `VSync` | `bool` | Toggle vertical sync. |
| `UseDirtyRendering` | `bool` | Mirrors the renderer setting. |
| `Size` | `SKSize?` | Client area size. |
| `MinSize` / `MaxSize` | `SKSize?` | Size constraints. |
| `Position` | `SKPoint?` | Top-left position. |
| `Opacity` | `float` | Window opacity (GLFW). |
| `IsMovable` | `bool` | Whether the user can drag the window. |
| `IsResizable` | `bool` | Whether the user can resize it. |
| `VisibleInTaskbar` | `bool` | Win32 taskbar visibility. |
| `IsMinimizable` | `bool` | Minimize button. |
| `IsMaximizable` | `bool` | Maximize button. |
| `IsCloseable` | `bool` | If `false`, close requests are canceled. |
| `TopMost` | `bool` | Always on top. |
| `Icon` | `SKImage?` | Window icon, applied automatically after init. |
| `SubwindowParent` | `HandyWindow?` | Native parent window. |
| `ModalParent` | `HandyWindow?` | Modal parent; disables the parent. |
| `ModalChild` | `HandyWindow?` | Read-only modal child. |
| `InvalidateParentOnMove` | `bool` | Invalidate parent on move. |

### `Icon`

```csharp
public SKImage? Icon { get; set; }
```

Assigning a new icon marks it pending; the actual `SetWindowIcon` call is
deferred until the window is initialized, marshaled onto the pump thread,
and converted from RGBA to BGRA for Win32.

```csharp
window.Icon = SKImage.FromEncodedData("Assets/app-icon.png");
```

## Events

| Event | Signature | Raised when |
| --- | --- | --- |
| `OnLoad` | `Action` | GL context initialized. |
| `OnClosing` | `Action` | Window is about to close. |
| `OnMove` | `Action<SKPoint>` | Window moved. |
| `OnResize` | `Action<SKSize>` | Window resized. |
| `OnStateChanged` | `Action<WindowState>` | Minimize/maximize/normal. |
| `OnFocusChanged` | `Action<bool>` | Focus gained/lost. |

## Methods

### `Initialize()`

Blocks until the GL context is ready.

### `Run()` / `RunAsync(CancellationToken)`

Runs the window until it closes. `RunAsync` returns a `Task` completed on
close; cancellation triggers `Close()`.

### `Close()`

Requests the pump loop to exit.

### `Invalidate()`

Marks the renderer dirty and wakes the pump thread.

### `SetSizeLimits(SKSize?, SKSize?)`

Convenience wrapper around `MinSize` / `MaxSize`.

### `WaitForInitialization(int = Timeout.Infinite)`

Blocks until the GL context is ready (or times out).

### `Invoke(Action)` / `Invoke<T>(Func<T>)`

Marshals a delegate onto the pump thread. Nested invokes from another thread
are fine; nested invokes from the pump thread must not block.

## Dispose

```csharp
public void Dispose();
```

Unregisters from `ThemeHelper`, closes the window, waits up to two seconds
for the pump thread, then marks the instance disposed.