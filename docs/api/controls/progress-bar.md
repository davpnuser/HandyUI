# `ProgressBar`

`HandyUI.Controls.Controls.ProgressBar`

A horizontal progress bar. Determinate mode fills from `0` to `Value`;
indeterminate mode slides a segment back and forth.

```csharp
public class ProgressBar : BaseControl<ProgressBar>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }   // default: 200f
public float Height { get; set; }   // default: 4f
```

### `Value`

```csharp
public float Value { get; set; }   // clamped to [0, 100]
```

Percentage fill in determinate mode.

### `IsIndeterminate`

```csharp
public bool IsIndeterminate { get; set; }
```

When `true`, a 35 %-wide segment slides continuously. Assigning `true` starts
the animation tween; assigning `false` stops it and resets progress.

### `BarHeight`

```csharp
public float BarHeight { get; set; }   // default: 4f
```

Thickness of the visible bar (centered vertically in `Bounds`).

### `CornerRadius`

```csharp
public float CornerRadius { get; set; }   // default: 2f
```

## Events

### `WhenValueChanged`

```csharp
public event Action<float>? WhenValueChanged;
```

Raised whenever `Value` actually changes (clamped).

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides. `OnUpdate` is a no-op - animation is driven
by the tween.

## Fluent API

```csharp
public ProgressBar WithWidth(float width);
public ProgressBar WithHeight(float height);
public ProgressBar WithValue(float value);
public ProgressBar WithIsIndeterminate(bool indeterminate);
public ProgressBar WithBarHeight(float height);
public ProgressBar WithCornerRadius(float radius);
public ProgressBar OnValueChanged(Action<float> handler);
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

!!! tip "Indeterminate mode"
    Use `IsIndeterminate = true` when you don't know the total work (e.g.
    loading). Switch to determinate (`IsIndeterminate = false; Value = ...;`)
    once progress becomes measurable.