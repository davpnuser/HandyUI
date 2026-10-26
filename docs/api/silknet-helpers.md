# Silk.NET Helpers

## `RendererHelper`

`HandyUI.SilkNet.Helper.RendererHelper`

```csharp
public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true);
```

Creates a `RendererAdapter`, assigns it to the window, registers the window
with [`ThemeHelper`](theme-helper.md) for automatic theme-driven
invalidation, and returns the underlying `UIRenderer`.

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

Low-level Win32 window manipulation. All methods **no-op** when the handle is
`IntPtr.Zero`.

| Method | Effect |
| --- | --- |
| `ApplyClipChildren(HandyWindow?)` | Sets `WS_CLIPCHILDREN`. |
| `ApplyParent(nint child, nint parent)` | Calls `SetParent`. |
| `SetEnabled(nint, bool)` | Calls `EnableWindow`. |
| `InvalidateWindow(nint, bool erase = false)` | Calls `InvalidateRect`. |
| `WakeWindow(nint)` | Posts `WM_NULL` to wake the pump loop. |
| `ApplyVisibleInTaskbar(nint, bool)` | Toggles `WS_EX_TOOLWINDOW` / `WS_EX_APPWINDOW`. |
| `ApplyMinimizeBox(nint, bool)` | Toggles `WS_MINIMIZEBOX`. |
| `ApplyMaximizeBox(nint, bool)` | Toggles `WS_MAXIMIZEBOX`. |
| `GetWindowLongPtr(nint, int)` / `SetWindowLongPtr(nint, int, nint)` | 32/64-bit safe wrappers. |

!!! warning
    Prefer the `HandyWindow` properties (e.g. `IsMinimizable`) - they
    re-apply automatically after initialization.

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