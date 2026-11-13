# `ProgressBar`

`HandyUI.Controls.Controls.ProgressBar`

Horizontal progress bar. Determinate or indeterminate.

```csharp
public class ProgressBar : BaseControl<ProgressBar>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `200f` / `4f` |
| `Value` | `float` | `0` (clamped to `[0, 100]`) |
| `IsIndeterminate` | `bool` | `false` |
| `BarHeight` | `float` | `4f` |
| `CornerRadius` | `float` | `2f` |

## Events

```csharp
public event Action<float>? WhenValueChanged;
```

## Methods

`OnUpdate` drives the indeterminate animation via
`updateContext.TweenManager`. `OnDraw` renders the fill or sliding segment.

## Fluent API

```csharp
WithWidth, WithHeight, WithValue, WithIsIndeterminate,
WithBarHeight, WithCornerRadius, WithWhenValueChanged
```

## Example

```csharp
new ProgressBar()
    .WithWidth(320)
    .WithValue(45)
    .WithBarHeight(6)
    .WithCornerRadius(3)
    .WithLocation(40, 200)
    .WithAddToRenderer(renderer);
```