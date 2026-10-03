using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Classes.Helper;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.TestApp;
using SkiaSharp;

var (window1, renderer1) = SilkWindowHelper.CreateWindowAndGetRenderer("Window 1", new SKSize(800, 600));
var (window2, renderer2) = SilkWindowHelper.CreateWindowAndGetRenderer("Window 2", new SKSize(640, 480), useDirtyRendering: false);

window2.Parent = window1;

var label = new TextLabel()
    .WithText("Example HandyUI app")
    .WithTextSize(24)
    .WithTextColor(SKColors.Black)
    .WithLocation(15, 15);

renderer1.AddRootControl(label);

var rainbow = new HueShiftingControl()
    .WithBounds(0, 0, 640, 480);

window2.OnResize += (newSize) =>
{
    rainbow.Bounds = newSize.ToSKRect();
};

renderer2.AddRootControl(rainbow);

_ = window1.RunAsync();
window2.Run();

//await Task.WhenAll(window1.RunAsync(), window2.RunAsync());