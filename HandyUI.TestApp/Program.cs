using HandyUI.Core.Classes.Controls;
using HandyUI.SilkNet.Classes.Extensions;
using Silk.NET.Windowing;

var thread1 = new Thread(() =>
{
    (var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example 1", new(500, 500), WindowBorder.Fixed);

    var tbcount = 0;

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
});

var thread2 = new Thread(() =>
{
    (var window, var renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example 2", new(250, 250), WindowBorder.Fixed);

    var bcount = 0;

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

    window.Run();
});

thread1.Start();
thread2.Start();

thread1.Join();
thread2.Join();