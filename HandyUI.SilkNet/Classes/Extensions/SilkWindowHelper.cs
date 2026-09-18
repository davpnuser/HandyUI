using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Extensions;

public static class SilkWindowHelper
{
    public static IWindow CreateWindow(string title, SKSize windowSize, WindowBorder windowBorder = WindowBorder.Resizable, bool vsync = true)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>((int)windowSize.Width, (int)windowSize.Height);
        options.Title = title;
        options.WindowBorder = windowBorder;

        if (vsync)
        {
            options.VSync = true;
            options.FramesPerSecond = 0;
        }
        else
        {
            options.VSync = false;
        }

        using var window = Window.Create(options);

        return window;
    }

    public static (IWindow window, UIRenderer rendere) CreateWindowAndGetRenderer(string title, SKSize windowSize, WindowBorder windowBorder = WindowBorder.Resizable, bool vsync = true, bool useDirtyRendering = true)
    {
        var window = CreateWindow(title, windowSize, windowBorder, vsync);
        var renderer = SilkRendererHelper.Attach(window, useDirtyRendering);

        return (window, renderer);
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