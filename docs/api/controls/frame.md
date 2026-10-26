# `Frame`

`HandyUI.Controls.Controls.Frame`

A rounded rectangle with a background, optional border, elevation shadow,
and click/hover events. Useful as a panel, card, or container.

```csharp
public class Frame : BaseControl<Frame>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }   // default: 100f
public float Height { get; set; }   // default: 100f
```

Unlike most controls, `Frame` defaults to `100 × 100` and *does* auto-size
when set to `0`.

### `CornerRadius`

```csharp
public float CornerRadius { get; set; }   // default: 4f
```

### `Elevation`

```csharp
public int Elevation { get; set; }   // default: 1
```

Shadow offset when `HasShadow` is `true`. `0` skips the shadow.

### `HasBorder` / `BorderWidth` / `BorderDirection`

```csharp
public bool HasBorder       { get; set; }   // default: true
public float BorderWidth    { get; set; }   // default: 1f
public BorderDirection BorderDirection { get; set; }   // default: Inside
```

### `HasShadow`

```csharp
public bool HasShadow { get; set; }   // default: true
```

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<float, float>? WhenSizeChanged;
```

`WhenSizeChanged` receives `(Width, Height)` after the frame resizes.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Raises `WhenClicked` on `Click`.

## Fluent API

```csharp
public Frame WithWidth(float width);
public Frame WithHeight(float height);
public Frame WithCornerRadius(float radius);
public Frame WithElevation(int elevation);
public Frame WithHasBorder(bool hasBorder);
public Frame WithBorderWidth(float width);
public Frame WithBorderDirection(BorderDirection dir);
public Frame WithHasShadow(bool hasShadow);
public Frame OnClicked(Action handler);
public Frame OnHoverEntered(Action handler);
public Frame OnHoverExited(Action handler);
public Frame OnSizeChanged(Action<float, float> handler);
```

## Example

```csharp
var card = new Frame()
    .WithWidth(320)
    .WithHeight(180)
    .WithCornerRadius(8)
    .WithElevation(3)
    .WithBorderWidth(1)
    .WithBorderDirection(BorderDirection.Inside)
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);
```