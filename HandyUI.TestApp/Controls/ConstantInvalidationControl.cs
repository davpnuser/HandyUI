using HandyUI.Core.Base;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

public class ConstantInvalidationControl : BaseControl<ConstantInvalidationControl>
{
    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return true;
    }

    public override void OnDraw(SKCanvas canvas)
    {
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition)
    {
        Invalidate();
    }
}