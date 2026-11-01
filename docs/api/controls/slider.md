# `Slider`

`HandyUI.Controls.Controls.Slider`

A horizontal slider with an animated thumb that grows on hover and drag.

```csharp
public class Slider : BaseControl<Slider>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `200f` / `ThumbRadius*2` | |
| `Minimum` / `Maximum` | `float` | `0` / `100` | Auto-clamped against each other. |
| `Value` | `float` | `0` | Clamped to `[Minimum, Maximum]`. |
| `TrackHeight` | `float` | `4f` | |
| `ThumbRadius` | `float` | `7f` | |

## Events

```csharp
public event Action<float>? WhenValueChanged;
```

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public Slider WithWidth(float width);
public Slider WithHeight(float height);
public Slider WithMin(float min);
public Slider WithMax(float max);
public Slider WithValue(float value);
public Slider WithTrackHeight(float height);
public Slider WithThumbRadius(float radius);

public Slider WithWhenValueChanged(Action<float> handler);
```

## Example

```csharp
var volume = new Slider()
    .WithWidth(240)
    .WithMin(0)
    .WithMax(100)
    .WithValue(75)
    .WithWhenValueChanged(v => player.Volume = v / 100f)
    .WithLocation(40, 300)
    .WithAddToRenderer(renderer);
```