# `TweenManager`

`HandyUI.Core.Helpers.TweenManager`

Frame-driven float interpolator. Derives from `BaseModule<TweenManager>` and
is registered as a module inside `UIRenderer` automatically.

```csharp
public class TweenManager : BaseModule<TweenManager>
```

## Constructor

```csharp
public TweenManager();
```

Calls `Start()` immediately so the manager ticks from frame one.

## Methods

### `To(...)`

```csharp
public Tween To(
    float start, float end, float duration,
    Action<float> onUpdate,
    EaseType ease = EaseType.Linear,
    Action? onComplete = null);
```

| Parameter | Description |
| --- | --- |
| `start` / `end` | Interpolation range. |
| `duration` | Seconds. Values `≤ 0` are clamped to `0.0001`. |
| `onUpdate` | Called once per tick with the eased value. |
| `ease` | Built-in easing curve. |
| `onComplete` | Called once when the tween finishes. |

Returns a [`Tween`](tween.md) handle.

```csharp
var tween = tweenManager.To(0f, 1f, 0.3f,
    v => Opacity = v,
    EaseType.CubicOut,
    () => Console.WriteLine("done"));
```

### `OnUpdate(UpdateContext)` - override

Advances all active tweens by `updateContext.DeltaTime`. Skipped when
`Enabled` is `false`.

## Accessing the manager

Inside a control's `OnUpdate` / `OnDraw` / `OnMouse` / `OnKey`, the
`TweenManager` is always available via:

```csharp
updateContext.TweenManager        // from OnUpdate
eventContext.TweenManager         // from OnMouse (MouseEventContext)
eventContext.Tweener              // from OnKey (KeyEventContext)
```

Or, from anywhere with a reference to a renderer:

```csharp
renderer.TweenManager
```

There is **no static `Tweener.Current`** in v4.

## `Easing`

`HandyUI.Core.Helpers.Easing`

```csharp
public static float Evaluate(float t, EaseType easeType);
```

Evaluates the eased value of `t` (clamped to `[0, 1]`). Called by
`TweenManager` internally; exposed for custom tween drivers.

## `EaseType`

`HandyUI.Core.Data.Enums.EaseType`

```csharp
public enum EaseType
{
    Linear,
    CubicIn,   CubicOut,   CubicInOut,
    SineIn,    SineOut,    SineInOut,
    QuartIn,   QuartOut,   QuartInOut
}
```

## Example

```csharp
public sealed class Fader : BaseControl<Fader>
{
    private float _opacity;
    private Tween? _tween;

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (eventContext.Type != MouseEventType.ClickEvent || !IsHovered) return false;

        if (_tween is { IsValid: true }) _tween.Cancel();
        _tween = eventContext.TweenManager.To(
            _opacity, 0f, 0.3f,
            v => { _opacity = v; Invalidate(); });
        return true;
    }

    public override bool DoesIntersect(SKPoint point) => Bounds.Contains(point);
    public override void OnUpdate(UpdateContext updateContext) { }
    public override void OnDraw(SKCanvas canvas) { /* use _opacity */ }
}
```