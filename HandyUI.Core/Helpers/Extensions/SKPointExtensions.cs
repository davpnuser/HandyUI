using SkiaSharp;

namespace HandyUI.Core.Helpers.Extensions;

public static class SKPointExtensions
{
    #region Convert to SKPoint

    public static SKPoint ToSKPoint(this SKRect rect)
        => new(rect.Right, rect.Bottom);
    public static SKPoint ToSKPoint(this SKSize size)
        => new(size.Width, size.Height);

    #endregion

    #region Convert from SKPoint

    public static SKRect ToSKRect(this SKPoint point)
        => new(0, 0, point.X, point.Y);

    public static SKSize ToSKSize(this SKPoint point)
        => new(point.X, point.Y);

    #endregion
}