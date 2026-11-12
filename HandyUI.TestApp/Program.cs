using HandyUI.Core.Helpers;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
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

//ThemeManager.Instance.SetTheme(50, 50, 50, ThemeMode.Dark);

renderer.UseProfiler = true;

await window.RunAsync();