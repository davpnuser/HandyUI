using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Helper;
using SkiaSharp;

namespace HandyUI.TestApp;

public class HueShiftingControl : UIControlBase
{
    private float _hue = 0f;
    private readonly SKPaint _paint = new() { Color = SKColors.Red, Style = SKPaintStyle.Fill };

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(Bounds, _paint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        _hue += deltaTime * 36;
        _hue %= 360;
        _paint.Color = SKColorHelper.FromHue(_hue);
    }

    protected override void OnDispose()
    {
        _hue = 0f;
        _paint.Dispose();
    }
}