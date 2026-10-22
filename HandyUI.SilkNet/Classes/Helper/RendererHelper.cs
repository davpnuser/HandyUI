using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Tools;
using HandyUI.SilkNet.Components;

namespace HandyUI.SilkNet.Classes.Helper;

public static class RendererHelper
{
    public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true)
    {
        PlatformTool.EnsureSupportedPlatform();
        ArgumentNullException.ThrowIfNull(window);

        return window.Invoke(() =>
        {
            var adapter = new RendererAdapter(window.InternalWindow!, useDirtyRendering);
            window.RendererAdapter = adapter;
            return adapter.InitializeAndGetRenderer();
        });
    }
}