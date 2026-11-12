# `TextInput`

`HandyUI.Controls.Controls.TextInput`

Single-line text editor. Line breaks and tabs are sanitized to spaces on
assignment.

```csharp
public class TextInput : BaseControl<TextInput>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto) |
| `Text` | `string` | `""` - sanitized: `\r\n`/`\r`/`\n` → space, `\t` → 4 spaces |
| `Placeholder` | `string` | `""` |
| `FontSize` | `float` | `14f` |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` |
| `CornerRadius` | `float` | `4f` |
| `HorizontalPadding` / `VerticalPadding` | `float` | `12f` / `8f` |

## Events

```csharp
public event Action<string>? WhenTextChanged;
```

## Keyboard

| Key | Behavior |
| --- | --- |
| Printable character | Insert at cursor. |
| Backspace | Delete previous (Ctrl = word). |
| Delete | Delete next (Ctrl = word). |
| ← / → | Move cursor (Shift extends). |
| Home / End | Jump to start / end. |
| Ctrl+A | Select all. |

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithPlaceholder, WithFontSize,
WithTypeface, WithCornerRadius, WithPadding, WithWhenTextChanged
```

## Example

```csharp
new TextInput()
    .WithWidth(260)
    .WithPlaceholder("Search…")
    .WithCornerRadius(8)
    .WithWhenTextChanged(q => Results.Filter(q))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```