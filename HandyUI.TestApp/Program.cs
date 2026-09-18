using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Extensions;
using Silk.NET.Windowing;
using SkiaSharp;

(var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example", new(500, 500), WindowBorder.Fixed);

if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
    SKImage.FromEncodedData(imageStream!).SetAsIcon(window);

var button = new TextButton()
    .WithText("Click me!")
    .WithWidth(100)
    .WithHeight(36)
    .WithLocation(15, 15);

renderer.AddRootControl(button);

window.Run();