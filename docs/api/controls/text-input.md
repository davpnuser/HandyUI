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

## Clipboard

`TextInput` supports cut / copy / paste through the host clipboard via
[`ClipboardManager`](../clipboard-manager.md).

### `Copy()`

```csharp
public void Copy();
```

Copies the current selection to `ClipboardManager.Instance.SetClipboard(...)`.
No-op when there's no selection.

### `Cut()`

```csharp
public void Cut();
```

Copies the selection, then deletes it. Raises `WhenTextChanged`.

### `Paste()`

```csharp
public void Paste();
```

Reads `ClipboardManager.Instance.ClipboardContentsText`, **sanitizes it to a
single line** (line breaks → spaces, tabs → four spaces), and inserts it at
the cursor. If there's a selection, it's replaced. No-op when the clipboard
is empty or contains only whitespace after sanitization.

!!! info "Single-line enforcement"
    Unlike [`MultilineTextInput`](multiline-text-input.md), `TextInput`
    sanitizes multi-line clipboard text on paste. A `"foo\nbar"` clipboard
    paste becomes `"foo bar"`.

### Keyboard shortcuts

| Shortcut | Action |
| --- | --- |
| **Ctrl+C** / **Ctrl+Insert** | `Copy()` |
| **Ctrl+X** / **Shift+Delete** | `Cut()` |
| **Ctrl+V** / **Shift+Insert** | `Paste()` |

!!! info "Requires `ClipboardHelper`"
    Clipboard methods only work when the host has initialized the OS
    bridge - see
    [`ClipboardHelper.EnsureInitialized()`](../silknet-helpers.md#clipboardhelper)
    (Silk.NET) or your own host equivalent.

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