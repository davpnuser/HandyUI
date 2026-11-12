using HandyUI.Controls.Controls;
using HandyUI.Core.Helpers;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using SkiaSharp;

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(
    WindowConfiguration.Default with
    {
        Title = "Demo",
        Icon = ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var iconStream)
        ? SKImage.FromEncodedData(iconStream)
        : null
    });

new Label()
    .WithText("Hello, World!")
    .WithFontSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

new Button()
    .WithText("Hello, World!")
    .WithLocation(15, 45)
    .WithAddToRenderer(renderer);

new Button()
    .WithText("Hello, World!")
    .WithLocation(15, 75)
    .WithAddToRenderer(renderer);

//a.Focusable = true;
//b.Focusable = true;

renderer.UseProfiler = true;

await window.RunAsync();