using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Classes.Themes;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Extensions;
using Silk.NET.Windowing;
using SkiaSharp;

(var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example", new(500, 500), WindowBorder.Fixed);

if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
    SKImage.FromEncodedData(imageStream!).SetAsIcon(window);

var scroll = new ScrollingFrame()
    .WithAutoCanvasSize(true)
    .WithWidth(500)
    .WithHeight(500);

renderer.AddRootControl(scroll);

for (var i = 0; i < 15; i++)
{
    var button = new TextButton()
        .WithParent(scroll)
        .WithText("exit app")
        .WithBorder(VS2017Theme.ButtonPressed, VS2017Theme.ButtonHover, 2)
        .WithTextSize(16)
        .WithWidth(150)
        .WithHeight(36)
        .WithLocation(25 + (40 * i), 40 * i);
}

var x = new TextButton()
        .WithParent(scroll)
        .WithOnClick(() => Environment.Exit(0))
        .WithText("exit app fr")
        .WithBorder(VS2017Theme.ButtonPressed, VS2017Theme.ButtonHover, 2)
        .WithTextSize(16)
        .WithWidth(150)
        .WithHeight(36)
        .WithLocation(625, 40 * 15);

window.Run();