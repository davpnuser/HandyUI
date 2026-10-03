using HandyUI.Core.Classes.Records;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Numerics;

namespace HandyUI.SilkNet.Classes.Helper;

//[SupportedOSPlatform("windows")]
public static class SilkRendererHelper
{
    public static UIRenderer Attach(HandyWindow window, bool useDirtyRendering = true)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("The HandyUI adapter for Silk.NET is currently not available on your platform. You can go to the HandyUI repository to help contribute in a fix.");
        }

        ArgumentNullException.ThrowIfNull(window);

        var adapter = new SilkWindowRendererAdapter(window.InternalWindow!, useDirtyRendering);
        return adapter.InitializeAndGetRenderer();
    }
}

//[SupportedOSPlatform("windows")]
internal sealed class SilkWindowRendererAdapter(IWindow window, bool useDirtyRendering) : IDisposable
{
    private readonly IWindow _window = window;
    private readonly bool _useDirtyRendering = useDirtyRendering;
    private readonly UIRenderer _renderer = new(useDirtyRendering);

    private SKPoint _currentMousePos = new(-1, -1);
    private int _dirtyFramesRemaining = 2;
    private bool _isDisposed;

    private GL? _gl;
    private GRContext? _grContext;
    private GRBackendRenderTarget? _backendRenderTarget;
    private SKSurface? _skSurface;
    private SKSurface? _offscreenSurface;
    private IInputContext? _inputContext;

    public UIRenderer InitializeAndGetRenderer()
    {
        _window.Load += OnWindowLoad;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Render += OnRender;
        _window.Closing += OnWindowClosing;

        if (_window.IsInitialized)
        {
            OnWindowLoad();
        }

        return _renderer;
    }

    private void MakeContextCurrent()
    {
        if (_window.GLContext is { IsCurrent: false })
        {
            _window.GLContext.MakeCurrent();
        }
    }

    private void InvalidateFrames()
    {
        _dirtyFramesRemaining = 2;
        _renderer.Invalidate();
    }

    private void OnWindowLoad()
    {
        MakeContextCurrent();

        _gl = GL.GetApi(_window);

        var skiaGlInterface = GRGlInterface.Create(proc =>
            _window.GLContext!.TryGetProcAddress(proc, out var addr) ? addr : IntPtr.Zero);

        _grContext = GRContext.CreateGl(skiaGlInterface);

        _inputContext = _window.CreateInput();
        BindInputEvents();

        CreateRenderTarget(_window.FramebufferSize.X, _window.FramebufferSize.Y);
    }

