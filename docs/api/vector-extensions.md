# `VectorExtensions`

`HandyUI.SilkNet.Extensions.VectorExtensions`

Conversions between SkiaSharp geometry and Silk.NET's `Vector2D<int>`.

```csharp
public static class VectorExtensions
{
    public static Vector2D<int> ToVector2D(this SKPoint? point);   // null → (0, 0)
    public static Vector2D<int> ToVector2D(this SKPoint  point);
    public static SKPoint       ToSKPoint (this Vector2D<int> vector);

    public static Vector2D<int> ToVector2D(this SKSize? size);     // null → (0, 0)
    public static Vector2D<int> ToVector2D(this SKSize  size);
    public static SKSize        ToSKSize (this Vector2D<int> vector);
}
```

`Vector2D<int>` truncates fractional coordinates - keep `SKPoint`/`SKSize`
for sub-pixel work and convert only at the Silk.NET boundary.