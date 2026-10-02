using HandyUI.SilkNet.Classes.Extensions;
using HandyUI.TestApp;
using Silk.NET.Windowing;

(var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer($"Example 1", new(250, 150), WindowBorder.Fixed, useDirtyRendering: true);

var ft = new FpsLabelControl()
{
    Location = new(15, 15)
};

renderer.AddRootControl(ft);

window.Run();