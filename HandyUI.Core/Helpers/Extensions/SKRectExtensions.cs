using SkiaSharp;

namespace HandyUI.Core.Helpers.Extensions;

public static class SKRectExtensions
{
    #region Convert to SKRect

    public static SKRect ToSKRect(this SKPoint point)
        => new(0, 0, point.X, point.Y);
    public static SKRect ToSKRect(this SKSize size)
        => new(0, 0, size.Width, size.Height);

    #endregion

    #region Division functions

    #region Halfs

    public static SKRect TopHalf(this SKRect rect)
        => new(rect.Left, rect.Top, rect.Right, rect.MidY);

    public static SKRect BottomHalf(this SKRect rect)
        => new(rect.Left, rect.MidY, rect.Right, rect.Bottom);

    public static SKRect LeftHalf(this SKRect rect)
        => new(rect.Left, rect.Top, rect.MidX, rect.Bottom);

    public static SKRect RightHalf(this SKRect rect)
        => new(rect.MidX, rect.Top, rect.Right, rect.Bottom);

    #endregion

    #region Quarters

    public static SKRect TopLeft(this SKRect rect)
        => new(rect.Left, rect.Top, rect.MidX, rect.MidY);

    public static SKRect TopRight(this SKRect rect)
        => new(rect.MidX, rect.Top, rect.Right, rect.MidY);

    public static SKRect BottomLeft(this SKRect rect)
        => new(rect.Left, rect.MidY, rect.MidX, rect.Bottom);

    public static SKRect BottomRight(this SKRect rect)
        => new(rect.MidX, rect.MidY, rect.Right, rect.Bottom);

    #endregion

    #region Horizontal

    public static SKRect[] DivideHorizontal(this SKRect rect, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

        var results = new SKRect[count];
        var sliceHeight = rect.Height / count;

        for (var i = 0; i < count; i++)
        {
            var top = rect.Top + (i * sliceHeight);
            var bottom = (i == count - 1) ? rect.Bottom : top + sliceHeight;
            results[i] = new SKRect(rect.Left, top, rect.Right, bottom);
        }

        return results;
    }

    public static SKRect DivideHorizontal(this SKRect rect, int count, int index)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));

        var sliceHeight = rect.Height / count;
        var top = rect.Top + (index * sliceHeight);
        var bottom = (index == count - 1) ? rect.Bottom : top + sliceHeight;

        return new SKRect(rect.Left, top, rect.Right, bottom);
    }

    #endregion

    #region Vertical

    public static SKRect[] DivideVertical(this SKRect rect, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

        var results = new SKRect[count];
        var sliceWidth = rect.Width / count;

        for (var i = 0; i < count; i++)
        {
            var left = rect.Left + (i * sliceWidth);
            var right = (i == count - 1) ? rect.Right : left + sliceWidth;
            results[i] = new SKRect(left, rect.Top, right, rect.Bottom);
        }

        return results;
    }

    public static SKRect DivideVertical(this SKRect rect, int count, int index)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));

        var sliceWidth = rect.Width / count;
        var left = rect.Left + (index * sliceWidth);
        var right = (index == count - 1) ? rect.Right : left + sliceWidth;

        return new SKRect(left, rect.Top, right, rect.Bottom);
    }

    #endregion

    #endregion
}