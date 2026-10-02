using HandyUI.SilkNet.Classes.Extensions;
using HandyUI.TestApp;
using Silk.NET.Windowing;

SilkWindowHelper.InitializeMultiWindowSupport();

var windowAmount = 3;

for (var i = 0; i < windowAmount; i++)
{
    new Thread(() =>
    {
        (var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer($"Example {i}", new(250, 150), WindowBorder.Fixed, useDirtyRendering: false);

        var ft = new FpsLabelControl()
        {
            Location = new(15, 15)
        };

        renderer.AddRootControl(ft);
    }).Start();
}

SilkWindowHelper.RunApplicationLoop();