using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.SilkNet.Components;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;
using CursorType = HandyUI.Core.Data.Enums.CursorType;
using MouseButton = HandyUI.Core.Data.Enums.MouseButton;

internal sealed class RendererAdapter(HandyWindow window) : IDisposable
{
    #region Internal Fields

    private readonly HandyWindow _window = window;
    private readonly IWindow _silkWindow = window.InternalWindow!;
    private readonly UIRenderer _renderer = new(window);
    private readonly Stopwatch _frameStopwatch = Stopwatch.StartNew();

    private double _lastFrameTime;
    private SKPoint _currentMousePos = new(-1, -1);
    private int _dirtyFramesRemaining = 2;
    private bool _isDisposed;

    private GL? _gl;
    private GRBackendRenderTarget? _backendRenderTarget;
    private SKSurface? _skSurface;
    private SKSurface? _offscreenSurface;
    private IInputContext? _inputContext;
    private IKeyboard? _primaryKeyboard;

    private long _lastMouseMoveInvalidateTick = 0;
    private const long TargetMouseTicksPerInvalidate = 60;

    private const long DoubleClickTimeMs = 500;
    private const float DoubleClickDistanceSq = 16f;
    private long _lastClickTick;
    private SKPoint _lastClickPos;
    private int _currentClickCount = 1;

    #endregion

    #region Internal Properties

    internal GRContext? DrawingContext { get; private set; }
    internal bool UseDirtyRendering => _window.UseDirtyRendering;
    internal bool IsInvalidated => Volatile.Read(ref _dirtyFramesRemaining) > 0;

    #endregion

    #region Initialization

    public UIRenderer InitializeAndGetRenderer()
    {
        _silkWindow.Load += OnWindowLoad;
        _silkWindow.FramebufferResize += OnFramebufferResize;

        if (_silkWindow.IsInitialized && _gl is null)
            OnWindowLoad();

        return _renderer;
    }

    #endregion

    #region Invalidate

    internal void Invalidate()
    {
        Volatile.Write(ref _dirtyFramesRemaining, 2);

        try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
        catch { }
    }

    #endregion

    #region Rendering/Context Functions

    internal void RenderFrame()
    {
        if (_isDisposed) return;
        OnRender();
    }

    private void MakeContextCurrent()
    {
        if (_silkWindow.GLContext is { IsCurrent: false })
            _silkWindow.GLContext.MakeCurrent();
    }

    private void OnFramebufferResize(Vector2D<int> size)
    {
        if (_gl == null) return;

        MakeContextCurrent();
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        CreateRenderTarget(size.X, size.Y);
    }

    private void CreateRenderTarget(int pixelWidth, int pixelHeight)
    {
        if (_gl == null || DrawingContext == null || pixelWidth <= 0 || pixelHeight <= 0) return;

        MakeContextCurrent();

        _offscreenSurface?.Dispose();
        _skSurface?.Dispose();
        _backendRenderTarget?.Dispose();

        _gl.GetInteger((GetPName)0x8CA6, out var framebuffer);
        _gl.GetInteger((GetPName)0x0D57, out var stencilBits);
        _gl.GetInteger((GetPName)0x80A9, out var samples);

        if (stencilBits == 0) stencilBits = 8;

        var maxSamples = DrawingContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888);
        var sampleCount = Math.Min(samples, maxSamples);
        var fbInfo = new GRGlFramebufferInfo((uint)framebuffer, 0x8058);

        _backendRenderTarget = new GRBackendRenderTarget(pixelWidth, pixelHeight, sampleCount, stencilBits, fbInfo);
        _skSurface = SKSurface.Create(DrawingContext, _backendRenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        var imageInfo = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        _offscreenSurface = SKSurface.Create(DrawingContext, false, imageInfo);

        Invalidate();
    }

    private void OnRender()
    {
        if (_skSurface?.Canvas == null || _offscreenSurface?.Canvas == null) return;

        MakeContextCurrent();

        var currentTime = _frameStopwatch.Elapsed.TotalSeconds;
        var deltaTime = (float)(currentTime - _lastFrameTime);
        deltaTime = Math.Min(deltaTime, 0.25f);
        _lastFrameTime = currentTime;

        _renderer.UpdateModules(deltaTime);

        var mustRenderUI = !UseDirtyRendering || Volatile.Read(ref _dirtyFramesRemaining) > 0;

        if (mustRenderUI)
        {
            _renderer.RenderControls(_offscreenSurface.Canvas, _currentMousePos, deltaTime);
            _offscreenSurface.Canvas.Flush();

            if (Interlocked.Decrement(ref _dirtyFramesRemaining) < 0)
                Interlocked.Exchange(ref _dirtyFramesRemaining, 0);
        }

        _skSurface.Canvas.Clear(_renderer.BackgroundColor);
        _offscreenSurface.Draw(_skSurface.Canvas, 0, 0, null);
        _skSurface.Canvas.Flush();
        DrawingContext?.Flush();
    }

