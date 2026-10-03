using Silk.NET.Maths;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Extensions;

public static class VectorExtensions
{
    // SKPoint
    public static Vector2D<int> ToVector2D(this SKPoint? point)
    {
        return point is not null ? new Vector2D<int>((int)point.Value.X, (int)point.Value.Y) : new Vector2D<int>(0, 0);
    }

    public static Vector2D<int> ToVector2D(this SKPoint point)
    {
        return new Vector2D<int>((int)point.X, (int)point.Y);
    }

    public static SKPoint ToSKPoint(this Vector2D<int> vector)
    {
        return new SKPoint(vector.X, vector.Y);
    }

    // SKSize
    public static Vector2D<int> ToVector2D(this SKSize? size)
    {
        return size is not null ? new Vector2D<int>((int)size.Value.Width, (int)size.Value.Height) : new Vector2D<int>(0, 0);
    }

    public static Vector2D<int> ToVector2D(this SKSize size)
    {
        return new Vector2D<int>((int)size.Width, (int)size.Height);
    }

    public static SKSize ToSKSize(this Vector2D<int> vector)
    {
        return new SKSize(vector.X, vector.Y);
    }
}