using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Extensions;

public static class SilkWindowHelper
{
    private static readonly Lock GlfwCreationLock = new();
    private static bool _isGlfwInitialized;

    public static IWindow CreateWindow(
        string title,
        SKSize windowSize,
        WindowBorder windowBorder = WindowBorder.Resizable,
        bool vsync = true,
        IGLContext? sharedContext = null,
        bool autoInitGlfw = true)
    {
        lock (GlfwCreationLock)
        {
            if (autoInitGlfw && !_isGlfwInitialized)
            {
                Window.PrioritizeGlfw();
                _isGlfwInitialized = true;
            }

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height);
            options.Title = title;
            options.WindowBorder = windowBorder;

            if (sharedContext != null)
            {
                options.SharedContext = sharedContext;
            }

            if (vsync)
            {
                options.VSync = true;
                options.FramesPerSecond = 0;
            }
            else
            {
                options.VSync = false;
            }

            var window = Window.Create(options);

            window.Initialize();

            return window;
        }
    }

    public static (IWindow window, UIRenderer renderer) CreateWindowAndGetRenderer(
        string title,
        SKSize windowSize,
        WindowBorder windowBorder = WindowBorder.Resizable,
        bool vsync = true,
        bool useDirtyRendering = true,
        IGLContext? sharedContext = null,
        bool autoInitGlfw = true)
    {
        lock (GlfwCreationLock)
        {
            if (autoInitGlfw && !_isGlfwInitialized)
            {
                Window.PrioritizeGlfw();
                _isGlfwInitialized = true;
            }

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height);
            options.Title = title;
            options.WindowBorder = windowBorder;

            if (sharedContext != null)
            {
                options.SharedContext = sharedContext;
            }

            if (vsync)
            {
                options.VSync = true;
                options.FramesPerSecond = 0;
            }
            else
            {
                options.VSync = false;
            }

            var window = Window.Create(options);

            var renderer = SilkRendererHelper.Attach(window, useDirtyRendering);

            window.Initialize();

            return (window, renderer);
        }
    }

    public static void SetAsIcon(this SKImage skImage, IWindow window)
    {
        if (!window.IsInitialized)
            window.Initialize();

        using var bmp = SKBitmap.FromImage(skImage);
        using var rgba = new SKBitmap(new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Rgba8888));
        bmp.CopyTo(rgba);

        var icon = new RawImage(rgba.Width, rgba.Height, rgba.Bytes);
        window.SetWindowIcon(ref icon);
    }
}