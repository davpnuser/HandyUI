using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Helper;

public static class SilkWindowHelper
{
    public static (HandyWindow window, UIRenderer renderer) CreateWindowAndGetRenderer(
        // Basic properties
        string title,
        SKSize windowSize,
        SKSize? minimumWindowSize = null,
        SKSize? maximumWindowSize = null,
        WindowBorder windowBorder = WindowBorder.Resizable,

        bool vsync = true,
        bool topMost = false,
        bool useDirtyRendering = true,
        bool invalidateOnMove = true,

        // Advanced config
        int framesPerSecond = 60,
        bool autoInitWindow = false,
        IGLContext? sharedContext = null,
        bool autoInitGlfw = true)
    {

        PlatformTools.EnsureSupportedPlatform();

        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height),
            Title = title,
            WindowBorder = windowBorder,
            IsEventDriven = useDirtyRendering,
            VSync = vsync,
            TopMost = topMost,
            FramesPerSecond = vsync ? 0 : framesPerSecond,
            SharedContext = sharedContext,
        };

        var window = new HandyWindow(options, minimumWindowSize, maximumWindowSize, invalidateOnMove, autoInitWindow, autoInitGlfw);
        var renderer = SilkRendererHelper.Attach(window, useDirtyRendering);

        return (window, renderer);
    }
}