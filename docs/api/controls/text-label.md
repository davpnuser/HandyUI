# `TextLabel`

`HandyUI.Controls.TextLabel`

A non-interactive, single-line text label. `Bounds` is recalculated to fit
the text whenever the font or content changes.

```csharp
public class TextLabel : UIControlBase<TextLabel>
```

## Constructor

```csharp
public TextLabel(string text = "")
```

## Properties

### `Text`

```csharp
public string Text { get; set; }   // default: string.Empty
```

Setting this triggers `RecalculateBounds`.

### `TextSize`

```csharp
public float TextSize { get; set; }   // default: 16f
```

### `TextColor`

```csharp
public SKColor TextColor { get; set; }
```

Seeded from `Theme.TextColor`. Assigning it marks the channel as
customized so a later `Theme` swap won't overwrite it.

### Font properties

```csharp
public SKTypeface?       Typeface   { get; set; }
public string            FontFamily { get; set; }   // "Segoe UI"
public SKFontStyleWeight FontWeight { get; set; }   // Normal
public SKFontStyleWidth  FontWidth  { get; set; }   // Normal
public SKFontStyleSlant  FontSlant  { get; set; }   // Upright
```

Any of these being changed invalidates the cached typeface and re-runs
`RecalculateBounds`.

### `Theme`

```csharp
public Theme Theme { get; set; }
```

### `Width` / `Height` / `Size`

```csharp
public float  Width  { get; set; }
public float  Height { get; set; }
public SKSize Size   { get; set; }
```

Explicit overrides - but note `RecalculateBounds` will overwrite them the
next time the text or font changes.

## Methods

### `RecalculateBounds()`

```csharp
public bool RecalculateBounds()
```

Fits `Bounds` to the text's measured width and the font's line height
(`Descent - Ascent`). Returns `true` if the bounds changed.

### `Draw(SKCanvas)`

```csharp
public override void Draw(SKCanvas canvas)
```

Draws the text left-aligned at `Bounds.Left`, vertically centered in
`Bounds`.

## Fluent API

```csharp
public TextLabel WithTheme(Theme theme);
public TextLabel WithWidth(float width);
public TextLabel WithHeight(float height);
public TextLabel WithSize(float width, float height);
public TextLabel WithText(string text);
public TextLabel WithTextColor(SKColor color);
public TextLabel WithTextSize(float size);
public TextLabel WithTypeface(SKTypeface? typeface);
public TextLabel WithFont(string family, float size = 16f,
    SKFontStyleWeight weight = SKFontStyleWeight.Normal,
    SKFontStyleSlant  slant  = SKFontStyleSlant.Upright);
```

## Example

```csharp
var title = new TextLabel("Settings")
    .WithFont("Segoe UI", 22, SKFontStyleWeight.SemiBold)
    .WithTextColor(SKColor.Parse("#0F172A"))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Auto-sizing vs. explicit size"
    `TextLabel` prefers to size itself to its content. If you need a fixed
    box (e.g. for right-aligned text), set `Width` **after** setting the
    text and avoid changing `Text` again - otherwise `RecalculateBounds`
    will overwrite your width.