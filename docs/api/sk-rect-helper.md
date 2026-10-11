# `SKRectHelper`

`HandyUI.Core.Classes.Helper.SKRectHelper`

Conversions between SkiaSharp geometry types.

```csharp
public static class SKRectHelper
```

## To `SKRect`

```csharp
public static SKRect ToSKRect(this SKPoint point);   // (0, 0, X, Y)
public static SKRect ToSKRect(this SKSize size);     // (0, 0, W, H)
```

## From `SKRect`

```csharp
public static SKPoint ToSKPoint(this SKRect rect);   // (Right, Bottom)
public static SKSize  ToSKSize (this SKRect rect);   // (Width, Height)
```

## Example

```csharp
var size = new SKSize(200, 100);
var rect = size.ToSKRect();   // SKRect(0, 0, 200, 100)
var back = rect.ToSKSize();   // SKSize(200, 100)
```

!!! tip
    Handy in fluent APIs, e.g. `WithBounds(new SKSize(200, 100))` converts to
    an `SKRect` at the origin.