# `MultilineTextInput`

`HandyUI.Controls.Controls.MultilineTextInput`

Multi-line text editor with blinking cursor, drag selection, and word-wise
navigation.

```csharp
public class MultilineTextInput : BaseControl<MultilineTextInput>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto) |
| `Text` | `string` | `""` |
| `Placeholder` | `string` | `""` |
| `FontSize` | `float` | `14f` |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` |
| `LineSpacing` | `float` | `4f` |
| `CornerRadius` | `float` | `4f` |
| `Padding` | `float` | `12f` |

## Events

```csharp
public event Action<string>? WhenTextChanged;
public event Action? WhenFocusGained;
public event Action? WhenFocusLost;
```

## Keyboard

| Key | Behavior |
| --- | --- |
| Printable character (`CharInput`) | Insert at cursor (replaces selection). |
| Enter | Newline. |
| Backspace | Delete previous char; Ctrl = previous word. |
| Delete | Delete next char; Ctrl = next word. |
| ← / → | Move cursor; Shift extends. |
| ↑ / ↓ | Move between lines; Shift extends. |
| Home / End | Line start / end; Ctrl = document start / end. |
| Ctrl+A | Select all. |

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithPlaceholder, WithFontSize,
WithTypeface, WithLineSpacing, WithCornerRadius, WithPadding,
WithWhenTextChanged, WithWhenFocusGained, WithWhenFocusLost
```

## Example

```csharp
new MultilineTextInput()
    .WithWidth(400)
    .WithHeight(200)
    .WithPlaceholder("Write your notes here…")
    .WithLineSpacing(6)
    .WithWhenTextChanged(t => Console.WriteLine($"{t.Length} chars"))
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);
```