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
}