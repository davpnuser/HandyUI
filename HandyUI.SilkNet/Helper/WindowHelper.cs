using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Tool;
using Silk.NET.GLFW;
using Silk.NET.Windowing;

namespace HandyUI.SilkNet.Helper;

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
                    var windowPtr = (WindowHandle*)rawHandle;
                    var glfw = Glfw.GetApi();
                    glfw.SetWindowShouldClose(windowPtr, false);
                }
            }
            catch { /* do not crash */ }
        }
    }

    public static void TrySetOpacity(IWindow window, float opacity)
    {
        if (window.Native?.Glfw is nint rawHandle && rawHandle != nint.Zero)
        {
            try
            {
                unsafe
                {
                    var windowPtr = (WindowHandle*)rawHandle;
                    var glfw = Glfw.GetApi();
                    glfw.SetWindowOpacity(windowPtr, opacity);
                }
            }
            catch { /* do not crash */ }
        }
    }

    public static float? TryGetOpacity(IWindow window)
    {
        if (window.Native?.Glfw is nint rawHandle && rawHandle != nint.Zero)
        {
            try
            {
                unsafe
                {
                    var windowPtr = (WindowHandle*)rawHandle;
                    var glfw = Glfw.GetApi();
                    return glfw.GetWindowOpacity(windowPtr);
                }
            }
            catch { /* do not crash */ }
        }

        return null;
    }

    #endregion
}