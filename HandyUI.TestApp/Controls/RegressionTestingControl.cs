using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

public class RegressionTestingControl : BaseControl<RegressionTestingControl>
{
    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return false;
    }

    public override void OnDraw(SKCanvas canvas)
    {
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition)
    {
        Console.WriteLine(deltaTime.ToString("F50"));
    }
}
