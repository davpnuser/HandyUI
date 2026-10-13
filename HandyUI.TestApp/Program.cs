using HandyUI.Controls;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Structs;
using HandyUI.TestApp.Controls;
using SkiaSharp;

var config = WindowConfiguration.Default with
{
    Title = "idek why it is getting flagged atp",
    TransparentFrameBuffer = true,
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);

renderer.BackgroundColor = SKColors.Transparent;

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(26)
    .WithTextColor(SKColors.White)
    .WithLocation(13, 13)
    .WithZIndex(1)
    .WithAddToRenderer(renderer);

new RegressionTestingControl()
    .WithAddToRenderer(renderer);

await window.RunAsync();