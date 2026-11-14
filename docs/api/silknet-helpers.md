# Silk.NET Helpers

## `RendererHelper`

`HandyUI.SilkNet.Helper.RendererHelper`

```csharp
public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true);
```

Wires up a `RendererAdapter` against `window.InternalWindow`, registers the
window with [`ThemeHelper`](theme-helper.md), and returns the freshly-created
`UIRenderer`. Also assigns `window.Renderer` and `window.UseDirtyRendering`.

!!! warning "Windows only"
    Throws `PlatformNotSupportedException` on non-Windows platforms.

```csharp
var renderer = RendererHelper.Attach(window, useDirtyRendering: false);
```

## `WindowHelper`

`HandyUI.SilkNet.Helper.WindowHelper`

```csharp
public static UIRenderer CreateRenderer(HandyWindow window, bool useDirtyRendering);
public static (HandyWindow window, UIRenderer renderer)
    CreateWindowAndGetRenderer(WindowConfiguration windowConfiguration);
```

Convenience factories.

### GLFW utilities

```csharp
public static void  TryCancelClose(IWindow window);
public static void  TrySetOpacity(IWindow window, float opacity);
public static float? TryGetOpacity(IWindow window);
```

Defensive wrappers that swallow exceptions.

## `ClipboardHelper`

`HandyUI.SilkNet.Helper.ClipboardHelper`

Bridges the OS clipboard to [`ClipboardManager`](clipboard-manager.md). It
listens for OS clipboard changes, writes them into the shared manager, and
pushes `SetClipboard` requests back out to the OS.

```csharp
public static class ClipboardHelper
{
    public static void EnsureInitialized();
}
```

### `EnsureInitialized()`

```csharp
public static void EnsureInitialized();
```

Creates the underlying clipboard listener, subscribes it to OS clipboard
events, and hooks `ClipboardManager.Instance.OnClipboardSet` so that
`SetClipboard(...)` calls from your UI reach the OS.

