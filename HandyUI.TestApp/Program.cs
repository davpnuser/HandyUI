//using HandyUI.Controls;
using HandyUI.Core.Components;
using HandyUI.Core.Helper;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using HandyUI.TestApp.Controls;
using SkiaSharp;

var config = WindowConfiguration.Default with
{
    Title = "Main Window",
    Icon =
        ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var iconStream)
        ? SKImage.FromEncodedData(iconStream)
        : null,
    //TransparentFrameBuffer = true,
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);
//renderer.BackgroundColor = SKColors.Transparent;

var bc = new BigControl()
    .WithBounds(new SKSize(800, 600))
    .WithAddToRenderer(renderer);

window.OnResize += (newSize) =>
{
    bc.Bounds = newSize.ToSKRect();
};

new RegressionTestingControl()
    .WithAddToRenderer(renderer);

await window.RunAsync();