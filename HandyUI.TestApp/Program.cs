using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Classes.Helper;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.TestApp;
using Silk.NET.Windowing;
using SkiaSharp;

var windowSize = new SKSize(500, 300);
var (window, renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window", windowSize, transparentFrameBuffer: true, windowCentered: true, windowBorder: WindowBorder.Fixed, visibleInTaskbar: false);
renderer.BackgroundColor = SKColors.White.WithTransparency(0.8f);

if (ResourceManager.GetResourceByPath("Resources/splash.png", out var splashStream))
{
    new ImageLabel()
        .WithImage(SKImage.FromEncodedData(splashStream))
        .WithSize(500, 300)
        .WithAutoSize(false)
        .WithAddToRenderer(renderer);
}

new RegressionTestingControl()
    .WithAddToRenderer(renderer);

await window.RunAsync();