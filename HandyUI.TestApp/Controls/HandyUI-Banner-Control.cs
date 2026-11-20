using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data.Input;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

internal class HandyUI_Banner_Control : BaseControl<HandyUI_Banner_Control>
{
    private readonly SKImage? _bannerImage;
    private readonly SKColor _color = new(35, 35, 35);
    private readonly SKColor _hoverColor = new(50, 50, 50);
    private readonly SKColor _strokeColor = new(25, 25, 25);
    private readonly SKColor _textColor = new(225, 225, 225);
    private readonly SKColor _downColor = new(25, 25, 25);

    private readonly SKPaint _paint;
    private readonly SKPaint _strokePaint;
    private readonly SKPaint _textPaint;

    private readonly SKTextBlob? _textBlob;

    private readonly SKSamplingOptions _sampling;
    private readonly SKFont _font;

    private readonly SKRect _imageBounds = new(0f, 40f, 287.5f, 147f);

    internal HandyUI_Banner_Control()
    {
        _bannerImage = ResourceManager.GetResourceByPath("Resources/HandyUI-Banner.png", out var bannerStream) ? SKImage.FromEncodedData(bannerStream) : null;
        Bounds = new SKRect(0f, 0f, 287.5f, 147f); // 107f originally
        _sampling = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest);
        _font = new SKFont(FontManager.DefaultFont, 26);
        _textBlob = SKTextBlob.Create("Made with HandyUI", _font);

        _paint = new() { Color = _color, IsAntialias = true, Style = SKPaintStyle.Fill };
        _strokePaint = new() { Color = _strokeColor, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
        _textPaint = new() { Color = _textColor, IsAntialias = true, Style = SKPaintStyle.Fill };
    }

    public override bool DoesIntersect(SKPoint point)
    {
        return Bounds.Contains(point);
    }

    public override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRoundRect(Bounds, 15, 15, _paint);
        canvas.DrawRoundRect(Bounds, 15, 15, _strokePaint);

        canvas.DrawText(_textBlob, 9, 33, _textPaint);
        canvas.DrawImage(_bannerImage, _imageBounds, _sampling);
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        var changesMade = false;

        if (IsHovered && !IsMouseDown && _paint.Color != _hoverColor)
        {
            _paint.Color = _hoverColor;
            _strokePaint.Color = _strokeColor;
            changesMade = true;
        }
        else if (IsHovered && IsMouseDown && _paint.Color != _strokeColor)
        {
            _paint.Color = _strokeColor;
            _strokePaint.Color = _hoverColor;
            changesMade = true;
        }
        else if (!IsHovered && !IsMouseDown && _paint.Color != _color)
        {
            _paint.Color = _color;
            _strokePaint.Color = _strokeColor;
            changesMade = true;
        }

        if (changesMade)
        {
            Invalidate();
            return true;
        }

        return false;
    }

    protected override void OnDispose()
    {
        _bannerImage?.Dispose();
        _paint.Dispose();
        _strokePaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        _textBlob?.Dispose();
    }
}