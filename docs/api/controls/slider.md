# `Slider`

`HandyUI.Controls.Controls.Slider`

Horizontal slider with an animated thumb that grows on hover / drag.

```csharp
public class Slider : BaseControl<Slider>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `200f` / `ThumbRadius * 2` |
| `Minimum` / `Maximum` | `float` | `0` / `100` |
| `Value` | `float` | `0` |
| `TrackHeight` | `float` | `4f` |
| `ThumbRadius` | `float` | `7f` |

`Minimum` and `Maximum` auto-clamp against each other. `Value` clamps to
`[Minimum, Maximum]`.

## Events

```csharp
public event Action<float>? WhenValueChanged;
```

## Methods

`OnMouse` starts dragging on `MouseDownEvent`, updates on `MoveEvent`, and
ends on `MouseUpEvent`.

## Fluent API

```csharp
WithWidth, WithHeight, WithMin, WithMax, WithValue,
WithTrackHeight, WithThumbRadius, WithWhenValueChanged
```

## Example

```csharp
new Slider()
    .WithWidth(240).WithMin(0).WithMax(100).WithValue(75)
    .WithWhenValueChanged(v => player.Volume = v / 100f)
    .WithLocation(40, 300)
    .WithAddToRenderer(renderer);
```