using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Structs;
using HandyUI.TestApp;

var (window, renderer) = SilkWindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

new RegressionTestingControl()
    .WithAddToRenderer(renderer);

await window.RunAsync();