# `HandyWindow`

`HandyUI.SilkNet.Components.HandyWindow`

A window wrapper around Silk.NET's `IWindow` that runs a dedicated message
pump thread, marshals calls with `Invoke`, and applies Win32 tweaks.

```csharp
public class HandyWindow : IDisposable
```

!!! warning "Windows only"
    The Silk.NET adapter currently supports Windows only. Construction throws
    `PlatformNotSupportedException` elsewhere.

## Construction

```csharp
public HandyWindow(WindowConfiguration windowConfiguration)
```

Creates the underlying window on a dedicated thread and returns once the
window handle has been created (not necessarily initialized).

```csharp
var window = new HandyWindow(WindowConfiguration.Default with { Title = "Demo" });
```

## Typical usage

Add controls **directly to the renderer**, not inside `OnLoad`:

```csharp
var window   = new HandyWindow(WindowConfiguration.Default with { Title = "Hi" });
var renderer = RendererHelper.Attach(window);

renderer.AddRootControl(
    new MyBox()
        .WithLocation(40, 40)
        .WithBounds(0, 0, 200, 120));

window.Run();
```

!!! info "Why not `OnLoad`?"
    `UIRenderer.AddRootControl` is thread-safe and queues the control for the
    next tick. There's no need to wait for the GL context to attach a
    control - only things that require a live GL context (like setting the
    window `Icon`) need to run after initialization, and those are handled
    for you through deferred properties.

## Properties

| Property | Type | Description |
| --- | --- | --- |
| `IsDisposed` | `bool` | True after `Dispose`. |
| `IsInitialized` | `bool` | True after the GL context has been created. |
| `InternalWindow` | `IWindow?` | The underlying Silk.NET window. |
| `Title` | `string` | Window title. |
| `Border` | `WindowBorder` | Resizable / fixed / hidden. |
| `State` | `WindowState` | Normal / maximized / minimized. |
| `VSync` | `bool` | Toggle vertical sync. |
| `UseDirtyRendering` | `bool` | Mirrors the `UIRenderer` setting. |
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

Assigning a new icon marks it pending; the actual Win32 `SetWindowIcon` call
is deferred until the window is initialized and is replayed on the pump
thread.

```csharp
window.Icon = SKImage.FromEncodedData("Assets/app-icon.png");
```

!!! info "No `SetIcon` method"
    The old `SetIcon(SKImage)` method has been **removed**. Assign the `Icon`
    property instead - it handles initialization timing and thread
    marshalling for you.

## Events

| Event | Signature | Raised when |
| --- | --- | --- |
| `OnLoad` | `Action` | Window GL context is initialized. |
| `OnClosing` | `Action` | Window is about to close. |
| `OnMove` | `Action<SKPoint>` | Window moved. |
| `OnResize` | `Action<SKSize>` | Window resized. |
| `OnStateChanged` | `Action<WindowState>` | Minimize/maximize/normal. |
| `OnFocusChanged` | `Action<bool>` | Focus gained/lost. |

## Methods

### `Initialize()`

```csharp
public void Initialize()
```

Requests initialization and blocks until the GL context is ready.

### `Run()`

```csharp
public void Run()
```

Calls `Initialize()` then blocks until the window closes.

### `RunAsync(CancellationToken)`

```csharp
public Task RunAsync(CancellationToken cancellationToken = default)
```

Async flavor of `Run`. Cancellation triggers `Close()`.

### `Close()`

```csharp
public void Close()
```

Requests the pump loop to exit and the window to close.

### `Invalidate()`

```csharp
public void Invalidate()
```

Marks the renderer dirty and wakes the pump thread.

### `Invoke(Action)` / `Invoke<T>(Func<T>)`

```csharp
internal void Invoke(Action action);
internal T   Invoke<T>(Func<T> func);
```

Marshals a delegate onto the pump thread and blocks until it completes.

!!! tip "Nested invokes"
    Nested invokes from a **different** thread are fine, but a nested invoke
    issued from the pump thread must not block.

### `SetSizeLimits(SKSize?, SKSize?)`

```csharp
public void SetSizeLimits(SKSize? minSize, SKSize? maxSize)
```

Convenience wrapper around `MinSize` / `MaxSize`.

### `WaitForInitialization(int timeoutMilliseconds = Timeout.Infinite)`

```csharp
public void WaitForInitialization(int timeoutMilliseconds = Timeout.Infinite)
```

Blocks until the GL context is ready (or times out).

## Dispose

```csharp
public void Dispose();
```

Closes the window, then marks the
instance disposed.