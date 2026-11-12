# `Tween`

`HandyUI.Core.Data.Tween`

Handle returned by `TweenManager.To(...)`. Wraps an internal `TweenJob`.

```csharp
public class Tween
{
    public bool IsValid { get; }

    public void Pause();
    public void Play();
    public void Cancel();
    public void Reverse();
}
```

| Member | Effect |
| --- | --- |
| `IsValid` | `true` until the tween completes or is cancelled. |
| `Pause()` | Freezes progress at the current value. |
| `Play()` | Resumes a paused tween. |
| `Cancel()` | Stops the tween and resets the value to its start (or end when reversed). |
| `Reverse()` | Reverses the direction of the tween. |

!!! tip "Store handles, not booleans"
    A common pattern is to store `Tween?` and use `is { IsValid: true }` to
    check whether it's still running before cancelling:

    ```csharp
    if (_hoverTween is { IsValid: true }) _hoverTween.Cancel();
    _hoverTween = updateContext.TweenManager.To(0f, 1f, 0.15f, ...);
    ```

## `TweenStatus`

`HandyUI.Core.Data.Enums.TweenStatus`

```csharp
public enum TweenStatus
{
    None = 0,
    Playing = 1,
    Stopped = 2,
    Paused = 3,
}
```

Currently informational; the manager exposes it in its internal `TweenJob`
for external tooling.