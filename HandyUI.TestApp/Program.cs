using HandyUI.Core.Helpers;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using HandyUI.TestApp.Controls;
using SkiaSharp;

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(
    WindowConfiguration.Default with
    {
        Title = "Profiler Demo",
        UseDirtyRendering = false,
        Icon = ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var iconStream)
        ? SKImage.FromEncodedData(iconStream)
        : null
    });

new MouseDisplayControl()
    .WithAddToRenderer(renderer);

renderer.UseProfiler = true;

await window.RunAsync();