# `ProgressBar`

`HandyUI.Controls.Controls.ProgressBar`

A horizontal progress bar in determinate or indeterminate mode.

```csharp
public class ProgressBar : BaseControl<ProgressBar>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `200f` / `4f` | |
| `Value` | `float` | `0` | Percentage, clamped to `[0, 100]`. |
| `IsIndeterminate` | `bool` | `false` | Toggles the sliding-segment animation. |
| `BarHeight` | `float` | `4f` | Visible bar thickness (centered vertically). |
| `CornerRadius` | `float` | `2f` | |

## Events

```csharp
public event Action<float>? WhenValueChanged;
```

## Methods

`DoesIntersect`, `OnDraw` - standard overrides. `OnUpdate` is a no-op
(animation is driven by `Tweener`).

## Fluent API

```csharp
public ProgressBar WithWidth(float width);
public ProgressBar WithHeight(float height);
public ProgressBar WithValue(float value);
public ProgressBar WithIsIndeterminate(bool indeterminate);
public ProgressBar WithBarHeight(float height);
public ProgressBar WithCornerRadius(float radius);

public ProgressBar WithWhenValueChanged(Action<float> handler);
```

## Example

```csharp
var progress = new ProgressBar()
    .WithWidth(320)
    .WithValue(45)
    .WithBarHeight(6)
    .WithCornerRadius(3)
    .WithLocation(40, 200)
    .WithAddToRenderer(renderer);
```