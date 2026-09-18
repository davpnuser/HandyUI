using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TestControl : UIControlBase
{
    private readonly SKPaint _paint;
    private readonly SKPaint _paint2;
    private readonly SKTypeface _typeface = SKTypeface.Default;
    private readonly SKFont _font;
    private int num = 0;

    public TestControl()
    {
        _paint = new SKPaint() { Color = new(0, 0, 0) };
        _paint2 = new SKPaint() { Color = new(255, 255, 255) };
        _font = new SKFont(_typeface);
    }

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(13, 0, 76, 16, _paint2);
        canvas.DrawText($"{num}", 15, 15, SKTextAlign.Left, _font, _paint);
    }

    public override bool Intersects(SKPoint clientPoint)
        => true;

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        num = Random.Shared.Next(100000, 999999);
    }

    protected override void OnDispose()
    {
        _paint.Dispose();
        _paint2.Dispose();
        _typeface.Dispose();
        _font.Dispose();
    }
}
