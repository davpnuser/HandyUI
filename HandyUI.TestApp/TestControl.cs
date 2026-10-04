using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.TestApp;

public class TestControl : UIControlBase
{
    private readonly SKPaint _backgroundPaint;
    private readonly SKPaint _textPaint;
    private readonly SKFont _font;

    private int _num = 0;
    private string _formattedText = "0";

    private const float PaddingX = 12f;
    private const float PaddingY = 8f;

    public TestControl()
    {
        _backgroundPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };

        _font = new SKFont(SKTypeface.Default, 14f);

        RecalculateBounds();
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        _num = Random.Shared.Next(100000, 999999);
        _formattedText = _num.ToString();

        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        var textWidth = _font.MeasureText(_formattedText);

        _font.GetFontMetrics(out var metrics);
        var textHeight = metrics.Descent - metrics.Ascent;

        var width = textWidth + (PaddingX * 2);
        var height = textHeight + (PaddingY * 2);

        Bounds = new SKRect(0, 0, width, height);
    }

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(Bounds, _backgroundPaint);
        _font.GetFontMetrics(out var metrics);

        var textX = PaddingX;
        var textY = PaddingY - metrics.Ascent;

        canvas.DrawText(_formattedText, textX, textY, SKTextAlign.Left, _font, _textPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint.X, clientPoint.Y);
    }

    protected override void OnDispose()
    {
        _backgroundPaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
    }
}