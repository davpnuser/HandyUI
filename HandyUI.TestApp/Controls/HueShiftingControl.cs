using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Helper;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

public class HueShiftingControl : BaseControl
{
    public float HueDelta = 36;
    private float _hue = 0f;
    private readonly SKPaint _paint = new() { Color = SKColors.Red, Style = SKPaintStyle.Fill };

    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return clientPoint != null && Bounds.Contains((SKPoint)clientPoint);
    }

    public override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(Bounds, _paint);
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition)
    {
        _hue += deltaTime * HueDelta;
        _hue %= 360;
        _paint.Color = SKColorHelper.FromHue(_hue);
    }

    public HueShiftingControl WithHuePerSecond(float hueDelta)
    {
        HueDelta = hueDelta;
        return this;
    }

    protected override void OnDispose()
    {
        _hue = 0f;
        _paint.Dispose();
    }
}