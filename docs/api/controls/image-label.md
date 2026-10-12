# `ImageLabel`

`HandyUI.Core.Classes.Controls.ImageLabel`

A non-interactive image. By default it auto-sizes its `Bounds` to the
image's natural size; you can override that with explicit width/height or
disable auto sizing.

```csharp
public class ImageLabel : UIControlBase<ImageLabel>
```

## Constructor

```csharp
public ImageLabel(SKImage? image = null, bool autoSize = true)
```

## Properties

### `Image`

```csharp
public SKImage? Image { get; set; }
```

Setting this recalculates `Bounds` when `AutoSize` is `true`.

### `AutoSize`

```csharp
public bool AutoSize { get; set; }   // default: true
```

- `true` → `Bounds` follows `Image.Width` / `Image.Height`, unless explicit
  `Width` / `Height` were set, in which case the image is letterboxed inside
  them.
- `false` → `Bounds` stays at the last explicit size (or the image size if
  none was given).

### `Width` / `Height` / `Size`

```csharp
public float  Width  { get; set; }
public float  Height { get; set; }
public SKSize Size   { get; set; }
```

Explicit overrides. Setting any of them disables auto-sizing from the
image's natural size until `ClearExplicitSize()` is called.

### `Theme`

```csharp
public ThemeRecord Theme { get; set; }
```

Used to tint the image when the control is disabled.

## Methods

### `ClearExplicitSize()`

```csharp
public void ClearExplicitSize()
```

Forgets explicit width/height and re-runs the auto-size logic.

### `RecalculateBounds()`

```csharp
public bool RecalculateBounds()
```

Recomputes `Bounds`. Returns `true` if the bounds actually changed.

### `Draw(SKCanvas)`

```csharp
public override void Draw(SKCanvas canvas)
```

When both an explicit width and height are set (and `AutoSize` is `true`),
the image is scaled to fit within `Bounds` while preserving aspect ratio.
Otherwise the image is drawn stretched to fill `Bounds`.

## Fluent API

```csharp
public ImageLabel WithTheme(ThemeRecord theme);
public ImageLabel WithImage(SKImage? image);
public ImageLabel WithAutoSize(bool autoSize);
public ImageLabel WithWidth(float width);
public ImageLabel WithHeight(float height);
public ImageLabel WithSize(float width, float height);
```

## Example

```csharp
var logo = new ImageLabel(myImage)
    .WithSize(256, 128)      // letterbox the source image
    .WithLocation(32, 32)
    .WithAddToRenderer(renderer);
```

!!! tip "Aspect-ratio preservation"
    With `AutoSize = true` and both `Width` and `Height` set, the image is
    scaled to *fit* - never cropped, never stretched. This makes it safe to
    use for arbitrary logo/icon slots.