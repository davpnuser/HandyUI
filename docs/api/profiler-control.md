# `ProfilerControl`

`HandyUI.Core.Helpers.Controls.ProfilerControl`

!!! warning "Internal"
    `ProfilerControl` is `internal`. You cannot construct it directly. It is
    created automatically by `UIRenderer` in Debug builds when `UseProfiler`
    is `true`, and toggled at runtime with **`Ctrl+Shift+G`**.

The profiler overlay displays:

- FPS + target FPS
- V-Sync / Dirty Rendering / Foreground state
- Current cursor type
- Managed memory (MB), GC counts (gen 0/1/2)
- Total elapsed seconds
- A 200-frame frame-time graph with target-FPS line

## Interactive buttons

| Button | Effect |
| --- | --- |
| **Toggle DiRe** | Toggles `Host.UseDirtyRendering`. |
| **Toggle V-Sync** | Toggles `Host.VSync`. |
| **Inc. Target FPS** | `Host.FramesPerSecond += 5`. |
| **Dec. Target FPS** | `Host.FramesPerSecond -= 5`. |

## Enabling programmatically

```csharp
renderer.UseProfiler = true;
```

Enabling creates the control; disabling destroys and disposes it.