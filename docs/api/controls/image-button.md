# `ImageButton`

`HandyUI.Controls.ImageButton`

A clickable button that renders an optional image followed by an optional
text label. Supports hover and pressed visual states, custom fonts, and
border direction.

```csharp
public class ImageButton : UIControlBase<ImageButton>
```

## Constructor

```csharp
public ImageButton(
    SKImage? image = null,
    string    text   = "",
    float     width  = 120f,
    float     height = 36f)
```

## Properties

### `Image` / `Text`

```csharp
public SKImage? Image { get; set; }
public string   Text  { get; set; }   // default: string.Empty
```

Either can be `null` / empty - the button simply omits that element.

### `AutoSizeImage`

```csharp
public bool AutoSizeImage { get; set; }   // default: true
```

When `true`, the image is scaled to fit the button's height (preserving
aspect ratio). When `false`, the explicit `ImageWidth` / `ImageHeight` are
used as-is.

### `ImageWidth` / `ImageHeight` / `ImageSize`

```csharp
public float ImageWidth  { get; set; }
public float ImageHeight { get; set; }
public float ImageSize   { get; set; }   // sets both
```

Explicit dimensions for the image container. Reading these when no explicit
value has been set falls back to `AutoSizeImage` logic.

### `Spacing`

```csharp
public float Spacing { get; set; }   // default: 6f
```

Gap between the image and the text.

### `TextSize`

```csharp
public float TextSize { get; set; }   // default: 13f
```

### Font properties

```csharp
public SKTypeface?       Typeface   { get; set; }
public string            FontFamily { get; set; }   // "Segoe UI"
public SKFontStyleWeight FontWeight { get; set; }   // Normal
public SKFontStyleWidth  FontWidth  { get; set; }   // Normal
public SKFontStyleSlant  FontSlant  { get; set; }   // Upright
```

When `Typeface` is `null`, a typeface is resolved from `FontFamily`,
`FontWeight`, `FontWidth`, and `FontSlant` and cached until any of those
change.

### Colors

```csharp
public SKColor NormalBackgroundColor  { get; set; }
public SKColor HoverBackgroundColor   { get; set; }
public SKColor PressedBackgroundColor { get; set; }
public SKColor NormalBorderColor      { get; set; }
public SKColor ActiveBorderColor      { get; set; }
public SKColor TextColor              { get; set; }
```

Assigning any of these marks the corresponding channel as customized so a
later `Theme` swap won't overwrite it.

### Convenience aliases

```csharp
public SKColor BackgroundColor { get; set; }   // → NormalBackgroundColor
public SKColor HoverColor      { get; set; }   // → HoverBackgroundColor
public SKColor PressedColor    { get; set; }   // → PressedBackgroundColor
public SKColor BorderColor     { get; set; }   // → NormalBorderColor
```

### `Theme`

```csharp
public Theme Theme { get; set; }
```

### `BorderThickness` / `BorderDirection`

```csharp
public float           BorderThickness { get; set; }   // default: 1f
public BorderDirection BorderDirection { get; set; }   // default: Inside
```

## Events

```csharp
public event Action? Clicked;
public Action?       OnClick { get; set; }
```

Both are invoked on a click. `WithOnClick` subscribes to both, so prefer
one or the other in your own code to avoid double invocation.

!!! warning "Double firing with `WithOnClick`"
    `WithOnClick` subscribes the **same** delegate to both `Clicked` and
    `OnClick`. If you also subscribe to `Clicked` manually with the same
    handler, it will run twice. Use one channel consistently.

## Methods

### `ClearExplicitImageSize()`

```csharp
public void ClearExplicitImageSize()
```

Clears any explicit width/height so the image falls back to auto sizing.

## Fluent API

```csharp
public ImageButton WithTheme(Theme theme);
public ImageButton WithWidth(float width);
public ImageButton WithHeight(float height);
public ImageButton WithSize(float width, float height);
public ImageButton WithImage(SKImage? image);
public ImageButton WithText(string text);
public ImageButton WithAutoSizeImage(bool autoSize);
public ImageButton WithImageSize(float width, float height);
public ImageButton WithFont(string family, float size = 13f,
    SKFontStyleWeight weight = SKFontStyleWeight.Normal,
    SKFontStyleSlant  slant  = SKFontStyleSlant.Upright);
public ImageButton WithTypeface(SKTypeface? typeface);
public ImageButton WithFontWeight(SKFontStyleWeight weight);
public ImageButton WithFontSlant(SKFontStyleSlant slant);
public ImageButton WithColors(SKColor normal, SKColor hover, SKColor pressed);
public ImageButton WithColors(SKColor bg, SKColor hover, SKColor pressed,
                              SKColor border, SKColor text);
public ImageButton WithBorder(SKColor normalColor, SKColor activeColor,
    float thickness = 1.0f,
    BorderDirection direction = BorderDirection.Inside);
public ImageButton WithBorderThickness(float thickness);
public ImageButton WithBorderDirection(BorderDirection direction);
public ImageButton WithOnClick(Action onClick);
```

## Example

```csharp
var icon = SKImage.FromEncodedData(
    ResourceManager.GetResourceByPath("Assets/save.png", out var s) ? s! : Stream.Null);

var save = new ImageButton(icon, "Save", 120, 36)
    .WithImageSize(16, 16)
    .WithOnClick(() => SaveDocument())
    .WithLocation(20, 20)
    .WithAddToRenderer(renderer);
```

!!! tip "Resource loading"
    `ImageButton` does not load images for you. Use
    [`ResourceManager`](../resource-manager.md) to load from disk or
    embedded resources, then hand the `SKImage` to `WithImage`.