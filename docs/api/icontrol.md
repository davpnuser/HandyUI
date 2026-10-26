# `IControl`

`HandyUI.Core.Interface.IControl`

The minimal contract for anything the `UIRenderer` can draw and route input
to.

```csharp
public interface IControl : IDisposable
```

## Properties

| Property | Type | Description |
| --- | --- | --- |
| `Children` | `IReadOnlyList<IControl>` | Child controls. |
| `Parent` | `BaseControl?` | Parent control. |
| `Location` | `SKPoint` | Offset relative to the parent. |
| `Bounds` | `SKRect` | Rectangle in local space. |
| `ZIndex` | `int` | Draw/hit order. |
| `ParentChildPositioningEnabled` | `bool` | Whether `Location` is applied. |
| `ScissoringEnabled` | `bool` | Whether the subtree is clipped to `Bounds`. |
| `AlphaPaint` | `SKPaint` | Pre-built `White * Opacity` paint. |
| `Opacity` | `float` | `[0, 1]`. |
| `IsHovered` | `bool` | Set by the renderer. |
| `IsMouseDown` | `bool` | Set by the renderer. |
| `IsFocused` | `bool` | Managed by the renderer. |
| `IsVisible` | `bool` | Draw/hit gate. |
| `IsEnabled` | `bool` | Interaction gate. |

## Events

| Event | Description |
| --- | --- |
| `WhenFocusRequested` | Raised by `RequestFocus()`. |
| `WhenInvalidated` | Raised when state changes. |

## Methods

| Method | Description |
| --- | --- |
| `Invalidate()` | Notify the renderer to redraw. |
| `RequestFocus()` | Ask the renderer to focus this control. |
| `ProcessMouseEvent(MouseEventContext)` | Route a mouse event. |
| `ProcessKeyEvent(KeyEventContext)` | Route a key event. |
| `DoesIntersect(SKPoint?)` | Hit-test in local space (`null` → clipped). |
| `OnUpdate(float, SKPoint?)` | Per-frame update. |
| `OnDraw(SKCanvas)` | Per-frame draw. |

## Fluent API: `IControl<TSelf>`

```csharp
public interface IControl<TSelf> : IControl where TSelf : IControl<TSelf>
{
    TSelf WithLocation(SKPoint location);
    TSelf WithLocation(float x, float y);
    TSelf WithBounds(SKRect bounds);
    TSelf WithBounds(float x, float y, float w, float h);
    TSelf WithAddToRenderer(UIRenderer renderer);
}
```