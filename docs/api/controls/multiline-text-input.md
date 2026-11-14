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

## Clipboard

`MultilineTextInput` supports cut / copy / paste through the host clipboard
via [`ClipboardManager`](../clipboard-manager.md).

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

Reads `ClipboardManager.Instance.ClipboardContentsText`, **normalizes line
endings** (`\r\n` / `\r` → `\n` so the internal line math stays correct), and
inserts it at the cursor. If there's a selection, it's replaced. No-op when
the clipboard is empty.

!!! tip "Multi-line preserved"
    Unlike [`TextInput`](text-input.md), `MultilineTextInput` preserves
    newlines when pasting. A `"foo\nbar"` clipboard paste stays two lines.

### Keyboard shortcuts

| Shortcut | Action |
| --- | --- |
| **Ctrl+C** / **Ctrl+Insert** | `Copy()` |
| **Ctrl+X** / **Shift+Delete** | `Cut()` |
| **Ctrl+V** / **Shift+Insert** | `Paste()` |

!!! info "Requires `ClipboardHelper`"
    Clipboard methods only work when the host has initialized the OS
    bridge - see
    [`ClipboardHelper.EnsureInitialized()`](../silknet-helpers.md#clipboardhelper).

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