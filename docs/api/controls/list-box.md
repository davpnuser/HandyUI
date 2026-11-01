# `ListBox`

`HandyUI.Controls.Controls.ListBox`

A vertically scrolling container that auto-lays-out its children top to
bottom.

```csharp
public class ListBox : BaseControl<ListBox>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `200f` | |
| `ItemSpacing` | `float` | `0` | Vertical gap between children. |
| `ScrollSpeed` | `float` | `40f` | Pixels per wheel notch. |

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<float>? WhenScrollChanged;
```

## Methods

`OnChildAdded` / `OnChildRemoved` trigger `RecalculateLayout()`. `OnMouse`
handles wheel scrolling and `WhenClicked`.

## Fluent API

```csharp
public ListBox WithWidth(float width);
public ListBox WithHeight(float height);
public ListBox WithItemSpacing(float spacing);
public ListBox WithScrollSpeed(float speed);

public ListBox WithWhenClicked(Action handler);
public ListBox WithWhenHoverEntered(Action handler);
public ListBox WithWhenHoverExited(Action handler);
public ListBox WithWhenScrollChanged(Action<float> handler);
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