# `SKRectHelper`

`HandyUI.Core.Helper.SKRectHelper`

Conversions and division helpers for SkiaSharp geometry.

```csharp
public static class SKRectHelper
```

## Conversions

```csharp
public static SKRect ToSKRect(this SKPoint point);   // (0, 0, X, Y)
public static SKRect ToSKRect(this SKSize size);     // (0, 0, W, H)

public static SKPoint ToSKPoint(this SKRect rect);   // (Right, Bottom)
public static SKSize  ToSKSize (this SKRect rect);   // (Width, Height)
```

## Halves

```csharp
public static SKRect TopHalf(this SKRect rect);
public static SKRect BottomHalf(this SKRect rect);
public static SKRect LeftHalf(this SKRect rect);
public static SKRect RightHalf(this SKRect rect);
```

## Quarters

```csharp
public static SKRect TopLeft(this SKRect rect);
public static SKRect TopRight(this SKRect rect);
public static SKRect BottomLeft(this SKRect rect);
public static SKRect BottomRight(this SKRect rect);
```

## Slicing

```csharp
public static SKRect[] DivideHorizontal(this SKRect rect, int count);
public static SKRect   DivideHorizontal(this SKRect rect, int count, int index);

public static SKRect[] DivideVertical(this SKRect rect, int count);
public static SKRect   DivideVertical(this SKRect rect, int count, int index);
```

`DivideHorizontal` splits into equal-height horizontal strips (top to
bottom). `DivideVertical` splits into equal-width vertical strips (left to
right). The last slice is snapped to the original edge to avoid cumulative
floating-point drift.

```csharp
var grid = new SKRect(0, 0, 300, 100);

var columns = grid.DivideVertical(3);           // three 100-wide cells
var secondRow = grid.DivideHorizontal(4, 1);    // the second of four strips

var centerLeft = grid.DivideVertical(4, 2);
```

!!! tip "Last-slice snapping"
    The three-argument overloads clamp out-of-range `index` values with
    `ArgumentOutOfRangeException`, and both variants align the final slice
    exactly with `rect.Right` / `rect.Bottom`.