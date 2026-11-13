# `WindowConfiguration`

`HandyUI.SilkNet.Data.WindowConfiguration`

Options record for [`HandyWindow`](handy-window.md).

```csharp
public record WindowConfiguration
```

## Properties

### Core

| Property | Default | Description |
| --- | --- | --- |
| `Title` | `"Untitled Window"` | Window title. |
| `Size` | `800 × 600` | Client size. |

### Launch state

| Property | Default | Description |
| --- | --- | --- |
| `MinSize` / `MaxSize` | `null` | Size constraints. |
| `WindowState` | `Normal` | Initial state. |
| `WindowBorder` | `Resizable` | Border style. |
| `Opacity` | `1f` | Initial opacity. |
| `IsCenteredOnStartup` | `false` | Center on monitor. |
| `IsVisibleInTaskbar` | `true` | |
| `IsResizable` | `true` | |
| `IsMovable` | `true` | |
| `IsCloseable` | `true` | |
| `IsMaximizable` | `true` | |
| `IsMinimizable` | `true` | |
| `IsTitleBarVisible` | `true` | |
| `Icon` | `null` | `SKImage?` icon. |

### Rendering

| Property | Default | Description |
| --- | --- | --- |
| `TopMost` | `false` | |
| `VSync` | `true` | |
| `FramesPerSecond` | `60` | Ignored when `VSync` is on. |
| `UseDirtyRendering` | `true` | |
| `TransparentFrameBuffer` | `false` | |

### GLFW

| Property | Default | Description |
| --- | --- | --- |
| `AutoInitWindow` | `false` | |
| `AutoInitGlfw` | `true` | |
| `PumpThreadName` | `null` → `HandyWindow-<Title>` | |
| `CustomWindowOptions` | `null` | Overrides whole `WindowOptions`. |

!!! info "Removed in v4"
    `SharedContext`, `SubwindowParent`, `ModalParent`, and
    `InvalidateParentOnMove` were removed. Use direct Win32 parent/child
    management if you need modal or sub-windows.

## Static members

```csharp
public static WindowConfiguration Default => new();
```

## Example

```csharp
var cfg = WindowConfiguration.Default with
{
    Title = "Custom chrome",
    Size = new SKSize(800, 600),
    IsTitleBarVisible = false,
    IsResizable = true
};
```