    #endregion

    #region Windowing Functions

    private void OnWindowLoad()
    {
        if (_gl is not null) return;

        MakeContextCurrent();

        _gl = GL.GetApi(_silkWindow);

        var skiaGlInterface = GRGlInterface.Create(proc =>
            _silkWindow.GLContext!.TryGetProcAddress(proc, out var addr) ? addr : IntPtr.Zero);

        DrawingContext = GRContext.CreateGl(skiaGlInterface);

        _inputContext = _silkWindow.CreateInput();
        _primaryKeyboard = _inputContext.Keyboards.Count > 0 ? _inputContext.Keyboards[0] : null;
        BindInputEvents();

        CreateRenderTarget(_silkWindow.FramebufferSize.X, _silkWindow.FramebufferSize.Y);
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

    #endregion

    #region Input Helpers

    private (bool IsControl, bool IsShift, bool IsAlt) GetModifierState()
    {
        if (_primaryKeyboard is null)
            return (false, false, false);

        return (
            _primaryKeyboard.IsKeyPressed(Key.ControlLeft) || _primaryKeyboard.IsKeyPressed(Key.ControlRight),
            _primaryKeyboard.IsKeyPressed(Key.ShiftLeft) || _primaryKeyboard.IsKeyPressed(Key.ShiftRight),
            _primaryKeyboard.IsKeyPressed(Key.AltLeft) || _primaryKeyboard.IsKeyPressed(Key.AltRight));
    }

    private int ComputeClickCount(SKPoint position)
    {
        var now = Environment.TickCount64;
        var elapsed = now - _lastClickTick;
        var dx = position.X - _lastClickPos.X;
        var dy = position.Y - _lastClickPos.Y;
        var distSq = (dx * dx) + (dy * dy);

        _currentClickCount = elapsed <= DoubleClickTimeMs && distSq <= DoubleClickDistanceSq ? Math.Min(_currentClickCount + 1, 3) : 1;

        _lastClickTick = now;
        _lastClickPos = position;
        return _currentClickCount;
    }

    internal void SetCursor(CursorType newCursor)
    {
        var mice = _inputContext?.Mice;

        if (mice is null || mice.Count == 0)
            return;

        var mouse = mice[0];
        mouse.Cursor.StandardCursor = MapCursor(newCursor);
    }

    internal CursorType GetCursor()
    {
        var mice = _inputContext?.Mice;

        if (mice is null || mice.Count == 0)
            return CursorType.Default;

        var mouse = mice[0];
        return MapCursor(mouse.Cursor.StandardCursor);
    }

    #endregion

    #region Input Handlers

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        _currentMousePos = new SKPoint(position.X, position.Y);
        var (isCtrl, isShift, isAlt) = GetModifierState();

        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: _window.TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.MoveEvent,
            ClickCount: _currentClickCount,
            IsControlPressed: isCtrl,
            IsShiftPressed: isShift,
            IsAltPressed: isAlt));

        if (UseDirtyRendering)
        {
            var currentTick = Environment.TickCount64;
            if (currentTick - _lastMouseMoveInvalidateTick >= TargetMouseTicksPerInvalidate)
            {
                _lastMouseMoveInvalidateTick = currentTick;
                Invalidate();
            }
        }
        else
        {
            Invalidate();
        }
    }

    private void OnMouseDown(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        var (isCtrl, isShift, isAlt) = GetModifierState();
        var clickCount = ComputeClickCount(_currentMousePos);

        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: _window.TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.MouseDownEvent,
            Button: MapButton(button),
            ClickCount: clickCount,
            IsControlPressed: isCtrl,
            IsShiftPressed: isShift,
            IsAltPressed: isAlt));
        Invalidate();
    }

    private void OnMouseUp(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        var (isCtrl, isShift, isAlt) = GetModifierState();

        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: _window.TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.MouseUpEvent,
            Button: MapButton(button),
            ClickCount: _currentClickCount,
            IsControlPressed: isCtrl,
            IsShiftPressed: isShift,
            IsAltPressed: isAlt));
        Invalidate();
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        var (isCtrl, isShift, isAlt) = GetModifierState();

        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: _window.TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.ScrollEvent,
            Button: MouseButton.Wheel,
            WheelDelta: (int)(scroll.Y * 120),
            ClickCount: _currentClickCount,
            IsControlPressed: isCtrl,
            IsShiftPressed: isShift,
            IsAltPressed: isAlt));
        Invalidate();
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        _renderer.ProcessKeyEvent(new KeyEventContext(
            Tweener: _window.TweenManager,
            Type: KeyEventType.KeyDown,
            KeyCode: MapKey(key),
            Character: '\0',
            IsControlPressed: keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight),
            IsShiftPressed: keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight),
            IsAltPressed: keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight)));

        Invalidate();
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        _renderer.ProcessKeyEvent(new KeyEventContext(
            Tweener: _window.TweenManager,
            Type: KeyEventType.KeyUp,
            KeyCode: MapKey(key),
            Character: '\0',
            IsControlPressed: keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight),
            IsShiftPressed: keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight),
            IsAltPressed: keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight)));

        Invalidate();
    }

    private void OnKeyChar(IKeyboard keyboard, char character)
    {
        _renderer.ProcessKeyEvent(new KeyEventContext(
            Tweener: _window.TweenManager,
            Type: KeyEventType.CharInput,
            KeyCode: (byte)character,
            Character: character,
            IsControlPressed: false,
            IsShiftPressed: false,
            IsAltPressed: false));

        Invalidate();
    }

    #endregion

    #region Cursor Mapping

    private static StandardCursor MapCursor(CursorType cursorType)
    {
        return cursorType switch
        {
            CursorType.Default => StandardCursor.Default,
            CursorType.Arrow => StandardCursor.Arrow,
            CursorType.Hand => StandardCursor.Hand,
            CursorType.IBeam => StandardCursor.IBeam,
            CursorType.BusyArrow => StandardCursor.WaitArrow,
            CursorType.Busy => StandardCursor.Wait,
            CursorType.HResize => StandardCursor.HResize,
            CursorType.VResize => StandardCursor.VResize,
            CursorType.NwseResize => StandardCursor.NwseResize,
            CursorType.NeswResize => StandardCursor.NeswResize,
            CursorType.ResizeAll => StandardCursor.ResizeAll,
            CursorType.NotAllowed => StandardCursor.NotAllowed,
            _ => StandardCursor.Default
        };
    }

    private static CursorType MapCursor(StandardCursor standardCursor)
    {
        return standardCursor switch
        {
            StandardCursor.Default => CursorType.Default,
            StandardCursor.Arrow => CursorType.Arrow,
            StandardCursor.Hand => CursorType.Hand,
            StandardCursor.IBeam => CursorType.IBeam,
            StandardCursor.WaitArrow => CursorType.BusyArrow,
            StandardCursor.Wait => CursorType.Busy,
            StandardCursor.HResize => CursorType.HResize,
            StandardCursor.VResize => CursorType.VResize,
            StandardCursor.NwseResize => CursorType.NwseResize,
            StandardCursor.NeswResize => CursorType.NeswResize,
            StandardCursor.ResizeAll => CursorType.ResizeAll,
            StandardCursor.NotAllowed => CursorType.NotAllowed,
            _ => CursorType.Default
        };
    }

    #endregion

    #region Key Mapping

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

    private static MouseButton MapButton(Silk.NET.Input.MouseButton button)
    {
        return button switch
        {
            Silk.NET.Input.MouseButton.Left => MouseButton.Left,
            Silk.NET.Input.MouseButton.Right => MouseButton.Right,
            Silk.NET.Input.MouseButton.Middle => MouseButton.Middle,
            _ => MouseButton.Generic
        };
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _silkWindow.Load -= OnWindowLoad;
        _silkWindow.FramebufferResize -= OnFramebufferResize;

        UnbindInputEvents();

        MakeContextCurrent();

        _offscreenSurface?.Dispose();
        _skSurface?.Dispose();
        _backendRenderTarget?.Dispose();
        DrawingContext?.Dispose();
        _renderer.Dispose();
        _inputContext?.Dispose();
        _gl?.Dispose();
    }

    #endregion
}