# `Button`

`HandyUI.Controls.Controls.Button`

Filled text button with rounded corners, elevation shadow, hover color
blending, and a click ripple. Multi-line text is centered.

```csharp
public class Button : BaseControl<Button>
```

## Properties

| Property | Type | Default | Notes |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Split on `'\n'`; each line centered. |
| `CornerRadius` | `float` | `2f` | |
| `Elevation` | `int` | `2` | Shadow offset. Grows on hover. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | Assigning `null` restores default. |
| `IsBold` | `bool` | `false` | |
| `HorizontalPadding` / `VerticalPadding` | `float` | `16f` / `8f` | |

`Enabled = false` renders with `DisabledSurface`.

## Events

```csharp
public event Action? WhenClicked;
```

## Methods

```csharp
public override bool DoesIntersect(SKPoint point);
public override void OnUpdate(UpdateContext updateContext);
public override void OnDraw(SKCanvas canvas);

protected override bool OnMouse(MouseEventContext eventContext);
```

`OnMouse` starts the ripple on `MouseDownEvent` and raises `WhenClicked` on
`ClickEvent`.

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithCornerRadius, WithElevation,
WithFontSize, WithTypeface, WithIsBold, WithPadding,
WithWhenClicked(Action)
```

## Example

```csharp
var save = new Button()
    .WithText("Save\nchanges")
    .WithCornerRadius(6)
    .WithElevation(3)
    .WithIsBold(true)
    .WithPadding(20, 10)
    .WithWhenClicked(() => Save())
    .WithLocation(24, 80)
    .WithAddToRenderer(renderer);
```