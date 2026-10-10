using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Structs;
using HandyUI.SilkNet.Classes.Tools;
using HandyUI.SilkNet.Components;
using Silk.NET.Windowing;

namespace HandyUI.SilkNet.Classes.Helper;

public static class WindowHelper
{
    #region Window and Renderer Creation Functions

    public static UIRenderer CreateRenderer(HandyWindow window, bool useDirtyRendering)
    {
        PlatformTool.EnsureSupportedPlatform();

        var renderer = RendererHelper.Attach(window, useDirtyRendering);

        return renderer;
    }

    public static (HandyWindow window, UIRenderer renderer) CreateWindowAndGetRenderer(WindowConfiguration windowConfiguration)
    {
        PlatformTool.EnsureSupportedPlatform();

        var window = new HandyWindow(windowConfiguration);
        var renderer = CreateRenderer(window, windowConfiguration.UseDirtyRendering);

        return (window, renderer);
    }

    #endregion

    #region Other Functions

    public static void TryCancelClose(IWindow window)
    {
        try { window.IsClosing = false; }
        catch { /* do not crash */ }

        if (window.Native?.Glfw is nint rawHandle && rawHandle != nint.Zero)
        {
            try
            {
                unsafe
                {
                    var windowPtr = (Silk.NET.GLFW.WindowHandle*)rawHandle;
                    Silk.NET.GLFW.Glfw.GetApi().SetWindowShouldClose(windowPtr, false);
                }
            }
            catch { /* do not crash */ }
        }
    }

    #endregion
}