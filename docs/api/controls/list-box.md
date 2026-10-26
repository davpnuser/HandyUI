# `ListBox`

`HandyUI.Controls.Controls.ListBox`

A vertically scrolling container. Children are laid out top-to-bottom
automatically and reflow when children are added or removed. `ListBox` itself
does not draw anything - it just manages layout and scrolling for its
children.

```csharp
public class ListBox : BaseControl<ListBox>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }   // default: 200f
public float Height { get; set; }   // default: 200f
```

### `ItemSpacing`

```csharp
public float ItemSpacing { get; set; }   // default: 0
```

Vertical gap between children.

### `ScrollSpeed`

```csharp
public float ScrollSpeed { get; set; }   // default: 40f
```

Pixels per wheel notch.

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<float>? WhenScrollChanged;
```

`WhenScrollChanged` receives the current scroll offset (in pixels).

## Methods

### `OnChildAdded(IControl)` / `OnChildRemoved(IControl)` - override hooks

```csharp
protected override void OnChildAdded(IControl control);
protected override void OnChildRemoved(IControl control);
```

Both call `RecalculateLayout()`.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Handles wheel scrolling and raises `WhenClicked`.

## Fluent API

```csharp
public ListBox WithWidth(float width);
public ListBox WithHeight(float height);
public ListBox WithItemSpacing(float spacing);
public ListBox WithScrollSpeed(float speed);
public ListBox OnClicked(Action handler);
public ListBox OnHoverEntered(Action handler);
public ListBox OnHoverExited(Action handler);
public ListBox OnScrollChanged(Action<float> handler);
```

## Example

```csharp
var list = new ListBox()
    .WithWidth(240)
    .WithHeight(320)
    .WithItemSpacing(2)
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);

foreach (var name in Directory.GetFiles(".").Take(100))
{
    new ItemBox().WithText(name).WithWidth(240).WithParent(list);
}
```

!!! warning "Children are re-laid out"
    `ListBox` overrides each child's `Location` in `RecalculateLayout()`. If
    you set a child's `Location` manually, it will be overwritten on the next
    layout pass.