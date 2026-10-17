using HandyUI.Controls;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Structs;
using HandyUI.TestApp.Controls;

var config = WindowConfiguration.Default with { Title = "idek why it is getting flagged atp" };
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