# `ItemBox`

`HandyUI.Controls.Controls.ItemBox`

A selectable list-style item. Selection is **exclusive among siblings** -
selecting one `ItemBox` deselects all other `ItemBox` children of the same
parent.

```csharp
public class ItemBox : BaseControl<ItemBox>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size.

### `Text` / `FontSize`

```csharp
public string Text     { get; set; }   // default: string.Empty
public float  FontSize { get; set; }   // default: 14f
```

### `IsSelected`

```csharp
public bool IsSelected { get; set; }
```

Setting it animates a background highlight and a 3-pixel accent bar on the
left edge. It also auto-deselects any sibling `ItemBox` instances under the
same parent.

### `HorizontalPadding` / `VerticalPadding`

```csharp
public float HorizontalPadding { get; set; }   // default: 16f
public float VerticalPadding   { get; set; }   // default: 8f
```

## Events

```csharp
public event Action<ItemBox>? WhenClicked;
public event Action<bool>? WhenSelectionChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Toggles `IsSelected` on `MouseDown`, deselecting siblings when selecting.

## Fluent API

```csharp
public ItemBox WithWidth(float width);
public ItemBox WithHeight(float height);
public ItemBox WithText(string text);
public ItemBox WithFontSize(float size);
public ItemBox WithIsSelected(bool selected);
public ItemBox WithPadding(float horizontal, float vertical);
public ItemBox OnClicked(Action<ItemBox> handler);
public ItemBox OnSelectionChanged(Action<bool> handler);
public ItemBox OnHoverEntered(Action handler);
public ItemBox OnHoverExited(Action handler);
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

!!! tip "Sibling selection is automatic"
    You don't need to track which item is selected - `ItemBox` deselects
    siblings itself.