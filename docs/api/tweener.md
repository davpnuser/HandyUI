# `Tweener`

`HandyUI.Core.Components.Tweener`

A frame-driven float interpolator built on `BaseModule<Tweener>`. Every
`UIRenderer` owns one and registers it as a module, so tweens run
automatically while the render loop ticks.

```csharp
public sealed class Tweener : BaseModule<Tweener>
```

## Properties

### `Current`

```csharp
[field: ThreadStatic]
public static Tweener? Current { get; private set; }
```

The tweener active on the current thread. `UIRenderer` sets this to its own
`Tweener` before dispatching input, updating modules, and rendering - so
inside a control's `OnUpdate`/`OnDraw`/`OnMouse` you can just do
`Tweener.Current.To(...)` without wiring anything up.

```csharp
if (Tweener.Current is { } tweener)
{
    tweener.To(0f, 1f, 0.2f, v => _hover = v, EaseType.SineOut);
}
```

## Methods

### `To(...)`

```csharp
public TweenHandle To(
    float start, float end, float duration,
    Action<float> onUpdate,
    EaseType ease = EaseType.Linear,
    Action? onComplete = null);
```

Starts a tween from `start` to `end` over `duration` seconds. Returns a
[`TweenHandle`](#tweenhandle) for controlling it.

```csharp
var handle = tweener.To(0f, 1f, 0.3f,
    v => _progress = v,
    EaseType.CubicOut,
    () => Console.WriteLine("done"));
```

The tween is queued and applied on the next `OnUpdate`. It runs once by
default; use the handle's `Reverse()` to run it backwards.

### `OnUpdate(float)` - override

```csharp
public override void OnUpdate(float deltaTime);
```

Advances all active tweens by `deltaTime`.

## `TweenHandle`

`HandyUI.Core.Data.TweenHandle`

A lightweight struct wrapping an internal tween job.

```csharp
public readonly struct TweenHandle
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
| `IsValid` | `false` once completed or cancelled. |
| `Pause()` | Freezes progress at the current value. |
| `Play()` | Resumes a paused tween. |
| `Cancel()` | Stops the tween and resets its value to the start (or end when reversed). |
| `Reverse()` | Runs the tween backwards from its current position. |

!!! tip "Cancel behaviour"
    `Cancel()` invokes `onUpdate` once more with the *reset* value so your
    visual state snaps back to a sensible place.

## `EaseType`

`HandyUI.Core.Enums.EaseType`

```csharp
public enum EaseType
{
    Linear,
    CubicIn,   CubicOut,   CubicInOut,
    SineIn,    SineOut,    SineInOut,
    QuartIn,   QuartOut,   QuartInOut
}
```

## `Easing`

`HandyUI.Core.Components.Easing`

```csharp
public static float Evaluate(float t, EaseType easeType);
```

Evaluates the eased `t` for the given curve. `t` is clamped to `[0, 1]`.

## Example

```csharp
public sealed class Fader : BaseControl<Fader>
{
    private float _opacity;
    private TweenHandle _tween;

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Click && IsHovered && Tweener.Current is { } t)
        {
            _tween.Cancel();
            _tween = t.To(_opacity, 0f, 0.3f, v => { _opacity = v; Invalidate(); });
            return true;
        }
        return false;
    }

    public override bool DoesIntersect(SKPoint? p) => p is { } v && Bounds.Contains(v);
    public override void OnUpdate(float dt, SKPoint? mouse) { }
    public override void OnDraw(SKCanvas canvas) { /* use _opacity */ }
}
```