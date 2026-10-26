# `ScrollFrame`

`HandyUI.Controls.Controls.ScrollFrame`

A scrolling viewport. Children scroll inside `Bounds` and a scrollbar appears
on the right when content is taller than the visible area. The scrollbar
fades out shortly after the last scroll event.

```csharp
public class ScrollFrame : BaseControl<ScrollFrame>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }   // default: 200f
public float Height { get; set; }   // default: 200f
```

### `ScrollSpeed`

```csharp
public float ScrollSpeed { get; set; }   // default: 40f
```

Pixels per wheel notch.

### `ScrollbarWidth` / `ScrollbarMinThumbHeight`

```csharp
public float ScrollbarWidth          { get; set; }   // default: 4f
public float ScrollbarMinThumbHeight { get; set; }   // default: 24f
```

Thumb height is `max(minHeight, trackHeight * viewport / content)`.

## Events

```csharp
public event Action<float>? WhenScrollChanged;
```

Fires with the current scroll offset (in pixels) after each scroll step.

## Methods

### `OnChildAdded(IControl)` / `OnChildRemoved(IControl)` - override hooks

```csharp
protected override void OnChildAdded(IControl control);
protected override void OnChildRemoved(IControl control);
```

Both recompute the total content height.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Handles wheel scrolling when hovered.

## Fluent API

```csharp
public ScrollFrame WithWidth(float width);
public ScrollFrame WithHeight(float height);
public ScrollFrame WithScrollSpeed(float speed);
public ScrollFrame WithScrollbarWidth(float width);
public ScrollFrame OnScrollChanged(Action<float> handler);
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

!!! info "Layout is manual"
    Unlike `ListBox`, `ScrollFrame` does **not** re-lay out children. Set
    each child's `Location` yourself; `ScrollFrame` just translates them by
    the current scroll offset.