using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

//new Label()
//    .WithText("Hello, World!")
//    .WithFontSize(24)
//    .WithLocation(15, 15)
//    .WithAddToRenderer(renderer);

renderer.UseProfiler = true;

await window.RunAsync();