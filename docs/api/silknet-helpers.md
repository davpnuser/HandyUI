# Silk.NET Helpers

## `RendererHelper`

`HandyUI.SilkNet.Helper.RendererHelper`

```csharp
public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true);
```

Creates a `RendererAdapter`, assigns it to the window, registers the window
with [`ThemeHelper`](theme-helper.md), and returns the `UIRenderer`.

!!! warning "Windows only"
    Throws `PlatformNotSupportedException` on non-Windows platforms.

```csharp
var renderer = RendererHelper.Attach(window, useDirtyRendering: false);
```

---

## `WindowHelper`

`HandyUI.SilkNet.Helper.WindowHelper`

```csharp
public static UIRenderer CreateRenderer(HandyWindow window, bool useDirtyRendering);
public static (HandyWindow window, UIRenderer renderer)
    CreateWindowAndGetRenderer(WindowConfiguration windowConfiguration);
```

Convenience factories. `CreateWindowAndGetRenderer` builds a `HandyWindow`
**and** attaches a renderer in one shot.

### Other functions

```csharp
public static void  TryCancelClose(IWindow window);
public static void  TrySetOpacity(IWindow window, float opacity);
public static float? TryGetOpacity(IWindow window);
```

Thin, defensive wrappers around GLFW that swallow exceptions.

---

## `Win32Helper`

`HandyUI.SilkNet.Helper.Win32Helper`

Low-level Win32 access. Every method is a **no-op** when the handle is
`IntPtr.Zero`. The class also exposes several public constants, a `WndProc`
delegate, and the Win32 structs required to interact with `user32` directly.

### Window lifecycle

| Method | Effect |
| --- | --- |
| `ApplyClipChildren(HandyWindow?)` | Sets `WS_CLIPCHILDREN`. |
| `ApplyParent(nint child, nint parent)` | Calls `SetParent`. |
| `SetEnabled(nint, bool)` | Calls `EnableWindow`. |
| `InvalidateWindow(nint, bool erase = false)` | Calls `InvalidateRect`. |
| `WakeWindow(nint)` | Posts `WM_NULL` to wake the pump loop. |
| `SetWindowPosition(nint, int x, int y)` | Moves the window with `SWP_NOSIZE \| SWP_NOZORDER \| SWP_NOACTIVATE`. |
| `ShowWindowVisible(nint, bool)` | `ShowWindow(SW_HIDE / SW_SHOW)`. |
| `DestroyWindow(nint)` | Calls `DestroyWindow`. |
| `GetWindowRect(nint, out RECT)` | Reads the outer window rect. |

### Styles and title bar

| Method | Effect |
| --- | --- |
| `ApplyVisibleInTaskbar(nint, bool)` | Toggles `WS_EX_TOOLWINDOW` / `WS_EX_APPWINDOW`. |
| `ApplyMinimizeBox(nint, bool)` | Toggles `WS_MINIMIZEBOX`. |
| `ApplyMaximizeBox(nint, bool)` | Toggles `WS_MAXIMIZEBOX`. |
| `ApplyTitleBarVisibility(hWnd, bool visible, bool resizable, bool minimizable, bool maximizable)` | Rebuilds `WS_CAPTION` / `WS_SYSMENU` / `WS_THICKFRAME` / min/max bits, then `SetWindowPos(..., SWP_FRAMECHANGED)`. |
| `IsMaximized(nint)` | Reads `WS_MAXIMIZE`. |
| `ApplyMaximizedBounds(hWnd, lParam)` | Writes `MINMAXINFO` for `WM_GETMINMAXINFO`. |
| `RefreshMaximize(nint)` | `SW_RESTORE` + `SW_MAXIMIZE` (forces correct size after style changes). |

### Drag / hit-test

```csharp
public static void StartWindowDrag(nint hWnd);
```

