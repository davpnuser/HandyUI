using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

public class RegressionTestingControl : BaseControl<RegressionTestingControl>
{
    public override bool DoesIntersect(SKPoint clientPoint) => true;

    public override void OnDraw(SKCanvas canvas)
    {
    }

    public override void OnUpdate(UpdateContext updateContext) => Console.WriteLine(updateContext.DeltaTime.ToString("F50"));
}
