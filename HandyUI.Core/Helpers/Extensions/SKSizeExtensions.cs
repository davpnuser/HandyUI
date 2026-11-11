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
}