# `Picture`

`HandyUI.Controls.Controls.Picture`

A non-interactive image with four stretch modes. Auto-sizes to the image
unless you override `Width`/`Height`.

```csharp
public class Picture : BaseControl<Picture>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size to `Image.Width` / `Image.Height`.

### `Image`

```csharp
public SKImage? Image { get; set; }
```

Assigning a new image triggers `ResizeToContent()` and raises
`WhenImageChanged`.

### `StretchMode`

```csharp
public StretchMode StretchMode { get; set; }   // default: Uniform
```

See [`StretchMode`](../stretch-mode.md) for behavior.

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

Raises `WhenClicked` on `Click`.

## Fluent API

```csharp
public Picture WithWidth(float width);
public Picture WithHeight(float height);
public Picture WithImage(SKImage? image);
public Picture WithStretchMode(StretchMode mode);
public Picture OnClicked(Action handler);
public Picture OnImageChanged(Action<SKImage?> handler);
public Picture OnHoverEntered(Action handler);
public Picture OnHoverExited(Action handler);
```

## Example

```csharp
var logo = new Picture()
    .WithImage(myImage)
    .WithWidth(256)
    .WithHeight(128)
    .WithStretchMode(StretchMode.Uniform)   // letterbox
    .WithLocation(32, 32)
    .WithAddToRenderer(renderer);
```

!!! tip "Aspect-ratio preservation"
    `StretchMode.Uniform` and `UniformToFill` preserve the source aspect
    ratio. Use `Fill` only when you deliberately want distortion.