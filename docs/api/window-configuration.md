# `WindowConfiguration`

`HandyUI.SilkNet.Data.WindowConfiguration`

An immutable options record for [`HandyWindow`](handy-window.md).

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
| `WindowState` | `Normal` | Initial window state. |
| `WindowBorder` | `Resizable` | Border style. |
| `Opacity` | `1f` | Initial opacity. |
| `IsCenteredOnStartup` | `false` | Center on the monitor. |
| `IsVisibleInTaskbar` | `true` | Win32 taskbar visibility. |
| `IsResizable` | `true` | |
| `IsMovable` | `true` | |
| `IsCloseable` | `true` | |
| `IsMaximizable` | `true` | |
| `IsMinimizable` | `true` | |
| `Icon` | `null` | Window icon as an `SKImage`. |

### Parent-child

| Property | Default | Description |
| --- | --- | --- |
| `SubwindowParent` | `null` | Native parent window. |
| `ModalParent` | `null` | Modal parent; disables it. |

!!! warning "Mutually exclusive"
    A window cannot have the same window as both `SubwindowParent` and
    `ModalParent`. Throws at construction.

### Rendering

| Property | Default | Description |
| --- | --- | --- |
| `TopMost` | `false` | |
| `VSync` | `true` | |
| `FramesPerSecond` | `60` | Ignored when `VSync` is on. |
| `UseDirtyRendering` | `true` | |
| `InvalidateParentOnMove` | `true` | |
| `TransparentFrameBuffer` | `false` | |

### OpenGL

| Property | Default | Description |
| --- | --- | --- |
| `SharedContext` | `null` | Share GL resources across windows. |
| `AutoInitWindow` | `false` | |
| `AutoInitGlfw` | `true` | |

### Advanced

| Property | Default | Description |
| --- | --- | --- |
| `PumpThreadName` | `null` → `HandyWindow-<Title>` | Name of the pump thread. |
| `CustomWindowOptions` | `null` | Overrides the whole `WindowOptions`. |

!!! tip "`with` expressions"
    ```csharp
    var cfg = WindowConfiguration.Default with
    {
        Title = "Chat",
        Size = new SKSize(420, 700),
        IsResizable = false,
        Icon = SKImage.FromEncodedData("Assets/chat.png")
    };
    ```

## Static members

### `Default`

```csharp
public static WindowConfiguration Default => new();
```