# `BorderDirection`

`HandyUI.Core.Enums.BorderDirection`

Specifies where a stroke is drawn relative to the shape it outlines.

```csharp
public enum BorderDirection
{
    Inside,
    Center,
    Outside
}
```

| Value | Meaning |
| --- | --- |
| `Inside` | Stroke lies entirely within the shape's bounds. |
| `Center` | Stroke straddles the edge. |
| `Outside` | Stroke lies entirely outside the shape. |

Used by [`Frame`](controls/frame.md) (and any custom control that draws
borders).

!!! tip
    For crisp 1-pixel borders, prefer `Inside`. `Center` can blur depending
    on pixel alignment.