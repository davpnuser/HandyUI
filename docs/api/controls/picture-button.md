# `PictureButton`

`HandyUI.Controls.Controls.PictureButton`

Clickable image with rounded corners, hover brightening, and click ripple.

```csharp
public class PictureButton : BaseControl<PictureButton>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto → image size) |
| `Image` | `SKImage?` | `null` |
| `StretchMode` | `StretchMode` | `Uniform` |
| `CornerRadius` | `float` | `4f` |

## Events

```csharp
public event Action? WhenClicked;
public event Action<SKImage?>? WhenImageChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

`OnMouse` starts the ripple on `MouseDownEvent` and fires `WhenClicked` on
`ClickEvent`.

## Fluent API

```csharp
WithWidth, WithHeight, WithImage, WithStretchMode, WithCornerRadius,
WithWhenClicked, WithWhenImageChanged,
WithWhenHoverEntered, WithWhenHoverExited
```

## Example

```csharp
new PictureButton()
    .WithImage(iconImage)
    .WithWidth(32).WithHeight(32)
    .WithCornerRadius(16)
    .WithWhenClicked(() => Settings())
    .WithLocation(40, 20)
    .WithAddToRenderer(renderer);
```