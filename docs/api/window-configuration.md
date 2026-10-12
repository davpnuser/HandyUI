# `WindowConfiguration`

`HandyUI.SilkNet.Classes.Structs.WindowConfiguration`

An immutable options record for `HandyWindow`.

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
| `IsVisibleInTaskbar` | `true` | Win32 taskbar. |
| `IsResizable` | `true` | |
| `IsMovable` | `true` | |
| `IsCloseable` | `true` | |
| `IsMaximizable` | `true` | |
| `IsMinimizable` | `true` | |

### Parent-child

| Property | Default | Description |
| --- | --- | --- |
| `SubwindowParent` | `null` | Native parent. |
| `ModalParent` | `null` | Modal parent; disables it. |

!!! warning "Mutually exclusive"
    A window cannot have the same window as both `SubwindowParent` and
    `ModalParent`. This throws at construction.

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
    Because it's a record, tweaking a default is trivial:

    ```csharp
    var cfg = WindowConfiguration.Default with
    {
        Title = "Chat",
        Size = new SKSize(420, 700),
        IsResizable = false
    };
    ```

## Static members

### `Default`

```csharp
public static WindowConfiguration Default => new();
```