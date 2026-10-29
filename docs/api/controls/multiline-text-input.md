# `MultilineTextInput`

`HandyUI.Controls.Controls.MultilineTextInput`

A multi-line text editor with a blinking cursor, mouse-drag selection,
word-wise navigation (Ctrl), and multi-line copy/cut/paste support.

```csharp
public class MultilineTextInput : BaseControl<MultilineTextInput>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size to content (widest line and number of lines).

### `Text` / `Placeholder`

```csharp
public string Text        { get; set; }   // default: string.Empty
public string Placeholder { get; set; }   // default: string.Empty
```

Newlines are preserved (this is the multi-line editor). Setting `Text`
clamps the cursor and selection anchor to the new length.

### `FontSize` / `Typeface`

```csharp
public float      FontSize { get; set; }   // default: 14f
public SKTypeface Typeface { get; set; }   // default: FontManager.DefaultFont
```

### `LineSpacing`

```csharp
public float LineSpacing { get; set; }   // default: 4f
```

### `CornerRadius` / `Padding`

```csharp
public float CornerRadius { get; set; }   // default: 4f
public float Padding      { get; set; }   // default: 12f
```

## Events

```csharp
public event Action<string>? WhenTextChanged;
public event Action? WhenFocusGained;
public event Action? WhenFocusLost;
```

`WhenTextChanged` fires on every mutation - typing, paste, delete, etc.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Handles click-to-position-cursor, drag selection, focus requests.

### `OnKey(KeyEventContext)` - override hook

```csharp
protected override bool OnKey(KeyEventContext ctx);
```

Full keyboard support:

| Key | Behavior |
| --- | --- |
| Printable character | Insert at cursor (replaces selection). |
| Enter | Insert newline. |
| Backspace | Delete previous char/selection; Ctrl+Backspace deletes previous word. |
| Delete | Delete next char/selection; Ctrl+Delete deletes next word. |
| ←/→ | Move cursor; Shift extends selection. |
| ↑/↓ | Move cursor between lines; Shift extends selection. |
| Home/End | Start/end of line; Ctrl = start/end of document. |
| Ctrl+A | Select all. |

## Fluent API

```csharp
public MultilineTextInput WithWidth(float width);
public MultilineTextInput WithHeight(float height);
public MultilineTextInput WithText(string text);
public MultilineTextInput WithPlaceholder(string placeholder);
public MultilineTextInput WithFontSize(float size);
public MultilineTextInput WithTypeface(SKTypeface? typeface);
public MultilineTextInput WithLineSpacing(float spacing);
public MultilineTextInput WithCornerRadius(float radius);
public MultilineTextInput WithPadding(float padding);
public MultilineTextInput OnTextChanged(Action<string> handler);
public MultilineTextInput OnFocusGained(Action handler);
public MultilineTextInput OnFocusLost(Action handler);
```

## Example

```csharp
var notes = new MultilineTextInput()
    .WithWidth(400)
    .WithHeight(200)
    .WithPlaceholder("Write your notes here…")
    .WithLineSpacing(6)
    .OnTextChanged(t => Console.WriteLine($"{t.Length} chars"))
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);
```