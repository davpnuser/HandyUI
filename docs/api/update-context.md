# `UpdateContext`

`HandyUI.Core.Data.UpdateContext`

Read-only record struct handed to `OnUpdate` on every tick and via the
`TweenManager` / `Host` properties on `MouseEventContext`.

```csharp
public readonly record struct UpdateContext(
    TweenManager TweenManager,
    IHostWindow Host,
    float DeltaTime
);
```

| Field | Purpose |
| --- | --- |
| `TweenManager` | The renderer's shared animation engine. |
| `Host` | The `IHostWindow` currently driving the renderer. |
| `DeltaTime` | Seconds since the last frame, clamped by the host. |

## Usage in controls

Every control builds animations through `updateContext.TweenManager`:

```csharp
public override void OnUpdate(UpdateContext updateContext)
{
    if (!IsHovered) return;
    _hoverTween = updateContext.TweenManager.To(
        0f, 1f, 0.15f,
        v => { _hover = v; Invalidate(); },
        EaseType.SineOut);
}
```