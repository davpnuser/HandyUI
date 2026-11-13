# `Frame`

`HandyUI.Controls.Controls.Frame`

Rounded rectangle with optional background, border, elevation shadow, and
click / hover events.

```csharp
public class Frame : BaseControl<Frame>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `100f` |
| `CornerRadius` | `float` | `4f` |
| `Elevation` | `int` | `1` |
| `HasBorder` | `bool` | `true` |
| `BorderWidth` | `float` | `1f` |
| `BorderDirection` | `BorderDirection` | `Inside` |
| `HasShadow` | `bool` | `true` |
| `HasBackground` | `bool` | `true` |

## Events

```csharp
public event Action? WhenMouseDown;
public event Action? WhenMouseUp;
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<float, float>? WhenSizeChanged;
```

`WhenSizeChanged` receives `(Width, Height)` after resizes.

## Methods

Standard overrides. `OnMouse` fires down / up / click while hovered.
`OnChildAdded` invalidates so the frame redraws its background behind new
children.

## Fluent API

```csharp
WithWidth, WithHeight, WithSize(float, float), WithSize(SKSize),
WithCornerRadius, WithElevation, WithHasBorder, WithHasShadow,
WithHasBackground, WithBorderWidth, WithBorderDirection,
WithWhenMouseDown, WithWhenMouseUp, WithWhenClicked,
WithWhenHoverEntered, WithWhenHoverExited, WithWhenSizeChanged
```

## Example

```csharp
// Card with elevation + border.
var card = new Frame()
    .WithSize(320, 180)
    .WithCornerRadius(8)
    .WithElevation(3)
    .WithBorderWidth(1)
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);

// Transparent container.
var overlay = new Frame()
    .WithSize(400, 300)
    .WithHasBackground(false)
    .WithHasBorder(false)
    .WithHasShadow(false)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom title bar"
    For a HandyWindow with `IsTitleBarVisible = false`, add the top strip
    of the window to `window.DragRegions` - see
    [`HandyWindow → DragRegions`](../handy-window.md#dragregions).