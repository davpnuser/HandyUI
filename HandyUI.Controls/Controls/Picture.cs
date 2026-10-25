using HandyUI.Controls.Enums;
using HandyUI.Core.Base;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class Picture : BaseControl<Picture>
{
    #region Properties

    public float Width
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public float Height
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public SKImage? Image
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    }

    public StretchMode StretchMode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = StretchMode.Uniform;

    #endregion

    #region Internal Fields

    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKSamplingOptions _sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    #endregion

    #region Overrides

    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return clientPoint is { } p && Bounds.Contains(p);
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition) { }

    public override void OnDraw(SKCanvas canvas)
    {
        if (Image is null) return;
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var srcW = Image.Width;
        var srcH = Image.Height;
        if (srcW <= 0 || srcH <= 0) return;

        var destRect = StretchMode switch
        {
            StretchMode.None => new SKRect(0, 0, srcW, srcH),
            StretchMode.Fill => new SKRect(0, 0, w, h),
            StretchMode.Uniform => CalculateUniform(srcW, srcH, w, h),
            StretchMode.UniformToFill => CalculateUniformToFill(srcW, srcH, w, h),
            _ => new SKRect(0, 0, w, h)
        };

        if (StretchMode == StretchMode.UniformToFill)
        {
            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, w, h));
            canvas.DrawImage(Image, destRect, _sampling, _paint);
            canvas.Restore();
            return;
        }

        canvas.DrawImage(Image, destRect, _sampling, _paint);
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var contentW = Image?.Width ?? 0f;
        var contentH = Image?.Height ?? 0f;
        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    private static SKRect CalculateUniform(float srcW, float srcH, float dstW, float dstH)
    {
        var scale = Math.Min(dstW / srcW, dstH / srcH);
        var w = srcW * scale;
        var h = srcH * scale;
        return new SKRect((dstW - w) * 0.5f, (dstH - h) * 0.5f, (dstW + w) * 0.5f, (dstH + h) * 0.5f);
    }

    private static SKRect CalculateUniformToFill(float srcW, float srcH, float dstW, float dstH)
    {
        var scale = Math.Max(dstW / srcW, dstH / srcH);
        var w = srcW * scale;
        var h = srcH * scale;
        return new SKRect((dstW - w) * 0.5f, (dstH - h) * 0.5f, (dstW + w) * 0.5f, (dstH + h) * 0.5f);
    }

    #endregion

    #region Fluent APIs

    public Picture WithWidth(float width) { Width = width; return Self; }
    public Picture WithHeight(float height) { Height = height; return Self; }
    public Picture WithImage(SKImage? image) { Image = image; return Self; }
    public Picture WithStretchMode(StretchMode mode) { StretchMode = mode; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _paint.Dispose();
    }

    #endregion
}