# `TextInput`

`HandyUI.Controls.Controls.TextInput`

A single-line text editor. Line breaks and tabs are sanitized to spaces on
assignment.

```csharp
public class TextInput : BaseControl<TextInput>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | **Sanitized**: `\r\n`/`\r`/`\n` → space, `\t` → 4 spaces. |
| `Placeholder` | `string` | `""` | |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | |
| `CornerRadius` | `float` | `4f` | |
| `HorizontalPadding` / `VerticalPadding` | `float` | `12f` / `8f` | |

## Events

```csharp
public event Action<string>? WhenTextChanged;
```

Fires on every mutation (after sanitization for programmatic assignment).

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse`, `OnKey` - standard
overrides.

## Fluent API

```csharp
public TextInput WithWidth(float width);
public TextInput WithHeight(float height);
public TextInput WithText(string text);
public TextInput WithPlaceholder(string placeholder);
public TextInput WithFontSize(float size);
public TextInput WithTypeface(SKTypeface? typeface);
public TextInput WithCornerRadius(float radius);
public TextInput WithPadding(float horizontal, float vertical);

public TextInput WithWhenTextChanged(Action<string> handler);
```

## Example

```csharp
var search = new TextInput()
    .WithWidth(260)
    .WithPlaceholder("Search…")
    .WithCornerRadius(8)
    .WithWhenTextChanged(q => Results.Filter(q))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```