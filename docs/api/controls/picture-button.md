# `PictureButton`

`HandyUI.Controls.Controls.PictureButton`

A clickable image with rounded corners, hover brightening, and a click
ripple.

```csharp
public class PictureButton : BaseControl<PictureButton>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size to the image.

### `Image` / `StretchMode`

```csharp
public SKImage? Image { get; set; }
public StretchMode StretchMode { get; set; }   // default: Uniform
```

### `CornerRadius`

```csharp
public float CornerRadius { get; set; }   // default: 4f
```

The image is clipped to a rounded rectangle of this radius, and the ripple
respects the same shape.

## Events

```csharp
public event Action? WhenClicked;
public event Action<SKImage?>? WhenImageChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

- `MouseDown` starts the ripple at the click position.
- `Click` raises `WhenClicked`.

## Fluent API

```csharp
public PictureButton WithWidth(float width);
public PictureButton WithHeight(float height);
public PictureButton WithImage(SKImage? image);
public PictureButton WithStretchMode(StretchMode mode);
public PictureButton WithCornerRadius(float radius);
public PictureButton OnClicked(Action handler);
public PictureButton OnImageChanged(Action<SKImage?> handler);
public PictureButton OnHoverEntered(Action handler);
public PictureButton OnHoverExited(Action handler);
```

## Example

```csharp
var iconButton = new PictureButton()
    .WithImage(iconImage)
    .WithWidth(32)
    .WithHeight(32)
    .WithCornerRadius(16)
    .OnClicked(() => Settings())
    .WithLocation(40, 20)
    .WithAddToRenderer(renderer);
```