    private void BindInputEvents()
    {
        if (_inputContext is null) return;

        foreach (var mouse in _inputContext.Mice)
        {
            mouse.MouseMove += OnMouseMove;
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.Scroll += OnMouseScroll;
        }

        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
            keyboard.KeyChar += OnKeyChar;
        }
    }

    private void UnbindInputEvents()
    {
        if (_inputContext is null) return;

        foreach (var mouse in _inputContext.Mice)
        {
            mouse.MouseMove -= OnMouseMove;
            mouse.MouseDown -= OnMouseDown;
            mouse.MouseUp -= OnMouseUp;
            mouse.Scroll -= OnMouseScroll;
        }

        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown -= OnKeyDown;
            keyboard.KeyUp -= OnKeyUp;
            keyboard.KeyChar -= OnKeyChar;
        }
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        _currentMousePos = new SKPoint(position.X, position.Y);
        _renderer.ProcessMouseEvent(new MouseEventContext(
            ClientPosition: _currentMousePos,
            Type: MouseEventType.Move
        ));
        InvalidateFrames();
    }

    private void OnMouseDown(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        _renderer.ProcessMouseEvent(new MouseEventContext(
            ClientPosition: _currentMousePos,
            Type: MouseEventType.MouseDown,
            Button: MapButton(button)
        ));
        InvalidateFrames();
    }

    private void OnMouseUp(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        _renderer.ProcessMouseEvent(new MouseEventContext(
            ClientPosition: _currentMousePos,
            Type: MouseEventType.MouseUp,
            Button: MapButton(button)
        ));
        InvalidateFrames();
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        _renderer.ProcessMouseEvent(new MouseEventContext(
            ClientPosition: _currentMousePos,
            Type: MouseEventType.Wheel,
            Button: HandyUI.Core.Classes.Records.MouseButton.Middle,
            WheelDelta: (int)(scroll.Y * 120)
        ));
        InvalidateFrames();
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        var mappedKey = MapKey(key);
        var isControl = keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);
        var isShift = keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight);
        var isAlt = keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight);

        _renderer.ProcessKeyEvent(new KeyEventContext(
            Type: KeyEventType.KeyDown,
            KeyCode: mappedKey,
            Character: '\0',
            IsControlPressed: isControl,
            IsShiftPressed: isShift,
            IsAltPressed: isAlt
        ));

        char? synthesizedChar = key switch
        {
            Key.Tab => '\t',
            Key.Enter => '\r',
            Key.Backspace => '\b',
            _ => null
        };

        if (synthesizedChar.HasValue)
        {
            _renderer.ProcessKeyEvent(new KeyEventContext(
                Type: KeyEventType.CharInput,
                KeyCode: (byte)synthesizedChar.Value,
                Character: synthesizedChar.Value,
                IsControlPressed: isControl,
                IsShiftPressed: isShift,
                IsAltPressed: isAlt
            ));
        }

        InvalidateFrames();
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        _renderer.ProcessKeyEvent(new KeyEventContext(
            Type: KeyEventType.KeyUp,
            KeyCode: MapKey(key),
            Character: '\0',
            IsControlPressed: keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight),
            IsShiftPressed: keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight),
            IsAltPressed: keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight)
        ));

        InvalidateFrames();
    }

    private void OnKeyChar(IKeyboard keyboard, char character)
    {
        _renderer.ProcessKeyEvent(new KeyEventContext(
            Type: KeyEventType.CharInput,
            KeyCode: (byte)character,
            Character: character,
            IsControlPressed: false,
            IsShiftPressed: false,
            IsAltPressed: false
        ));

        InvalidateFrames();
    }

    private void CreateRenderTarget(int pixelWidth, int pixelHeight)
    {
        if (_gl == null || _grContext == null || pixelWidth <= 0 || pixelHeight <= 0) return;

        MakeContextCurrent();

        _offscreenSurface?.Dispose();
        _skSurface?.Dispose();
        _backendRenderTarget?.Dispose();

        _gl.GetInteger((GetPName)0x8CA6, out var framebuffer);
        _gl.GetInteger((GetPName)0x0D57, out var stencilBits);
        _gl.GetInteger((GetPName)0x80A9, out var samples);

        if (stencilBits == 0) stencilBits = 8;

        var maxSamples = _grContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888);
        var sampleCount = Math.Min(samples, maxSamples);
        var fbInfo = new GRGlFramebufferInfo((uint)framebuffer, 0x8058);

        _backendRenderTarget = new GRBackendRenderTarget(pixelWidth, pixelHeight, sampleCount, stencilBits, fbInfo);
        _skSurface = SKSurface.Create(_grContext, _backendRenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        var imageInfo = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        _offscreenSurface = SKSurface.Create(_grContext, false, imageInfo);

        InvalidateFrames();
    }

    private void OnFramebufferResize(Silk.NET.Maths.Vector2D<int> size)
    {
        if (_gl == null) return;

        MakeContextCurrent();
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        CreateRenderTarget(size.X, size.Y);
    }

    private void OnRender(double delta)
    {
        if (_skSurface?.Canvas == null || _offscreenSurface?.Canvas == null) return;

        MakeContextCurrent();

        var mustRenderUI = !_useDirtyRendering || _dirtyFramesRemaining > 0;

        if (mustRenderUI)
        {
            _offscreenSurface.Canvas.Clear(SKColors.Transparent);
            _renderer.RenderControls(_offscreenSurface.Canvas, _currentMousePos);
            _offscreenSurface.Canvas.Flush();

            if (_dirtyFramesRemaining > 0)
            {
                _dirtyFramesRemaining--;
            }
        }

        _skSurface.Canvas.Clear(SKColors.Transparent);
        using (var snapshot = _offscreenSurface.Snapshot())
        {
            _skSurface.Canvas.DrawImage(snapshot, 0, 0, SKSamplingOptions.Default);
        }

        _skSurface.Canvas.Flush();
        _grContext?.Flush();
    }

    private void OnWindowClosing()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _window.Load -= OnWindowLoad;
        _window.FramebufferResize -= OnFramebufferResize;
        _window.Render -= OnRender;
        _window.Closing -= OnWindowClosing;

        UnbindInputEvents();

        MakeContextCurrent();

        _offscreenSurface?.Dispose();
        _skSurface?.Dispose();
        _backendRenderTarget?.Dispose();
        _grContext?.Dispose();
        _renderer.Dispose();
        _inputContext?.Dispose();
        _gl?.Dispose();
    }

    private static int MapKey(Key key)
    {
        return key switch
        {
            Key.Tab => 9,
            Key.Enter => 13,
            Key.Backspace => 8,
            Key.Escape => 27,
            Key.Delete => 46,
            Key.Space => 32,
            Key.Left => 37,
            Key.Up => 38,
            Key.Right => 39,
            Key.Down => 40,
            Key.Home => 36,
            Key.End => 35,
            _ => (int)key
        };
    }

    private static HandyUI.Core.Classes.Records.MouseButton MapButton(Silk.NET.Input.MouseButton button)
    {
        return button switch
        {
            Silk.NET.Input.MouseButton.Left => HandyUI.Core.Classes.Records.MouseButton.Left,
            Silk.NET.Input.MouseButton.Right => HandyUI.Core.Classes.Records.MouseButton.Right,
            Silk.NET.Input.MouseButton.Middle => HandyUI.Core.Classes.Records.MouseButton.Middle,
            _ => Core.Classes.Records.MouseButton.None
        };
    }
}