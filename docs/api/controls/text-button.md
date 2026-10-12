# `TextButton`

`HandyUI.Core.Classes.Controls.TextButton`

A clickable button that renders a text label. Supports hover and pressed
states, auto-sizing, custom fonts, and configurable borders.

```csharp
public class TextButton : UIControlBase<TextButton>
```

## Constructor

```csharp
public TextButton()   // default text: "Button"
```

## Properties

### `Text`

```csharp
public string Text { get; set; }   // default: "Button"
```

Changing the text re-runs `RecalculateBounds` when `AutoSize` is `true`.

### `AutoSize`

```csharp
public bool AutoSize { get; set; }   // default: false
```

When `true`, `Bounds` grows to fit the text plus padding.

### `Padding`

```csharp
public SKPoint Padding { get; set; }   // default: (12, 6)
```

Inner padding used when `AutoSize` is `true` and also as visual margin.

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

### Colors

```csharp
public SKColor NormalBackgroundColor  { get; set; }
public SKColor HoverBackgroundColor   { get; set; }
public SKColor PressedBackgroundColor { get; set; }
public SKColor NormalBorderColor      { get; set; }
public SKColor ActiveBorderColor      { get; set; }
public SKColor TextColor              { get; set; }
public SKColor LightTextColor         { get; set; }
```

`LightTextColor` is used when the button is pressed (so the label stays
readable on the darker `PressedBackgroundColor`).

### `BorderThickness` / `BorderDirection`

```csharp
public float           BorderThickness { get; set; }   // default: 1f
public BorderDirection BorderDirection { get; set; }   // default: Inside
```

### `Theme`

```csharp
public ThemeRecord Theme { get; set; }
```

## Events

```csharp
public event Action? Clicked;
```

Raised when the button is clicked (mouse released while hovered and
enabled).

## Methods

### `RecalculateBounds()`

```csharp
public bool RecalculateBounds()
```

Recomputes `Bounds` from the text measurement and padding when
`AutoSize == true`. Returns `true` if the bounds changed.

### `Draw(SKCanvas)`

```csharp
public override void Draw(SKCanvas canvas)
```

Draws the background, then the border, then the centered text.

## Fluent API

```csharp
public TextButton WithTheme(ThemeRecord theme);
public TextButton WithWidth(float width);
public TextButton WithHeight(float height);
public TextButton WithSize(float width, float height);
public TextButton WithAutoSize(bool autoSize);
public TextButton WithPadding(float x, float y);
public TextButton WithText(string text);
public TextButton WithTextSize(float size);
public TextButton WithTextColor(SKColor color);
public TextButton WithLightTextColor(SKColor color);
public TextButton WithTypeface(SKTypeface? typeface);
public TextButton WithFont(string family, float size = 13f,
    SKFontStyleWeight weight = SKFontStyleWeight.Normal,
    SKFontStyleSlant  slant  = SKFontStyleSlant.Upright);
public TextButton WithFontWeight(SKFontStyleWeight weight);
public TextButton WithFontSlant(SKFontStyleSlant slant);
public TextButton WithBorder(SKColor normalColor, SKColor activeColor,
    float thickness = 1.0f,
    BorderDirection direction = BorderDirection.Inside);
public TextButton WithBorderThickness(float thickness);
public TextButton WithBorderDirection(BorderDirection direction);
public TextButton WithOnClick(Action onClick);
public TextButton WithColors(SKColor normal, SKColor hover, SKColor pressed);
```

## Example

```csharp
var ok = new TextButton()
    .WithText("Save")
    .WithAutoSize(true)
    .WithFont("Segoe UI", 14, SKFontStyleWeight.SemiBold)
    .WithColors(
        normal:  SKColor.Parse("#2563EB"),
        hover:   SKColor.Parse("#3B82F6"),
        pressed: SKColor.Parse("#1D4ED8"))
    .WithLightTextColor(SKColors.White)
    .WithOnClick(() => Save())
    .WithLocation(24, 80)
    .WithAddToRenderer(renderer);
```

!!! tip "Auto-sizing with padding"
    `AutoSize` measures the cap height plus `Padding.Y * 2` for the height,
    and the text width plus `Padding.X * 2` for the width - so a button
    always clears its own label with a comfortable margin.