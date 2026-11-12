# `ScrollFrame`

`HandyUI.Controls.Controls.ScrollFrame`

Scrolling viewport with a fade-out scrollbar. Does **not** re-layout
children - the frame translates existing children by the current offset.

```csharp
public class ScrollFrame : BaseControl<ScrollFrame>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `200f` |
| `ScrollSpeed` | `float` | `40f` |
| `ScrollbarWidth` | `float` | `4f` |
| `ScrollbarMinThumbHeight` | `float` | `24f` |

## Events

```csharp
public event Action<float>? WhenScrollChanged;
```

## Fluent API

```csharp
WithWidth, WithHeight, WithScrollSpeed, WithScrollbarWidth,
WithWhenScrollChanged
```

## Example

```csharp
var scroller = new ScrollFrame()
    .WithWidth(320).WithHeight(240)
    .WithScrollSpeed(60)
    .WithAddToRenderer(renderer);

for (var i = 0; i < 20; i++)
    new Label()
        .WithText($"Row {i + 1}")
        .WithWidth(320)
        .WithLocation(0, i * 32)
        .WithParent(scroller);
```