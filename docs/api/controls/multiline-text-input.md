# `MultilineTextInput`

`HandyUI.Controls.Controls.MultilineTextInput`

A multi-line text editor with cursor blink, drag selection, and word-wise
navigation.

```csharp
public class MultilineTextInput : BaseControl<MultilineTextInput>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Newlines preserved. Clamps cursor + selection. |
| `Placeholder` | `string` | `""` | Shown when empty. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | |
| `LineSpacing` | `float` | `4f` | Extra pixels between lines. |
| `CornerRadius` | `float` | `4f` | |
| `Padding` | `float` | `12f` | All-sides inner padding. |

## Events

```csharp
public event Action<string>? WhenTextChanged;
public event Action? WhenFocusGained;
public event Action? WhenFocusLost;
```

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse`, `OnKey` - standard
overrides. `OnKey` supports printable chars, Enter, Backspace/Delete
(Ctrl = word), arrows (Shift = extend), Home/End (Ctrl = start/end of
document), and Ctrl+A.

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

public MultilineTextInput WithWhenTextChanged(Action<string> handler);
public MultilineTextInput WithWhenFocusGained(Action handler);
public MultilineTextInput WithWhenFocusLost(Action handler);
```

## Example

```csharp
var notes = new MultilineTextInput()
    .WithWidth(400)
    .WithHeight(200)
    .WithPlaceholder("Write your notes here…")
    .WithLineSpacing(6)
    .WithWhenTextChanged(t => Console.WriteLine($"{t.Length} chars"))
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```