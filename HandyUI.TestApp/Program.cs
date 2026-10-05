using HandyUI.Core.Classes.Controls;
using HandyUI.SilkNet.Classes.Helper;
using SkiaSharp;

(var window1, var renderer1) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window 1", new SKSize(800, 600));
(var window2, var renderer2) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window 2", new SKSize(800, 600));

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer1);

new TextLabel()
    .WithText("Hello World, Once Again!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer2);

await Task.WhenAll(window1.RunAsync(), window2.RunAsync());