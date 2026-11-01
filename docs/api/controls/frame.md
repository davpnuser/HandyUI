# `Frame`

`HandyUI.Controls.Controls.Frame`

A rounded rectangle with a background fill, optional border, elevation
shadow, and click/hover events. Useful as a panel, card, container, or
custom title bar.

```csharp
public class Frame : BaseControl<Frame>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `100f` | Frame size (auto-sizes when set to `0`). |
| `CornerRadius` | `float` | `4f` | |
| `Elevation` | `int` | `1` | Shadow offset. |
| `HasBorder` | `bool` | `true` | Draw the border. |
| `BorderWidth` | `float` | `1f` | |
| `BorderDirection` | `BorderDirection` | `Inside` | See [`BorderDirection`](../border-direction.md). |
| `HasShadow` | `bool` | `true` | Draw the elevation shadow. |
| `HasBackground` | `bool` | `true` | Draw the fill. Set `false` for transparent containers. |

## Events

```csharp
public event Action? WhenMouseDown;
public event Action? WhenMouseUp;
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<float, float>? WhenSizeChanged;
```

`WhenSizeChanged` receives `(Width, Height)` after the frame resizes.

## Methods

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Raises `WhenMouseDown` / `WhenMouseUp` / `WhenClicked` while hovered.

## Fluent API

```csharp
public Frame WithWidth(float width);
public Frame WithHeight(float height);
public Frame WithSize(float width, float height);
public Frame WithSize(SKSize size);
public Frame WithCornerRadius(float radius);
public Frame WithElevation(int elevation);
public Frame WithHasBorder(bool hasBorder);
public Frame WithHasShadow(bool hasShadow);
public Frame WithHasBackground(bool hasBackground);
public Frame WithBorderWidth(float width);
public Frame WithBorderDirection(BorderDirection dir);

public Frame WithWhenMouseDown(Action handler);
public Frame WithWhenMouseUp(Action handler);
public Frame WithWhenClicked(Action handler);
public Frame WithWhenHoverEntered(Action handler);
public Frame WithWhenHoverExited(Action handler);
public Frame WithWhenSizeChanged(Action<float, float> handler);
```

## Example

```csharp
var card = new Frame()
    .WithSize(320, 180)
    .WithCornerRadius(8)
    .WithElevation(3)
    .WithBorderWidth(1)
    .WithWhenClicked(() => OpenSettings())
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);

// Transparent container:
var overlay = new Frame()
    .WithSize(400, 300)
    .WithHasBackground(false)
    .WithHasBorder(false)
    .WithHasShadow(false)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom title bar"
    Set `HasBackground = false` and `HasBorder = false` on a `Frame` placed
    at the top of a `HandyWindow` with `IsTitleBarVisible = false`, then
    handle `WhenMouseDown` by calling `window.BeginDrag` for OS-driven
    dragging.