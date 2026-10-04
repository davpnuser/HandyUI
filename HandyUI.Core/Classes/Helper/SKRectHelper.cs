using SkiaSharp;

namespace HandyUI.Core.Classes.Helper;

public static class SKRectHelper
{
    #region Convert to SKRect

    public static SKRect ToSKRect(this SKPoint point)
    {
        return new SKRect(0, 0, point.X, point.Y);

    }
    public static SKRect ToSKRect(this SKSize size)
    {
        return new SKRect(0, 0, size.Width, size.Height);
    }

    #endregion

    #region Convert from SKRect

    public static SKPoint ToSKPoint(this SKRect rect)
    {
        return new SKPoint(rect.Right, rect.Bottom);
    }

    public static SKSize ToSKSize(this SKRect rect)
    {
        return new SKSize(rect.Width, rect.Height);
    }

    #endregion
}