Call this **once** during app startup, typically after
[`ThemeHelper.EnsureInitialized()`](theme-helper.md#ensureinitialized):

```csharp
ThemeHelper.EnsureInitialized();
ClipboardHelper.EnsureInitialized();
```

!!! warning "Call once"
    `EnsureInitialized` subscribes to `ClipboardManager.OnClipboardSet` and
    to the clipboard monitor every time it's called. Calling it twice will
    double-fire events and duplicate clipboard writes. Guard with your own
    static flag if you need to re-enter safely:

    ```csharp
    private static bool _clipboardInit;
    if (!_clipboardInit) { ClipboardHelper.EnsureInitialized(); _clipboardInit = true; }
    ```

### What it does

- **Inbound** - when the OS clipboard changes:
  - Text → sets `ClipboardManager.Instance.ClipboardContentsText`
  - Bitmap → converts to `SKImage` and sets `ClipboardManager.Instance.ClipboardContentsImage`
- **Outbound** - when `ClipboardManager.OnClipboardSet` fires:
  - Calls `ClipboardService.SetText(text)` to write to the OS clipboard

### Dependencies

`ClipboardHelper` uses two NuGet packages internally:

- **`ClipboardMonitor`** - cross-platform OS clipboard-change listener
- **`TextCopy`** - writes text to the OS clipboard

Both are pulled in by `HandyUI.SilkNet` - no extra setup required.

!!! info "Windows bitmap path"
    On Windows 6.1+ (Win7+), bitmaps from the clipboard go through a
    `Bitmap` → `SKImage` conversion via `LockBits`/`Bgra8888`. On other
    platforms, the image is read from `IClipboardImage.Data` directly.

## `Win32Helper`

`HandyUI.SilkNet.Helper.Win32Helper`

Low-level Win32 access. Every method is a **no-op** when the handle is
`IntPtr.Zero`. Exposes public constants, a `WndProc` delegate, and every
Win32 struct needed for direct `user32` interaction.

### Window / style helpers

| Method | Effect |
| --- | --- |
| `ApplyClipChildren(HandyWindow?)` | Sets `WS_CLIPCHILDREN`. |
| `ApplyParent(nint child, nint parent)` | Calls `SetParent`. |
| `SetEnabled(nint, bool)` | Calls `EnableWindow`. |
| `InvalidateWindow(nint, bool erase = false)` | `InvalidateRect`. |
| `WakeWindow(nint)` | Posts `WM_NULL`. |
| `SetWindowPosition(nint, int x, int y)` | Moves window, keeps size/zorder. |
| `ShowWindowVisible(nint, bool)` | `ShowWindow(SW_HIDE / SW_SHOW)`. |
| `ApplyVisibleInTaskbar(nint, bool)` | Toggles `WS_EX_TOOLWINDOW` / `WS_EX_APPWINDOW`. |
| `ApplyMinimizeBox(nint, bool)` | Toggles `WS_MINIMIZEBOX`. |
| `ApplyMaximizeBox(nint, bool)` | Toggles `WS_MAXIMIZEBOX`. |
| `ApplyTitleBarVisibility(hWnd, bool visible, bool resizable, bool minimizable, bool maximizable)` | Rebuilds `WS_CAPTION` / `WS_SYSMENU` / `WS_THICKFRAME` / min/max bits. |

### Maximize / NCCALCSIZE

| Method | Effect |
| --- | --- |
| `IsMaximized(nint)` | Wraps `IsZoomed`. |
| `ApplyMaximizedBounds(nint hWnd, nint lParam)` | Fills `MINMAXINFO` from monitor work area. |
| `ApplyMaximizedClientRect(nint hWnd, nint lParam)` | Fills `NCCALCSIZE_PARAMS.rgrc[0]` from work area. |
| `RefreshMaximize(nint)` | `SW_RESTORE` + `SW_MAXIMIZE`. |

### Drag / hit test

```csharp
public static void StartWindowDrag(nint hWnd);   // HTCAPTION drag
```

Exports hit-test codes: `HTCLIENT`, `HTLEFT`, `HTRIGHT`, `HTTOP`,
`HTBOTTOM`, `HTTOPLEFT`, `HTTOPRIGHT`, `HTBOTTOMLEFT`, `HTBOTTOMRIGHT`,
`HTCAPTION`.

### DPI

```csharp
public static nint GetDC(nint hWnd);
public static int  ReleaseDC(nint hWnd, nint hDC);
public static int  GetDeviceCaps(nint hdc, int index);
```

With `LOGPIXELSX = 88`, `HORZSIZE = 4`, `VERTSIZE = 6`, and
`USER_DEFAULT_SCREEN_DPI = 96f`.

### Tray / menu / icons

```csharp
public static bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);
public static nint CreatePopupMenu();
public static bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string? lpNewItem);
public static bool DestroyMenu(nint hMenu);
public static nint TrackPopupMenu(nint hMenu, uint uFlags, int x, int y, int nReserved, nint hWnd, nint prcRect);
public static bool SetMenuDefaultItem(nint hMenu, uint uItem, uint fByPos);
public static bool SetForegroundWindow(nint hWnd);
public static bool GetCursorPos(out POINT lpPoint);
public static bool ScreenToClient(nint hWnd, ref POINT lpPoint);

public static nint CreateHIconFromSKImage(SKImage image);
public static void DestroyIcon(nint hIcon);
```

The tray icon implementation in `TrayIcon` builds entirely on these.

### Timing

```csharp
public static uint TimeBeginPeriod(uint uPeriod);
public static uint TimeEndPeriod(uint uPeriod);
```

`HandyWindow` uses `TimeBeginPeriod(1)` for its pump loop to keep frame
timing accurate.

### Structs and delegate

```csharp
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

public struct NOTIFYICONDATA    { … }
public struct ICONINFO          { … }
public struct POINT             { public int X, Y; }
public struct RECT              { public int Left, Top, Right, Bottom; }
public struct MONITORINFO       { … }
public struct MINMAXINFO        { … }
public struct NCCALCSIZE_PARAMS { … }
```

## Internal helpers

- `PlatformTool.EnsureSupportedPlatform()` - throws on non-Windows. Internal.
- `GlfwTool.EnsureGlfwInitialized()` - one-shot `Window.PrioritizeGlfw()`. Internal.