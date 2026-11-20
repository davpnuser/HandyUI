using HandyUI.Controls.Controls;
using HandyUI.Controls.Enums;
using HandyUI.Core.Data;
using HandyUI.Core.Helpers;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using HandyUI.TestApp.Controls;

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

var a = new HandyUI_Banner_Control()
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

var b = new Picture()
    .WithStretchMode(StretchMode.Uniform)
    .WithLocation(15, 160)
    .WithHeight(250)
    .WithWidth(250)
    .WithAddToRenderer(renderer);

renderer.UseProfiler = true;

ClipboardManager.Instance.ClipboardUpdated += (type) =>
{
    if (type == ClipboardItemType.Image)
    {
        b.Image = ClipboardManager.Instance.ClipboardContentsImage;
    }
};

await window.RunAsync();