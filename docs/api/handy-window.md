# `HandyWindow`

`HandyUI.SilkNet.Components.HandyWindow`

Silk.NET / GLFW implementation of [`IHostWindow`](ihostwindow.md). Owns the
pump thread, GL context bridge, Win32 integrations, custom title-bar
handling, drag logic, DPI, and tray icon.

```csharp
public class HandyWindow : IDisposable, IHostWindow
```

!!! warning "Windows only"
    Construction throws `PlatformNotSupportedException` on non-Windows
    platforms.

## Construction

```csharp
public HandyWindow(WindowConfiguration windowConfiguration);
```

The constructor calls `ThemeHelper.EnsureInitialized()` and spins up the
pump thread.

```csharp
var window = new HandyWindow(WindowConfiguration.Default with { Title = "Chat" });
var renderer = RendererHelper.Attach(window);
window.Run();
```

## `IHostWindow` implementation

All `IHostWindow` members are implemented, with these notable additions:

| Member | Notes |
| --- | --- |
| `Title` | Mirrors `InternalWindow.Title`. |
| `Icon` | Deferred apply - set then pumped onto the pump thread after init. |
| `Size` | Returns `SKSize.Empty` until initialized. |
| `UseDirtyRendering` | Mirrors `InternalWindow.IsEventDriven`. |
| `VSync` | Pass-through. |
| `FramesPerSecond` | Stored property; used when VSync is off. |
| `Cursor` | Deferred apply on the pump thread via `RendererAdapter.SetCursor`. |
| `IsForeground` | True while window has focus. |
| `DeviceType` | Always `Desktop`. |
| `Orientation` | Landscape / portrait; updated on resize. |
| `DpiScale` | `GetDeviceCaps(LOGPIXELSX) / 96`. |
| `DisplayDist` | Physical diagonal in inches (via `HORZSIZE` / `VERTSIZE`). `0` if unknown. |
| `TweenManager` | Assigned by `UIRenderer`. |
| `Renderer` | The attached `UIRenderer`, or `null`. |
| `TotalTime` | Mirrors `Renderer.TotalTime`. |
| `WhenOrientationChanged` | Raised on landscape ↔ portrait flips. |
| `Invalidate()` | Wakes the pump thread and raises `WhenInvalidated`. |

## Additional properties

### `InternalWindow`

```csharp
public IWindow? InternalWindow { get; }
```

The underlying Silk.NET window.

### `Border` / `IsTitleBarVisible` / `IsResizable` / `IsMinimizable` / `IsMaximizable`

Configure the OS chrome. When `IsTitleBarVisible` is `false`, `HandyWindow`
subclasses the WndProc to strip `WM_NCCALCSIZE` and reimplement resize
hit-testing via `WM_NCHITTEST`.

### `DragRegions`

```csharp
public IList<SKRect> DragRegions { get; }
```

Screen-space rectangles (relative to the client area) that the WndProc
treats as `HTCAPTION` for OS drag. Use this for a custom title bar drawn
in your UI:

```csharp
window.DragRegions.Add(new SKRect(0, 0, window.Size.Width, 40));
```

The WndProc converts hit positions via `ScreenToClient` before checking
each region.

### `IsDragging`

```csharp
public bool IsDragging { get; set; }
```

Toggles the internal drag tracker (cursor capture + move). Handy for
manual drag loops; `BeginDrag()` is simpler for OS-driven drags.

### `IsHidden`

```csharp
public bool IsHidden { get; }
```

True between `Hide()` and `Show()`.

### `TrayIcon`

```csharp
public TrayIcon TrayIcon { get; }
```

Lazily-created tray icon, disposed with the window.

## Windowing methods

```csharp
public void Initialize();
public void Run();
public Task RunAsync(CancellationToken cancellationToken = default);
public void Close();

public void Hide();
public void Show();

public void BeginDrag();
public void SetSizeLimits(SKSize? min, SKSize? max);

public void WaitForInitialization(int timeoutMilliseconds = Timeout.Infinite);
```

## Drag with `DragRegions`

The idiomatic custom-title-bar pattern:

```csharp
window.IsTitleBarVisible = false;
window.Border = WindowBorder.Resizable;

// Tell HandyWindow that the top 40 px of the client area behaves like a title bar.
window.DragRegions.Add(new SKRect(0, 0, window.Size.Width, 40));

window.WhenResized += size =>
{
    window.DragRegions.Clear();
    window.DragRegions.Add(new SKRect(0, 0, size.Width, 40));
};
```

Now mouse-downs in the top 40 px are routed to the OS `HTCAPTION` behavior
automatically - the WndProc handles it via `WM_NCHITTEST`.

## Events

```csharp
public event Action? WhenLoaded;
public event Action? WhenClosing;
public event Action<SKPoint>? WhenMoved;
public event Action<SKSize>? WhenResized;
public event Action<WindowState>? WhenStateChanged;
public event Action<bool>? WhenFocusChanged;
public event Action<DisplayOrientation>? WhenOrientationChanged;
public event Action? WhenInvalidated;
```

## Dispose

```csharp
public void Dispose();
```

Unregisters from `ThemeHelper`, disposes the tray icon, closes the window,
and waits up to two seconds for the pump thread.