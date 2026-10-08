using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.TestApp;

public class RegressionTestingControl : UIControlBase<RegressionTestingControl>
{
    public override bool Intersects(SKPoint clientPoint)
    {
        return false;
    }

    public override void Draw(SKCanvas canvas)
    {
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        Console.WriteLine(deltaTime.ToString("F50"));
    }
}
