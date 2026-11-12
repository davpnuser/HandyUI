# `ItemBox`

`HandyUI.Controls.Controls.ItemBox`

Selectable list item with sibling-exclusive selection.

```csharp
public class ItemBox : BaseControl<ItemBox>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto) |
| `Text` | `string` | `""` (multi-line) |
| `FontSize` | `float` | `14f` |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` |
| `IsSelected` | `bool` | `false` |
| `HorizontalPadding` / `VerticalPadding` | `float` | `16f` / `8f` |

`IsSelected = true` animates an accent background and 3-pixel accent bar,
and clears `IsSelected` on any sibling `ItemBox` under the same parent.

## Events

```csharp
public event Action<ItemBox>? WhenClicked;
public event Action<bool>? WhenSelectionChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

`OnMouse` toggles selection on `MouseDownEvent` (not `ClickEvent`) so
selection happens immediately on press.

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithFontSize, WithTypeface,
WithIsSelected, WithPadding,
WithWhenClicked(Action<ItemBox>), WithWhenSelectionChanged(Action<bool>),
WithWhenHoverEntered, WithWhenHoverExited
```

## Example

```csharp
var list = new Frame().WithSize(240, 200).WithAddToRenderer(renderer);

foreach (var name in new[] { "Inbox", "Sent", "Drafts" })
    new ItemBox().WithText(name).WithWidth(240).WithParent(list);
```