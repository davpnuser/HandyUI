using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;

namespace HandyUI.SilkNet.Classes.Helper;

public static class SilkRendererHelper
{
    public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("The HandyUI adapter for Silk.NET is currently not available on your platform. You can go to the HandyUI repository to help contribute in a fix.");

        ArgumentNullException.ThrowIfNull(window);

        return window.Invoke(() =>
        {
            var adapter = new SilkWindowRendererAdapter(window.InternalWindow!, useDirtyRendering);
            window.RendererAdapter = adapter;
            return adapter.InitializeAndGetRenderer();
        });
    }
}