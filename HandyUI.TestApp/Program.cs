using HandyUI.Controls;
using HandyUI.Core.Classes.Helper;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Records;
using SkiaSharp;

var (window1, renderer1) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default with { Title = "Window 1", Icon = ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var imgStream) ? SKImage.FromEncodedData(imgStream) : null });
var (window2, renderer2) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default with { Title = "Window 2" });

new Frame()
    .WithSize(150, 150)
    .WithBackgroundColor(SKColorHelper.FromHue(75))
    .WithAddToRenderer(renderer1);

new Frame()
    .WithSize(150, 150)
    .WithBackgroundColor(SKColorHelper.FromHue(50))
    .WithAddToRenderer(renderer2);

await Task.WhenAll(window1.RunAsync(), window2.RunAsync());