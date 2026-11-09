using SkiaSharp;

namespace HandyUI.Core.Helpers.Extensions;

public static class SKSizeExtensions
{
    #region Convert to SKSize

    public static SKSize ToSKSize(this SKRect rect)
        => new(rect.Right, rect.Bottom);
    public static SKSize ToSKSize(this SKPoint point)
        => new(point.X, point.Y);

    #endregion

    #region Convert from SKSize

    public static SKRect ToSKRect(this SKSize size)
        => new(0, 0, size.Width, size.Height);

    public static SKPoint ToSKPoint(this SKSize size)
        => new(size.Width, size.Height);

    #endregion
}