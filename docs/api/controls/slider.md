# `Slider`

`HandyUI.Controls.Controls.Slider`

A horizontal slider with an animated thumb that grows on hover and drag.

```csharp
public class Slider : BaseControl<Slider>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }   // default: 200f
public float Height { get; set; }
```

`Height` defaults to `ThumbRadius * 2` when left at `0`.

### `Minimum` / `Maximum` / `Value`

```csharp
public float Minimum { get; set; }   // default: 0
public float Maximum { get; set; }   // default: 100
public float Value   { get; set; }   // clamped to [Minimum, Maximum]
```

Adjusting `Minimum` above `Maximum` clamps `Maximum` upward (and vice versa).

### `TrackHeight` / `ThumbRadius`

```csharp
public float TrackHeight { get; set; }   // default: 4f
public float ThumbRadius { get; set; }   // default: 7f
```

## Events

### `WhenValueChanged`

```csharp
public event Action<float>? WhenValueChanged;
```

Raised on every value change during drag.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

- `MouseDown` on the track begins dragging and jumps to the click position.
- `Move` while dragging updates `Value`.
- `MouseUp` ends the drag and fades the hover glow if the cursor left.

## Fluent API

```csharp
public Slider WithWidth(float width);
public Slider WithHeight(float height);
public Slider WithMin(float min);
public Slider WithMax(float max);
public Slider WithValue(float value);
public Slider WithTrackHeight(float height);
public Slider WithThumbRadius(float radius);
public Slider OnValueChanged(Action<float> handler);
```

## Example

```csharp
var volume = new Slider()
    .WithWidth(240)
    .WithMin(0)
    .WithMax(100)
    .WithValue(75)
    .OnValueChanged(v => player.Volume = v / 100f)
    .WithLocation(40, 300)
    .WithAddToRenderer(renderer);
```