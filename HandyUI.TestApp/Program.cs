using HandyUI.Core.Components;
using HandyUI.Core.Helper;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using HandyUI.TestApp.Controls;
using SkiaSharp;

var config = WindowConfiguration.Default with
{
    Title = "Control Showcase",
    Size = new(1050, 1000),
    //Icon = ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var iconStream)
    //    ? SKImage.FromEncodedData(iconStream)
    //    : null
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var logoStream))
    logoImage = SKImage.FromEncodedData(logoStream);

var bc = new BigControl()
    .WithBounds(new SKSize(800, 600))
    .WithZIndex(55)
    .WithAddToRenderer(renderer);

window.OnResize += (newSize) =>
{
    bc.Bounds = newSize.ToSKRect();
};

await window.RunAsync();