# `ItemBox`

`HandyUI.Controls.Controls.ItemBox`

A selectable list-style item. Selection is **exclusive among siblings**.

```csharp
public class ItemBox : BaseControl<ItemBox>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Multi-line. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | |
| `IsSelected` | `bool` | `false` | Animates a highlight + 3-pixel accent bar. Auto-deselects siblings. |
| `HorizontalPadding` / `VerticalPadding` | `float` | `16f` / `8f` | |

## Events

```csharp
public event Action<ItemBox>? WhenClicked;
public event Action<bool>? WhenSelectionChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public ItemBox WithWidth(float width);
public ItemBox WithHeight(float height);
public ItemBox WithText(string text);
public ItemBox WithFontSize(float size);
public ItemBox WithTypeface(SKTypeface? typeface);
public ItemBox WithIsSelected(bool selected);
public ItemBox WithPadding(float horizontal, float vertical);

public ItemBox WithWhenClicked(Action<ItemBox> handler);
public ItemBox WithWhenSelectionChanged(Action<bool> handler);
public ItemBox WithWhenHoverEntered(Action handler);
public ItemBox WithWhenHoverExited(Action handler);
```

## Example

```csharp
var list = new Frame().WithSize(240, 200).WithAddToRenderer(renderer);

foreach (var name in new[] { "Inbox", "Sent", "Drafts" })
{
    new ItemBox()
        .WithText(name)
        .WithWidth(240)
        .WithParent(list);
}
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```