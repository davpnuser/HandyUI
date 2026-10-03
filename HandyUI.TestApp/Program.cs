using HandyUI.SilkNet.Classes.Helper;
using HandyUI.TestApp;
using SkiaSharp;

var (a, rendererA) = SilkWindowHelper.CreateWindowAndGetRenderer("Window A", new SKSize(800, 600));
var (b, rendererB) = SilkWindowHelper.CreateWindowAndGetRenderer("Window B", new SKSize(640, 480), invalidateOnMove: true);

rendererA.BackgroundColor = SKColors.White;
rendererB.BackgroundColor = SKColors.White;

b.Parent = a;

var fps1 = new FpsLabelControl()
    .WithLocation(15, 15);

rendererA.AddRootControl(fps1);

var fps2 = new FpsLabelControl()
    .WithLocation(15, 15);

rendererB.AddRootControl(fps2);

await Task.WhenAll(a.RunAsync(), b.RunAsync());