`ReleaseCapture()` + `SendMessage(WM_NCLBUTTONDOWN, HTCAPTION)`. Sends the
window into the OS drag loop. Bound to [`HandyWindow.BeginDrag()`](handy-window.md#isdragging--begindrag).

Hit-test codes are exported as constants so custom `WndProc` subclasses can
return them from `WM_NCHITTEST`:

```csharp
Win32Helper.HTCLIENT
Win32Helper.HTLEFT,   Win32Helper.HTRIGHT
Win32Helper.HTTOP,    Win32Helper.HTBOTTOM
Win32Helper.HTTOPLEFT,  Win32Helper.HTTOPRIGHT
Win32Helper.HTBOTTOMLEFT, Win32Helper.HTBOTTOMRIGHT
```

### Tray icons and popup menus

```csharp
public static bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);
public static nint CreatePopupMenu();
public static bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string? lpNewItem);
public static bool DestroyMenu(nint hMenu);
public static nint TrackPopupMenu(nint hMenu, uint uFlags,
    int x, int y, int nReserved, nint hWnd, nint prcRect);
public static bool SetMenuDefaultItem(nint hMenu, uint uItem, uint fByPos);
public static bool SetForegroundWindow(nint hWnd);
public static bool GetCursorPos(out POINT lpPoint);
public static nint GetModuleHandle(string? lpModuleName);
public static bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);
```

These are the pieces [`TrayIcon`](tray-icon.md) builds on top of. You
normally won't call them directly, but they're available for custom
shell integration.

### Icons

```csharp
public static nint CreateHIconFromSKImage(SKImage image);
public static void DestroyIcon(nint hIcon);
```

`CreateHIconFromSKImage` converts an `SKImage` to BGRA, builds the required
mask bitmap, and returns an `HICON`. `DestroyIcon` frees it. `TrayIcon` and
`HandyWindow.Icon` both use these internally.

### Delegates and structs

```csharp
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

public struct NOTIFYICONDATA { … }   // Windows SHELLAPI struct
public struct ICONINFO       { … }
public struct POINT          { public int X, Y; }
public struct RECT           { public int Left, Top, Right, Bottom; }
public struct MONITORINFO    { … }
public struct MINMAXINFO     { … }
```

### Public constants

| Group | Members |
| --- | --- |
| Window styles | `WS_CLIPCHILDREN`, `WS_MINIMIZEBOX`, `WS_MAXIMIZEBOX`, `WS_CAPTION`, `WS_SYSMENU`, `WS_THICKFRAME`, `WS_MAXIMIZE` |
| Extended styles | `WS_EX_TOOLWINDOW`, `WS_EX_APPWINDOW` |
| Messages | `WM_NULL`, `WM_COMMAND`, `WM_GETMINMAXINFO`, `WM_NCCALCSIZE`, `WM_NCHITTEST`, `WM_USER`, `WM_TRAYICON`, `WM_LBUTTONUP`, `WM_LBUTTONDBLCLK`, `WM_RBUTTONUP` |
| Hit-test codes | `HTCLIENT`, `HTLEFT`, `HTRIGHT`, `HTTOP`, `HTTOPLEFT`, `HTTOPRIGHT`, `HTBOTTOM`, `HTBOTTOMLEFT`, `HTBOTTOMRIGHT` |
| Tray flags | `NIM_ADD`, `NIM_MODIFY`, `NIM_DELETE`, `NIF_MESSAGE`, `NIF_ICON`, `NIF_TIP`, `NIF_STATE`, `NIF_INFO`, `NIF_GUID`, `NIF_REALTIME`, `NIF_SHOWTIP` |
| Menu flags | `MF_STRING`, `MF_SEPARATOR`, `MF_CHECKED`, `MF_UNCHECKED`, `MF_GRAYED`, `MF_DISABLED`, `MF_ENABLED` |
| Track-popup flags | `TPM_LEFTALIGN`, `TPM_CENTERALIGN`, `TPM_RIGHTALIGN`, `TPM_TOPALIGN`, `TPM_VCENTERALIGN`, `TPM_BOTTOMALIGN`, `TPM_LEFTBUTTON`, `TPM_RIGHTBUTTON`, `TPM_NONOTIFY`, `TPM_RETURNCMD` |
| Misc | `GWLP_WNDPROC`, `HWND_MESSAGE` |

### 32/64-bit wrappers

```csharp
public static nint GetWindowLongPtr(nint hWnd, int nIndex);
public static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
```

### Cross-platform note

All methods **no-op** outside Windows. You can call them unconditionally from
code that runs cross-platform without crashing.

---

## `PlatformTool`

`HandyUI.SilkNet.Tool.PlatformTool`

```csharp
internal static void EnsureSupportedPlatform();
```

Throws `PlatformNotSupportedException` on non-Windows platforms. Internal.

---

## `GlfwTool`

`HandyUI.SilkNet.Tool.GlfwTool`

```csharp
internal static void EnsureGlfwInitialized();
```

Ensures `Window.PrioritizeGlfw()` is called once. Internal.