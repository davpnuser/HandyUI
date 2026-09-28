using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Extensions;
using Silk.NET.Windowing;
using SkiaSharp;

(var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example", new(500, 500), WindowBorder.Fixed);

if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
    SKImage.FromEncodedData(imageStream!).SetAsIcon(window);

var count = 0;
var bcount = 0;
var tbcount = 0;

var cb = new CheckBox();
cb
.WithOnCheckChanged(_ =>
{
    if (count++ == 4)
    {
        cb.IsEnabled = false;
    }
})
.WithLocation(15, 15);

renderer.AddRootControl(cb);

var b = new TextButton();
b
.WithOnClick(() =>
{
    if (bcount++ == 3)
    {
        b.IsEnabled = false;
    }
})
.WithSize(200, 36)
.WithLocation(15, 55);

renderer.AddRootControl(b);

var tb = new TextBox();
tb
.WithOnSubmit(text =>
{
    if (tbcount++ == 3)
    {
        tb.IsEnabled = false;
    }
})
.WithSize(200, 36)
.WithLocation(15, 95);

renderer.AddRootControl(tb);

window.Run();