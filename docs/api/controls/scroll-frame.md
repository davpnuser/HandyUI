# `ScrollFrame`

`HandyUI.Controls.Controls.ScrollFrame`

A scrolling viewport with a fade-out scrollbar. Does **not** re-lay out
children - you position them yourself; the frame translates by the current
scroll offset.

```csharp
public class ScrollFrame : BaseControl<ScrollFrame>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `200f` | |
| `ScrollSpeed` | `float` | `40f` | Pixels per wheel notch. |
| `ScrollbarWidth` | `float` | `4f` | |
| `ScrollbarMinThumbHeight` | `float` | `24f` | Minimum thumb height. |

## Events

```csharp
public event Action<float>? WhenScrollChanged;
```

## Methods

`OnChildAdded` / `OnChildRemoved` recompute content height. `OnMouse`
handles wheel scrolling.

## Fluent API

```csharp
public ScrollFrame WithWidth(float width);
public ScrollFrame WithHeight(float height);
public ScrollFrame WithScrollSpeed(float speed);
public ScrollFrame WithScrollbarWidth(float width);

public ScrollFrame WithWhenScrollChanged(Action<float> handler);
```

## Example

```csharp
var scroller = new ScrollFrame()
    .WithWidth(320)
    .WithHeight(240)
    .WithScrollSpeed(60)
    .WithLocation(40, 40)
    .WithAddToRenderer(renderer);

for (var i = 0; i < 20; i++)
{
    new Label()
        .WithText($"Row {i + 1}")
        .WithWidth(320)
        .WithLocation(0, i * 32)
        .WithParent(scroller);
}
```