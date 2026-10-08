using HandyUI.SilkNet.Classes.Helper;
using HandyUI.TestApp;
using SkiaSharp;

var (window, renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window", new SKSize(800, 600), transparentFrameBuffer: true);

renderer.BackgroundColor = SKColors.Transparent;

new RegressionTestingControl()
    .WithAddToRenderer(renderer);

await window.RunAsync();