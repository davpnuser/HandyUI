# `VectorExtensions`

`HandyUI.SilkNet.Extensions.VectorExtensions`

Conversions between SkiaSharp geometry and Silk.NET's `Vector2D<int>`.

```csharp
public static class VectorExtensions
```

## `SKPoint`

```csharp
public static Vector2D<int> ToVector2D(this SKPoint? point);   // null → (0, 0)
public static Vector2D<int> ToVector2D(this SKPoint  point);
public static SKPoint       ToSKPoint (this Vector2D<int> vector);
```

## `SKSize`

```csharp
public static Vector2D<int> ToVector2D(this SKSize? size);    // null → (0, 0)
public static Vector2D<int> ToVector2D(this SKSize  size);
public static SKSize        ToSKSize (this Vector2D<int> vector);
```

## Example

```csharp
SKPoint p = new(120, 80);
Vector2D<int> v = p.ToVector2D();   // (120, 80)
SKPoint back = v.ToSKPoint();
```

!!! info "Integral precision"
    `Vector2D<int>` truncates fractional coordinates. Keep `SKPoint`/`SKSize`
    for sub-pixel work; convert only at the Silk.NET boundary.