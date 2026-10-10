using HandyUI.Core.Classes.Controls;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Structs;

var (window1, renderer1) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default with { Title = "Window 1" });
var (window2, renderer2) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default with { Title = "Window 2" });

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