using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Structs;
using HandyUI.SilkNet.Components;
using Silk.NET.Windowing;

namespace HandyUI.SilkNet.Classes.Helper;

public static class SilkWindowHelper
{
    #region Renderer Creation

    public static (HandyWindow window, UIRenderer renderer) CreateWindowAndGetRenderer(WindowConfiguration windowConfiguration)
    {
        PlatformTools.EnsureSupportedPlatform();

        var window = new HandyWindow(windowConfiguration);

        var renderer = SilkRendererHelper.Attach(window, windowConfiguration.UseDirtyRendering);

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