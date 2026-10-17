using HandyUI.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Records;
using HandyUI.TestApp.Controls;
using SkiaSharp;

var config = WindowConfiguration.Default with
{
    Title = "Main Window",
    Icon =
        ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var iconStream)
        ? SKImage.FromEncodedData(iconStream)
        : null,
    IsCloseable = false
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);

new TextBox()
    .WithPlaceholder("Hello, World!")
    .WithTextSize(24)
    .WithSize(250, 36)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

new RegressionTestingControl()
    .WithAddToRenderer(renderer);

await window.RunAsync();