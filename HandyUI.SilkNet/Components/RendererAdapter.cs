using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;
using MouseButton = HandyUI.Core.Data.Enums.MouseButton;

internal sealed class RendererAdapter : IHostWindow, IDisposable
{
    #region Internal Fields

    private readonly IWindow _window;
    private readonly UIRenderer _renderer;
    private readonly Stopwatch _frameStopwatch = Stopwatch.StartNew();

    private double _lastFrameTime;
    private SKPoint _currentMousePos = new(-1, -1);
    private int _dirtyFramesRemaining = 2;
    private int _invalidateGuard;
    private bool _isDisposed;

    private GL? _gl;
    private GRContext? _grContext;
    private GRBackendRenderTarget? _backendRenderTarget;
    private SKSurface? _skSurface;
    private SKSurface? _offscreenSurface;
    private IInputContext? _inputContext;

    private long _lastMouseMoveInvalidateTick = 0;
    private const long TargetMouseTicksPerInvalidate = 60;
    private TweenManager? _tweenManager;

    #endregion

    #region Constructor

    public RendererAdapter(IWindow window, bool useDirtyRendering)
    {
        _window = window;
        UseDirtyRendering = useDirtyRendering;

        _renderer = new UIRenderer(this)
        {
            BackgroundColor = ThemeManager.Instance.BackgroundPrimary
        };
    }

    #endregion

    #region IHostWindow Properties

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    public SKImage? Icon { get; set; }

    public SKSize Size
    {
        get => new(_window.Size.X, _window.Size.Y);
        set => _window.Size = new Vector2D<int>((int)value.Width, (int)value.Height);
    }

    public bool UseDirtyRendering { get; set; }

    public bool VSync
    {
        get => _window.VSync;
        set => _window.VSync = value;
    }

    public int FramesPerSecond { get; set; } = 60;

    public bool IsForeground { get; private set; }

    public DeviceType DeviceType => DeviceType.Desktop;

    public DisplayOrientation Orientation { get; private set; } = DisplayOrientation.Unknown;

    public float DpiScale => 1.0f;

    public float DisplayDist => 50.0f;

    public GRContext DrawingContext => _grContext!;

    public TweenManager TweenManager => _tweenManager!;

    TweenManager IHostWindow.TweenManager
    {
        get => _tweenManager!;
        set => _tweenManager = value;
    }

    public event Action<DisplayOrientation>? WhenOrientationChanged;
    public event Action? WhenInvalidated;

    #endregion

    #region Internal Properties

    internal bool IsInvalidated => Volatile.Read(ref _dirtyFramesRemaining) > 0;

    #endregion

    #region Initialization

    public UIRenderer InitializeAndGetRenderer()
    {
        _window.Load += OnWindowLoad;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Resize += OnWindowResize;
        _window.FocusChanged += OnFocusChanged;

        if (_window.IsInitialized && _gl is null)
            OnWindowLoad();

        return _renderer;
    }

    public void ThemeManagerReady() => ThemeManager.Instance.ThemeChanged += OnThemeChanged;

    private void OnThemeChanged()
    {
        _renderer.BackgroundColor = ThemeManager.Instance.BackgroundPrimary;
        Invalidate();
    }

    #endregion

    #region Invalidate

    public void Invalidate()
    {
        Volatile.Write(ref _dirtyFramesRemaining, 2);

        if (Interlocked.Exchange(ref _invalidateGuard, 1) == 1)
            return;

        try
        {
            _renderer.Invalidate();
            WhenInvalidated?.Invoke();

            try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
            catch { }
        }
        finally
        {
            Interlocked.Exchange(ref _invalidateGuard, 0);
        }
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
        if (_window.GLContext is { IsCurrent: false })
            _window.GLContext.MakeCurrent();
    }

    private void OnFramebufferResize(Vector2D<int> size)
    {
        if (_gl == null) return;

        MakeContextCurrent();
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        CreateRenderTarget(size.X, size.Y);
    }

    private void OnWindowResize(Vector2D<int> size)
    {
        var newOrientation = ComputeOrientation(size.X, size.Y);
        if (newOrientation == Orientation) return;

        Orientation = newOrientation;
        WhenOrientationChanged?.Invoke(newOrientation);
    }

    private static DisplayOrientation ComputeOrientation(int width, int height)
    {
        return width <= 0 || height <= 0
            ? DisplayOrientation.Unknown
            : width >= height ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
    }

    private void OnFocusChanged(bool focused) => IsForeground = focused;

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
        using (var snapshot = _offscreenSurface.Snapshot())
        {
            _skSurface.Canvas.DrawImage(snapshot, 0, 0, SKSamplingOptions.Default);
        }

        _skSurface.Canvas.Flush();
        _grContext?.Flush();
    }

    #endregion

    #region Windowing Functions

    private void OnWindowLoad()
    {
        if (_gl is not null) return;

        MakeContextCurrent();

        _gl = GL.GetApi(_window);

        var skiaGlInterface = GRGlInterface.Create(proc =>
            _window.GLContext!.TryGetProcAddress(proc, out var addr) ? addr : IntPtr.Zero);

        _grContext = GRContext.CreateGl(skiaGlInterface);

        _inputContext = _window.CreateInput();
        BindInputEvents();

        Orientation = ComputeOrientation(_window.Size.X, _window.Size.Y);
        IsForeground = _window.IsVisible;

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

    #endregion

    #region Input Handlers

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        _currentMousePos = new SKPoint(position.X, position.Y);
        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.None));

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
        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.MouseDownEvent,
            Button: MapButton(button)));
        Invalidate();
    }

    private void OnMouseUp(IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.MouseUpEvent,
            Button: MapButton(button)));
        Invalidate();
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        _renderer.ProcessMouseEvent(new MouseEventContext(
            TweenManager: TweenManager,
            MousePosition: _currentMousePos,
            Type: MouseEventType.ScrollEvent,
            Button: MouseButton.Wheel,
            WheelDelta: (int)(scroll.Y * 120)));
        Invalidate();
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        var mappedKey = MapKey(key);
        var isControl = keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);
        var isShift = keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight);
        var isAlt = keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight);

        _renderer.ProcessKeyEvent(new KeyEventContext(
            Tweener: TweenManager,
            Type: KeyEventType.KeyDown,
            KeyCode: mappedKey,
            Character: '\0',
            IsControlPressed: isControl,
            IsShiftPressed: isShift,
            IsAltPressed: isAlt));

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
                Tweener: TweenManager,
                Type: KeyEventType.CharInput,
                KeyCode: (byte)synthesizedChar.Value,
                Character: synthesizedChar.Value,
                IsControlPressed: isControl,
                IsShiftPressed: isShift,
                IsAltPressed: isAlt));
        }

        Invalidate();
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        _renderer.ProcessKeyEvent(new KeyEventContext(
            Tweener: TweenManager,
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
            Tweener: TweenManager,
            Type: KeyEventType.CharInput,
            KeyCode: (byte)character,
            Character: character,
            IsControlPressed: false,
            IsShiftPressed: false,
            IsAltPressed: false));

        Invalidate();
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

        ThemeManager.Instance.ThemeChanged -= OnThemeChanged;

        _window.Load -= OnWindowLoad;
        _window.FramebufferResize -= OnFramebufferResize;
        _window.Resize -= OnWindowResize;
        _window.FocusChanged -= OnFocusChanged;

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

    #endregion
}