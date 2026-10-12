# `Frame`

`HandyUI.Controls.Frame`

A rectangle with a solid background fill and an optional border. Useful as
a panel, card, or container for other controls.

```csharp
public class Frame : UIControlBase<Frame>
```

!!! warning "Namespace mismatch in the current source"
    `Frame.cs` currently declares `namespace HandyUI.Controls.Controls;` -
    every other control in the same package uses `HandyUI.Controls`. This is
    almost certainly a typo. Until it's fixed, you'll have to reference it as
    `HandyUI.Controls.Controls.Frame`. The rest of this page assumes the
    intended namespace.

## Properties

### `Theme`

```csharp
public Theme Theme { get; set; }
```

Palette used when the frame is disabled (and to seed the default background
and border colors). Assigning a new theme only overwrites `BackgroundColor`
or `BorderColor` if they haven't been customized via a `With…` call.

### `Width` / `Height` / `Size`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
public SKSize Size  { get; set; }
```

Convenience accessors over `Bounds`. Assigning any of them keeps the frame
anchored at its current `Location`.

### `BackgroundColor`

```csharp
public SKColor BackgroundColor { get; set; }
```

Fill color. Assigning it marks the channel as customized so a later
`Theme` swap won't overwrite it.

### `BorderColor`

```csharp
public SKColor BorderColor { get; set; }
```

Stroke color. Same customization semantics as `BackgroundColor`.

### `BorderThickness`

```csharp
public float BorderThickness { get; set; }   // default: 1f
```

Stroke width. Set to `0` to hide the border entirely.

### `BorderDirection`

```csharp
public BorderDirection BorderDirection { get; set; }   // default: Inside
```

Where the stroke is drawn relative to the frame's bounds. See
[`BorderDirection`](../border-direction.md).

### `IsEnabled` (shadowed)

```csharp
public new bool IsEnabled { get; set; }
```

Refreshes the brushes when toggled. Disabled frames use
`Theme.DarkerBackgroundColor` for the fill and `Theme.DisabledColor` for the
border.

## Methods

### `Draw(SKCanvas)`

```csharp
public override void Draw(SKCanvas canvas)
```

Draws the fill first (when its alpha is > 0), then the border (when the
thickness and alpha are both > 0). Border geometry is adjusted according to
`BorderDirection`.

## Fluent API

```csharp
public Frame WithTheme(Theme theme);
public Frame WithWidth(float width);
public Frame WithHeight(float height);
public Frame WithSize(float width, float height);
public Frame WithBackgroundColor(SKColor color);
public Frame WithBorder(SKColor color, float thickness = 1.0f,
                       BorderDirection direction = BorderDirection.Inside);
public Frame WithBorderColor(SKColor color);
public Frame WithBorderThickness(float thickness);
public Frame WithBorderDirection(BorderDirection direction);
```

## Example

```csharp
var card = new Frame()
    .WithSize(320, 180)
    .WithBackgroundColor(SKColors.White)
    .WithBorder(SKColor.Parse("#CBD5E1"), 1f, BorderDirection.Inside)
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);
```

!!! tip "Rounded corners"
    `Frame` draws rectangular corners only. For rounded panels, subclass it
    and override `Draw` to call `canvas.DrawRoundRect`.