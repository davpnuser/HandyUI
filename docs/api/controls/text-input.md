# `TextInput`

`HandyUI.Controls.Controls.TextInput`

A single-line text editor with a blinking cursor, drag selection, and
word-wise navigation.

```csharp
public class TextInput : BaseControl<TextInput>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size based on the current text (or placeholder) plus padding.

### `Text` / `Placeholder`

```csharp
public string Text        { get; set; }   // default: string.Empty
public string Placeholder { get; set; }   // default: string.Empty
```

### `FontSize` / `CornerRadius`

```csharp
public float FontSize     { get; set; }   // default: 14f
public float CornerRadius { get; set; }   // default: 4f
```

### `HorizontalPadding` / `VerticalPadding`

```csharp
public float HorizontalPadding { get; set; }   // default: 12f
public float VerticalPadding   { get; set; }   // default: 8f
```

## Events

### `WhenTextChanged`

```csharp
public event Action<string>? WhenTextChanged;
```

Fires on every text mutation - typing, delete, selection replace.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Handles click-to-position-cursor and drag selection, and requests focus.

### `OnKey(KeyEventContext)` - override hook

```csharp
protected override bool OnKey(KeyEventContext ctx);
```

| Key | Behavior |
| --- | --- |
| Printable character | Insert at cursor. |
| Backspace | Delete previous char (Ctrl = previous word). |
| Delete | Delete next char (Ctrl = next word). |
| ←/→ | Move cursor (Shift = extend selection). |
| Home/End | Jump to start/end. |
| Ctrl+A | Select all. |

## Fluent API

```csharp
public TextInput WithWidth(float width);
public TextInput WithHeight(float height);
public TextInput WithText(string text);
public TextInput WithPlaceholder(string placeholder);
public TextInput WithFontSize(float size);
public TextInput WithCornerRadius(float radius);
public TextInput WithPadding(float horizontal, float vertical);
public TextInput OnTextChanged(Action<string> handler);
```

## Example

```csharp
var search = new TextInput()
    .WithWidth(260)
    .WithPlaceholder("Search…")
    .WithCornerRadius(8)
    .OnTextChanged(q => Results.Filter(q))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Focus"
    `TextInput` requests focus on click. To focus programmatically, call
    `search.RequestFocus()`.