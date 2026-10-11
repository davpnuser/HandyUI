using HandyUI.Core.Classes.Controls;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Structs;

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);
window.IsMinimizable = false;

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

await window.RunAsync();