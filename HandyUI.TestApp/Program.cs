using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Extensions;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Diagnostics;

(var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example", new(250, 350));

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
{
    logoImage = SKImage.FromEncodedData(imageStream!);
}
logoImage!.SetAsIcon(window);

var button = new TextButton()
    .WithText("exit app")
    .WithOnClick(Debugger.Break)
    .WithWidth(150)
    .WithHeight(36)
    .WithLocation(25, 25);

renderer.AddRootControl(button);

window.Run();