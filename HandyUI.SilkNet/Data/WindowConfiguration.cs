using HandyUI.SilkNet.Components;
using Silk.NET.Core.Contexts;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Data;

public record WindowConfiguration
{
    // Core properties
    public string Title { get; init; } = "Untitled Window";
    public SKSize Size { get; init; } = new SKSize(800, 600);

    // Launch State
    public SKSize? MinSize { get; init; } = null;
    public SKSize? MaxSize { get; init; } = null;
    public WindowState WindowState { get; init; } = WindowState.Normal;
    public WindowBorder WindowBorder { get; init; } = WindowBorder.Resizable;
    public float Opacity { get; init; } = 1f;
    public bool IsCenteredOnStartup { get; init; } = false;
    public bool IsVisibleInTaskbar { get; init; } = true;
    public bool IsResizable { get; init; } = true;
    public bool IsMovable { get; init; } = true;
    public bool IsCloseable { get; init; } = true;
    public bool IsMaximizable { get; init; } = true;
    public bool IsMinimizable { get; init; } = true;
    public SKImage? Icon { get; init; } = null;

    // Parent-Child Relationships
    public HandyWindow? SubwindowParent { get; init; } = null;
    public HandyWindow? ModalParent { get; init; } = null;

    // Rendering
    public bool TopMost { get; init; } = false;
    public bool VSync { get; init; } = true;
    public int FramesPerSecond { get; init; } = 60;
    public bool UseDirtyRendering { get; init; } = true;
    public bool InvalidateParentOnMove { get; init; } = true;
    public bool TransparentFrameBuffer { get; init; } = false;

    // OpenGL
    public IGLContext? SharedContext { get; init; } = null;
    public bool AutoInitWindow { get; init; } = false;
    public bool AutoInitGlfw { get; init; } = true;

    // Advanced Configuration
    public string PumpThreadName
    {
        get => field ?? $"HandyWindow-{Title}";
        init;
    }
    public WindowOptions? CustomWindowOptions { get; init; } = null;

    public static WindowConfiguration Default => new();
};