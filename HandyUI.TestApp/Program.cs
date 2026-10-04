using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Components;
using HandyUI.TestApp;
using SkiaSharp;

static void GetWindowGoing(HandyWindow window, UIRenderer renderer)
{
    var label = new TextLabel()
        .WithText(window.Title)
        .WithTextSize(24)
        .WithLocation(15, 15)
        .WithAddToRenderer(renderer);

    var testcontrol = new TestControl()
        .WithLocation(15, 560)
        .WithAddToRenderer(renderer);

    testcontrol.Location = new SKPoint(15, 480 - testcontrol.Bounds.Height - 15);

    var fpscontrol = new FpsLabelControl()
        .WithLocation(15, 0)
        .WithAddToRenderer(renderer);

    fpscontrol.Location = new SKPoint(15, 480 - fpscontrol.Bounds.Height - testcontrol.Bounds.Height - 30);

    window.OnResize += (newSize) =>
    {
        testcontrol.Location = new SKPoint(15, newSize.Height - testcontrol.Bounds.Height - 15);
        fpscontrol.Location = new SKPoint(15, newSize.Height - fpscontrol.Bounds.Height - testcontrol.Bounds.Height - 30);
    };
}

var (window1, renderer1) = SilkWindowHelper.CreateWindowAndGetRenderer("Window 1", new(800, 600), new(0, 100), useDirtyRendering: false);
//var (window2, renderer2) = SilkWindowHelper.CreateWindowAndGetRenderer("Window 2", new(600, 480), useDirtyRendering: false, modalParentWindow: window1);

GetWindowGoing(window1, renderer1);
//GetWindowGoing(window2, renderer2);

//_ = window2.RunAsync();
//window1.Run();

//await Task.WhenAll(window1.RunAsync(), window2.RunAsync());
await window1.RunAsync();
//window1.Run();