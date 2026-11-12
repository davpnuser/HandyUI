# `ListBox`

`HandyUI.Controls.Controls.ListBox`

Vertically scrolling container that auto-lays out its children top to bottom.

```csharp
public class ListBox : BaseControl<ListBox>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `200f` |
| `ItemSpacing` | `float` | `0` |
| `ScrollSpeed` | `float` | `40f` (px per wheel notch) |

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<float>? WhenScrollChanged;
```

## Behavior

`OnChildAdded` / `OnChildRemoved` trigger `RecalculateLayout()`, which sets
every child's `Location` to `(0, runningY)`. Manual child locations are
overwritten on the next layout pass.

`OnMouse` handles `ScrollEvent` (wheel) and raises `WhenClicked`.

## Fluent API

```csharp
WithWidth, WithHeight, WithItemSpacing, WithScrollSpeed,
WithWhenClicked, WithWhenHoverEntered, WithWhenHoverExited,
WithWhenScrollChanged
```

## Example

```csharp
var list = new ListBox()
    .WithWidth(240)
    .WithHeight(320)
    .WithItemSpacing(2)
    .WithAddToRenderer(renderer);

foreach (var name in Directory.GetFiles(".").Take(100))
    new ItemBox().WithText(name).WithWidth(240).WithParent(list);
```