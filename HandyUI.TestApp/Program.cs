using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Classes.Helper;
using HandyUI.SilkNet.Classes.Helper;
using SkiaSharp;

(var window1, var renderer1) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window 1", new SKSize(800, 600), transparentFrameBuffer: true);

renderer1.BackgroundColor = SKColors.White.WithTransparency(0.75f);

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer1);

await window1.RunAsync();