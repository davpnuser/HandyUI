# `TextBox`

`HandyUI.Controls.TextBox`

A single-line editable text field with a blinking caret, placeholder text,
and keyboard navigation. Handles character input, backspace, delete, arrow
keys, home/end, and submit.

```csharp
public class TextBox : BaseControl<TextBox>
```

## Constructor

```csharp
public TextBox(
    float  width       = 200f,
    float  height      = 36f,
    string text        = "",
    string placeholder = "Type here...")
```

## Properties

### `Text`

```csharp
public string Text { get; set; }   // default: string.Empty
```

The current content. Setting this raises both `TextChanged` and
`OnTextChanged`, and clamps the caret index to the new length.

### `PlaceholderText`

```csharp
public string PlaceholderText { get; set; }   // "Type here..."
```

Shown when `Text` is empty **and** the control is not focused.

### `CornerRadius`

```csharp
public float CornerRadius { get; set; }   // default: 6f
```

### `PaddingX`

```csharp
public float PaddingX { get; set; }   // default: 10f
```

Horizontal inner padding for the text and caret.

### `TextSize`

```csharp
public float TextSize { get; set; }   // default: 13f
```

### Font properties

```csharp
public string            FontFamily { get; set; }   // "Segoe UI"
public SKFontStyleWeight FontWeight { get; set; }   // Normal
public SKFontStyleWidth  FontWidth  { get; set; }   // Normal
public SKFontStyleSlant  FontSlant  { get; set; }   // Upright
```

### Colors

```csharp
public SKColor BackgroundColor   { get; set; }
public SKColor BorderColor       { get; set; }
public SKColor FocusBorderColor  { get; set; }
public SKColor TextColor         { get; set; }
public SKColor PlaceholderColor  { get; set; }
public SKColor CaretColor        { get; set; }
```

Each color is seeded from `Theme` but can be overridden individually.

### `Theme`

```csharp
public Theme Theme { get; set; }
```

## Events

```csharp
public event Action<string>? TextChanged;
public event Action<string>? Submitted;

public Action<string>? OnTextChanged { get; set; }
public Action<string>? OnSubmit      { get; set; }
```

- `TextChanged` / `OnTextChanged` fire whenever `Text` changes (typing,
  paste, programmatic assignment).
- `Submitted` / `OnSubmit` fire when the user presses **Enter**.

!!! warning "Both channels fire"
    The `event` and the `Action` property are invoked back-to-back. Subscribe
    to only one per handler, or your callback runs twice.

## Methods

### `Intersects(SKPoint)`

```csharp
public override bool Intersects(SKPoint clientPoint)
```

### `Update(float, SKPoint)`

```csharp
public override void Update(float deltaTime, SKPoint clientMousePosition)
```

Advances the caret blink timer while focused (500 ms on / off).

### `Draw(SKCanvas)`

```csharp
public override void Draw(SKCanvas canvas)
```

Draws the rounded background, border (focused vs. unfocused), the text or
placeholder, and the caret when focused and blinking on.

## Input behavior

### Mouse

- Click inside a focused/unfocused `TextBox` sets focus and moves the caret
  to the nearest character boundary.
- Clicking outside a focused `TextBox` clears focus.

### Keyboard (only when focused)

| Key | Action |
| --- | --- |
| Printable character (`CharInput`) | Insert at the caret. |
| Backspace (`8`) | Delete the character before the caret. |
| Delete (`46`) | Delete the character at the caret. |
| Left (`37`) / Right (`39`) | Move the caret one character. |
| Home (`36`) / End (`35`) | Jump to start/end. |
| Enter (`13`) | Raise `Submitted` / `OnSubmit`. |

## Fluent API

```csharp
public TextBox WithTheme(Theme theme);
public TextBox WithWidth(float width);
public TextBox WithHeight(float height);
public TextBox WithSize(float width, float height);
public TextBox WithText(string text);
public TextBox WithPlaceholder(string placeholder);
public TextBox WithCornerRadius(float radius);
public TextBox WithPaddingX(float paddingX);
public TextBox WithTextSize(float size);
public TextBox WithFont(string family, float size = 13f,
    SKFontStyleWeight weight = SKFontStyleWeight.Normal,
    SKFontStyleSlant  slant  = SKFontStyleSlant.Upright);
public TextBox WithColors(SKColor bg, SKColor border, SKColor focusBorder,
                          SKColor text, SKColor placeholder, SKColor caret);
public TextBox WithOnTextChanged(Action<string> onTextChanged);
public TextBox WithOnSubmit(Action<string> onSubmit);
```

## Example

```csharp
var search = new TextBox(width: 260, height: 34, placeholder: "Search…")
    .WithCornerRadius(8)
    .WithOnTextChanged(q => Results.Filter(q))
    .WithOnSubmit(q => Results.Execute(q))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Focus"
    `TextBox` requests focus automatically when clicked. To focus it
    programmatically, call `search.RequestFocus()`.

!!! info "Caret is drawn locally"
    The caret is a single vertical stroke drawn inside the clipped content
    rect. Horizontal scrolling for long text is not implemented - the text is
    clipped rather than scrolled.