# `PictureButton`

`HandyUI.Controls.Controls.PictureButton`

A clickable image with rounded corners, hover brightening, and a click
ripple.

```csharp
public class PictureButton : BaseControl<PictureButton>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Image` | `SKImage?` | `null` | |
| `StretchMode` | `StretchMode` | `Uniform` | |
| `CornerRadius` | `float` | `4f` | Also shapes the ripple clip. |

## Events

```csharp
public event Action? WhenClicked;
public event Action<SKImage?>? WhenImageChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public PictureButton WithWidth(float width);
public PictureButton WithHeight(float height);
public PictureButton WithImage(SKImage? image);
public PictureButton WithStretchMode(StretchMode mode);
public PictureButton WithCornerRadius(float radius);

public PictureButton WithWhenClicked(Action handler);
public PictureButton WithWhenImageChanged(Action<SKImage?> handler);
public PictureButton WithWhenHoverEntered(Action handler);
public PictureButton WithWhenHoverExited(Action handler);
```

## Example

```csharp
var iconButton = new PictureButton()
    .WithImage(iconImage)
    .WithWidth(32)
    .WithHeight(32)
    .WithCornerRadius(16)
    .WithWhenClicked(() => Settings())
    .WithLocation(40, 20)
    .WithAddToRenderer(renderer);
```