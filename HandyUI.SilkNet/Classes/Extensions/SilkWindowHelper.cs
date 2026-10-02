using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Extensions;

//[SupportedOSPlatform("windows")]
public static class SilkWindowHelper
{
    private static readonly Lock GlfwCreationLock = new();
    private static bool _isGlfwInitialized;

    private static void EnsureWindowsPlatform()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("The HandyUI adapter for Silk.NET is currently not available on your platform. You can go to the HandyUI repository to help contribute in a fix.");
        }
    }

    private static void EnsureGlfwInitialized()
    {
        if (_isGlfwInitialized) return;

        lock (GlfwCreationLock)
        {
            if (!_isGlfwInitialized)
            {
                Window.PrioritizeGlfw();
                _isGlfwInitialized = true;
            }
        }
    }

    public static IWindow CreateWindow(
        string title,
        SKSize windowSize,
        WindowBorder windowBorder = WindowBorder.Resizable,
        bool vsync = true,
        IGLContext? sharedContext = null,
        bool autoInitGlfw = true)
    {
        EnsureWindowsPlatform();

        if (autoInitGlfw)
        {
            EnsureGlfwInitialized();
        }

        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height),
            Title = title,
            WindowBorder = windowBorder,
            VSync = vsync,
            FramesPerSecond = vsync ? 0 : WindowOptions.Default.FramesPerSecond,
            SharedContext = sharedContext
        };

        var window = Window.Create(options);
        window.Initialize();

        return window;
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
        EnsureWindowsPlatform();

        if (autoInitGlfw)
        {
            EnsureGlfwInitialized();
        }

        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height),
            Title = title,
            WindowBorder = windowBorder,
            IsEventDriven = useDirtyRendering,
            VSync = vsync,
            FramesPerSecond = vsync ? 0 : WindowOptions.Default.FramesPerSecond,
            SharedContext = sharedContext
        };

        var window = Window.Create(options);
        var renderer = SilkRendererHelper.Attach(window, useDirtyRendering);

        window.Initialize();

        return (window, renderer);
    }

    public static void SetAsIcon(this SKImage skImage, IWindow window)
    {
        EnsureWindowsPlatform();
        ArgumentNullException.ThrowIfNull(skImage);
        ArgumentNullException.ThrowIfNull(window);

        if (!window.IsInitialized)
        {
            window.Initialize();
        }

        using var bmp = SKBitmap.FromImage(skImage);
        using var rgba = new SKBitmap(new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Rgba8888));
        bmp.CopyTo(rgba);

        var icon = new RawImage(rgba.Width, rgba.Height, rgba.Bytes);
        window.SetWindowIcon(ref icon);
    }
}