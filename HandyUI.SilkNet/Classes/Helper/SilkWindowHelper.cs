using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Helper;

public static class SilkWindowHelper
{
    #region Window Creation

    public static HandyWindow CreateWindow(
        // Core Window Properties
        string title,
        SKSize windowSize,

        // Launch State
        SKSize? minimumWindowSize = null,
        SKSize? maximumWindowSize = null,
        WindowState windowState = WindowState.Normal,
        WindowBorder windowBorder = WindowBorder.Resizable,
        bool windowCentered = false,
        bool visibleInTaskbar = true,

        // Parent-Child Relationships
        HandyWindow? parentWindow = null,
        HandyWindow? modalParentWindow = null,

        // Rendering
        bool topMost = false,
        bool vsync = true,
        int framesPerSecond = 60,
        bool useDirtyRendering = true,
        bool invalidateParentOnMove = true,
        bool transparentFrameBuffer = false,

        // OpenGL
        IGLContext? sharedContext = null,
        bool autoInitWindow = false,
        bool autoInitGlfw = true)
    {
        PlatformTools.EnsureSupportedPlatform();

        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height),
            Title = title,
            WindowBorder = windowBorder,
            WindowState = windowState,
            IsEventDriven = useDirtyRendering,
            VSync = vsync,
            TopMost = topMost,
            FramesPerSecond = vsync ? 0 : framesPerSecond,
            SharedContext = sharedContext,
            TransparentFramebuffer = transparentFrameBuffer,
        };

        var window = new HandyWindow(options, minimumWindowSize, maximumWindowSize, useDirtyRendering, invalidateParentOnMove, windowCentered, autoInitWindow, autoInitGlfw)
        {
            VisibleInTaskbar = visibleInTaskbar
        };

        if (modalParentWindow is not null)
        {
            window.ModalParent = modalParentWindow;
        }
        else if (parentWindow is not null)
        {
            window.Parent = parentWindow;
        }

        return window;
    }

    #endregion

    #region Renderer Creation

    public static (HandyWindow window, UIRenderer renderer) CreateWindowAndGetRenderer(
        // Core Window Properties
        string title,
        SKSize windowSize,

        // Launch State
        SKSize? minimumWindowSize = null,
        SKSize? maximumWindowSize = null,
        WindowState windowState = WindowState.Normal,
        WindowBorder windowBorder = WindowBorder.Resizable,
        bool windowCentered = false,
        bool visibleInTaskbar = true,

        // Parent-Child Relationships
        HandyWindow? parentWindow = null,
        HandyWindow? modalParentWindow = null,

        // Rendering
        bool topMost = false,
        bool vsync = true,
        int framesPerSecond = 60,
        bool useDirtyRendering = true,
        bool invalidateParentOnMove = true,
        bool transparentFrameBuffer = false,

        // OpenGL
        IGLContext? sharedContext = null,
        bool autoInitWindow = false,
        bool autoInitGlfw = true)
    {

        PlatformTools.EnsureSupportedPlatform();

        var window = CreateWindow(
            title,
            windowSize,
            minimumWindowSize,
            maximumWindowSize,
            windowState,
            windowBorder,
            windowCentered,
            visibleInTaskbar,
            parentWindow,
            modalParentWindow,
            topMost,
            vsync,
            framesPerSecond,
            useDirtyRendering,
            invalidateParentOnMove,
            transparentFrameBuffer,
            sharedContext,
            autoInitWindow,
            autoInitGlfw);

        var renderer = SilkRendererHelper.Attach(window, useDirtyRendering);

        return (window, renderer);
    }

    #endregion
}