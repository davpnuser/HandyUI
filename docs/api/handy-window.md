# `HandyWindow`

`HandyUI.SilkNet.Components.HandyWindow`

A Silk.NET/GLFW-backed window with its own pump thread, Win32 integrations,
and a "hide-the-title-bar" mode for custom chrome. Integrates with
[`ThemeHelper`](theme-helper.md) for OS appearance and exposes a
[`TrayIcon`](tray-icon.md) via a lazily-created `TrayIcon` property.

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

The constructor also calls `ThemeHelper.EnsureInitialized()` and registers
the window for automatic re-invalidation when the OS theme changes.

```csharp
var window = new HandyWindow(WindowConfiguration.Default with { Title = "Demo" });
```

## Typical usage

```csharp
var (window, renderer) =
    WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

renderer.AddRootControl(
    new Button().WithText("Save").WithLocation(20, 20));

window.Run();
```

!!! info "Add controls to the renderer, not `WhenLoaded`"
    `UIRenderer.AddRootControl` is thread-safe and queues the control for
    the next tick. Only operations that genuinely need a live GL context
    (icon, tray icon, title bar tweaks) are deferred automatically.

## Properties

| Property | Type | Description |
| --- | --- | --- |
| `IsDisposed` | `bool` | True after `Dispose`. |
| `IsInitialized` | `bool` | True after the GL context exists. |
| `InternalWindow` | `IWindow?` | The underlying Silk.NET window. |
| `Title` | `string` | Window title. |
| `Border` | `WindowBorder` | Resizable / fixed / hidden. |
| `IsTitleBarVisible` | `bool` | Whether the OS title bar is shown (default: `true`). |
| `State` | `WindowState` | Normal / maximized / minimized. |
| `VSync` | `bool` | Toggle vertical sync. |
| `UseDirtyRendering` | `bool` | Mirrors the renderer setting. |
| `Size` | `SKSize?` | Client area size. |
| `MinSize` / `MaxSize` | `SKSize?` | Size constraints. |
| `Position` | `SKPoint?` | Top-left window position. |
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
| `IsDragging` | `bool` | True while a custom drag (via `BeginDrag`/pump) is active. |
| `IsHidden` | `bool` | True while `Hide()` has removed the window from view. |
| `TrayIcon` | `TrayIcon` | Lazily-created tray icon for this window. |

### `IsTitleBarVisible`

```csharp
public bool IsTitleBarVisible { get; set; }   // default: true
```

When `false` and `Border` is not `WindowBorder.Hidden`, the window is
subclassed to strip the OS title bar via `WM_NCCALCSIZE` and reimplement
resize hit-testing with `WM_NCHITTEST`. Combine with `Border = Resizable`
to keep resize edges while drawing your own chrome.

```csharp
var window = new HandyWindow(WindowConfiguration.Default with
{
    Title = "Custom Chrome",
    IsTitleBarVisible = false
});

// Your controls can now draw the title bar area themselves.
```

!!! tip "Drag with the Win32 pump loop"
    When the title bar is hidden, call `window.BeginDrag()` from your
    custom title bar's `MouseDown` to hand control to the OS drag loop:

    ```csharp
    titleBar.WhenMouseDown += window.BeginDrag;
    ```

### `IsDragging` / `BeginDrag()`

```csharp
public bool IsDragging { get; set; }

public void BeginDrag();
```

`BeginDrag()` calls `Win32Helper.StartWindowDrag(hwnd)` on the pump thread.
Setting `IsDragging = true` starts the internal drag tracker (cursor
capture on the pump loop); setting it to `false` restores the previous
position if `IsMovable` is off.

### `IsHidden` / `Hide()` / `Show()`

```csharp
public bool IsHidden { get; }

public void Hide();
public void Show();
```

`Hide()` sets `VisibleInTaskbar = false`, hides the window, and skips
rendering until `Show()` restores it. The previous taskbar-visibility state
is remembered and restored by `Show()`.

!!! info "Why not `IsVisible`?"
    `Hide()` also removes the window from the taskbar and pauses the pump
    loop's render path. Use it for "close to tray" behavior:

    ```csharp
    window.WhenClosing += () =>
    {
        // Cancel the close and hide instead.
        WindowHelper.TryCancelClose(window.InternalWindow!);
        window.Hide();
    };
    ```

### `TrayIcon`

```csharp
public TrayIcon TrayIcon { get; }
```

A lazily-created [`TrayIcon`](tray-icon.md) bound to this window. It is
disposed automatically when the window closes.

```csharp
window.TrayIcon.Icon = SKImage.FromEncodedData("Assets/tray.png");
window.TrayIcon.ToolTip = "My App";
window.TrayIcon.MenuItems.Add(
    new TrayMenuItem("Show", () => window.Show()));
window.TrayIcon.MenuItems.Add(TrayMenuItem.Separator());
window.TrayIcon.MenuItems.Add(
    new TrayMenuItem("Quit", () => window.Close()));
window.TrayIcon.Show();
```

## Events

| Event | Signature | Raised when |
| --- | --- | --- |
| `WhenLoaded` | `Action` | GL context initialized. |
| `WhenClosing` | `Action` | Window is about to close. |
| `WhenMoved` | `Action<SKPoint>` | Window moved. |
| `WhenResized` | `Action<SKSize>` | Window resized. |
| `WhenStateChanged` | `Action<WindowState>` | Minimize/maximize/normal. |
| `WhenFocusChanged` | `Action<bool>` | Focus gained/lost. |

```csharp
window.WhenLoaded += () => Console.WriteLine("Ready");
window.WhenClosing += () => SaveLayout();
window.WhenResized += size => Layout.Recalculate(size);
```

## Methods

### Lifecycle

```csharp
public void Initialize();     // blocks until GL context is ready
public void Run();            // Initialize + block until close
public Task RunAsync(CancellationToken = default);
public void Close();
public void WaitForInitialization(int timeout = Timeout.Infinite);
```

### Windowing

```csharp
public void Invalidate();
public void SetSizeLimits(SKSize? min, SKSize? max);

public void Hide();
public void Show();

public void BeginDrag();
```

### Marshalling

```csharp
internal void Invoke(Action action);
internal T   Invoke<T>(Func<T> func);
```

Runs a delegate on the pump thread. Nested invokes from another thread are
fine; nested invokes from the pump thread must not block.

## Dispose

```csharp
public void Dispose();
```

Unregisters from `ThemeHelper`, disposes the tray icon, closes the window,
and waits up to two seconds for the pump thread.