# `Button`

`HandyUI.Controls.Controls.Button`

A filled text button with rounded corners, elevation shadow, hover color
blending, and a click ripple. Text can be multi-line.

```csharp
public class Button : BaseControl<Button>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` | `float` | `0` | Auto-sized when `0`. |
| `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Split on `'\n'` - each newline starts a new centered line. |
| `CornerRadius` | `float` | `2f` | Rounds the fill and clipping path. |
| `Elevation` | `int` | `2` | Shadow offset in pixels. Grows by `2` while hovered. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | Assigning `null` restores the default. |
| `IsBold` | `bool` | `false` | Embolds the font. |
| `HorizontalPadding` | `float` | `16f` | Content width padding. |
| `VerticalPadding` | `float` | `8f` | Content height padding. |

## Events

### `WhenClicked`

```csharp
public event Action? WhenClicked;
```

Raised on the renderer-synthesized `Click` event.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides. `OnUpdate` drives the hover color blend.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Starts the ripple on `MouseDown` and raises `WhenClicked` on `Click`.

## Fluent API

```csharp
public Button WithWidth(float width);
public Button WithHeight(float height);
public Button WithText(string text);
public Button WithCornerRadius(float radius);
public Button WithElevation(int elevation);
public Button WithFontSize(float size);
public Button WithTypeface(SKTypeface? typeface);
public Button WithIsBold(bool bold);
public Button WithPadding(float horizontal, float vertical);

public Button WithWhenClicked(Action handler);
```

## Example

```csharp
var save = new Button()
    .WithText("Save\nchanges")
    .WithCornerRadius(6)
    .WithElevation(3)
    .WithFontSize(15)
    .WithIsBold(true)
    .WithPadding(20, 10)
    .WithWhenClicked(() => Save())
    .WithLocation(24, 80)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```