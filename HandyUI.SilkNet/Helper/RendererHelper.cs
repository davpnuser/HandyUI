using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;

namespace HandyUI.SilkNet.Helper;

public static class RendererHelper
{
    public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("The HandyUI adapter for Silk.NET is currently not available on your platform. You can go to the HandyUI repository to help contribute in a fix.");

        ArgumentNullException.ThrowIfNull(window);

        return window.Invoke(() =>
        {
            window.UseDirtyRendering = useDirtyRendering;
            var adapter = new RendererAdapter(window);
            window.RendererAdapter = adapter;
            ThemeHelper.RegisterWindow(window);
            var renderer = adapter.InitializeAndGetRenderer();
            window.Renderer = renderer;
            return renderer;
        });
    }
}