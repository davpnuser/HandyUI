using HandyUI.Controls.Controls;
using HandyUI.Controls.Enums;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

var a = new Picture()
    .WithStretchMode(StretchMode.Uniform)
    .WithHeight(250)
    .WithWidth(250)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

renderer.UseProfiler = true;

ClipboardManager.Instance.ClipboardUpdated += (type) =>
{
    if (type == ClipboardItemType.Image)
    {
        a.Image = ClipboardManager.Instance.ClipboardContentsImage;
    }
};

await window.RunAsync();