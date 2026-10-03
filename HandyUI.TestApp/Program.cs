using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using SkiaSharp;

var windowSize2 = new SKSize(760, 760);
(var window2, var renderer2) = SilkWindowHelper.CreateWindowAndGetRenderer("HandyUI Example App 2", windowSize2, windowSize2);

var windowSize = new SKSize(500, 500);
(var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("HandyUI Example App 1", windowSize, windowSize, useDirtyRendering: false);

window.Parent = window2;

if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
    window.SetIcon(SKImage.FromEncodedData(imageStream!));

var textLabel = new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15);

renderer2.AddRootControl(textLabel);

var textButton = new TextButton()
    .WithText("Hello World")
    .WithWidth(100)
    .WithHeight(36)
    .WithOnClick(() => { window.IsMovable = !window.IsMovable; window.IsResizable = !window.IsResizable; })
    .WithLocation(15, 50);

renderer2.AddRootControl(textButton);

_ = window.RunAsync();
window2.Run();