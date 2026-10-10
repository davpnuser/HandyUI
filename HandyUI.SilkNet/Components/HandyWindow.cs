using HandyUI.SilkNet.Classes.Extensions;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Structs;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Components;

public class HandyWindow : IDisposable
{
    #region Important Properties/Fields

    public bool IsDisposed { get; private set; }
    public bool IsInitialized => _isInitialized;

    public IWindow? InternalWindow { get; private set; }
    internal SilkWindowRendererAdapter? RendererAdapter { get; set; }

    #endregion

    #region Properties

    public string Title
    {
        get => InternalWindow is null ? "" : InternalWindow.Title;
        set
        {
            if (InternalWindow is null)
                return;

            InternalWindow.Title = value;
        }
    }
    public WindowBorder Border
    {
        get => InternalWindow is null ? WindowBorder.Hidden : InternalWindow.WindowBorder;
        set
        {
            if (InternalWindow is null)
                return;

            InternalWindow.WindowBorder = value;
        }
    }

    public WindowState State
    {
        get => InternalWindow is null ? WindowState.Normal : InternalWindow.WindowState;
        set
        {
            if (InternalWindow is null)
                return;

            InternalWindow.WindowState = value;
        }
    }

    public bool VSync
    {
        get => InternalWindow is not null && InternalWindow.VSync;
        set
        {
            if (InternalWindow is null)
                return;

            InternalWindow.VSync = value;
        }
    }

    public bool UseDirtyRendering { get; set; } = true;

    public SKSize? Size
    {
        get => InternalWindow is not null ? new SKSize(InternalWindow.Size.X, InternalWindow.Size.Y) : null;
        set
        {
            if (InternalWindow is not null && value is not null)
            {
                InternalWindow.Size = new Vector2D<int>((int)value.Value.Width, (int)value.Value.Height);
            }
        }
    }

    public SKSize? MaxSize
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            ApplySizeLimitsInternal();
        }
    } = null;

    public SKSize? MinSize
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            ApplySizeLimitsInternal();
        }
    } = null;

    private SKPoint? _previousPosition;
    public SKPoint? Position
    {
        get => InternalWindow is not null ? new SKPoint(InternalWindow.Position.X, InternalWindow.Position.Y) : null;
        set
        {
            if (InternalWindow is not null && value is not null)
            {
                InternalWindow.Position = new Vector2D<int>((int)value.Value.X, (int)value.Value.Y);
                _previousPosition = value;
            }
        }
    }

    public bool IsMovable
    {
        get;
        set
        {
            field = value;
            if (!field && Position.HasValue)
                _previousPosition = Position.Value;
        }
    }

    public bool IsResizable
    {
        get;
        set
        {
            field = value;
            InternalWindow?.WindowBorder = field ? Border : WindowBorder.Fixed;
        }
    }

    public bool VisibleInTaskbar
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ApplyVisibleInTaskbar();
        }
    } = true;

    public bool AllowClose { get; set; } = true;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetParent(nint hWndChild, nint hWndNewParent);

    private readonly List<HandyWindow> _children = [];

    public HandyWindow? Parent
    {
        get;
        set
        {
            if (IsDisposed) return;

            if (value is not null && value == ModalParent)
                throw new InvalidOperationException("A window cannot have the same window as both its Parent and its ModalParent.");

            field?._children.Remove(this);
            field = value;
            value?._children.Add(this);
            _parentNeedsApply = true;
        }
    }

    public HandyWindow? ModalParent
    {
        get;
        set
        {
            if (IsDisposed) return;
            if (field == value) return;

            if (value is not null && value == Parent)
                throw new InvalidOperationException("A window cannot have the same window as both its Parent and its ModalParent.");

            if (field is not null && field._modalChild == this)
            {
                field._modalChild = null;
                field.ApplyModalState();
            }

            field = value;

            if (value is not null)
            {
                if (value._modalChild is not null && value._modalChild != this)
                    throw new InvalidOperationException("The specified window already has a modal child window.");

                value._modalChild = this;
                value.ApplyModalState();
            }
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnableWindow(nint hWnd, bool bEnable);

    public HandyWindow? ModalChild => _modalChild;

    private void ApplyModalState()
    {
        if (IsDisposed) return;

        if (!_isInitialized || InternalWindow?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == nint.Zero)
        {
            _modalStateNeedsApply = true;
            return;
        }

        void apply()
        {
            if (InternalWindow?.Native?.Win32?.Hwnd is nint h && h != nint.Zero)
            {
                try { EnableWindow(h, _modalChild is null); }
                catch { /* do not crash */ }
            }
        }

        if (IsOnPumpThread)
        {
            apply();
        }
        else if (!_closeRequested)
        {
            try { Invoke(apply); }
            catch { /* do not crash */ }
        }

        _modalStateNeedsApply = false;
    }

    public bool InvalidateParentOnMove { get; set; } = true;

    #endregion

    #region Events

    public event Action? OnLoad;
    public event Action? OnClosing;
    public event Action<SKPoint>? OnMove;
    public event Action<SKSize>? OnResize;
    public event Action<WindowState>? OnStateChanged;
    public event Action<bool>? OnFocusChanged;

    #endregion

    #region Internal Fields

    private volatile HandyWindow? _modalChild;

    internal bool IsOnPumpThread => Thread.CurrentThread == _pumpThread;

    private static readonly Lock WindowCreationLock = new();

    private readonly Thread _pumpThread;
    private readonly ManualResetEventSlim _windowCreated = new(false);
    private readonly ManualResetEventSlim _initSignal = new(false);
    private readonly ManualResetEventSlim _closedSignal = new(false);
    private readonly ConcurrentQueue<Action> _workQueue = new();
    private readonly Stopwatch _pumpStopwatch = new();

    private TaskCompletionSource? _closedTcs;

    private volatile bool _initRequested;
    private volatile bool _programmaticClose;
    private volatile bool _closeRequested;
    private volatile bool _isInitialized;
    private volatile bool _parentNeedsApply;
    private volatile bool _modalStateNeedsApply;
    private volatile bool _visibleInTaskbarNeedsApply;
    private volatile bool _closingHandled;
    private volatile bool _invalidateRequested;
    private readonly bool _autoInit;

    private double _lastFrameTime;
    private int _moveTick = 0;
    private bool _moveInProgress;
    private int _resizeTick = 0;
    private bool _resizeInProgress;

    private readonly bool _centerOnStartup = false;

    #endregion

    #region Constructor

    public HandyWindow(WindowConfiguration windowConfiguration)
    {
        IsMovable = true;
        IsResizable = true;
        UseDirtyRendering = windowConfiguration.UseDirtyRendering;
        _autoInit = windowConfiguration.AutoInitWindow;
        InvalidateParentOnMove = windowConfiguration.InvalidateParentOnMove;
        _centerOnStartup = windowConfiguration.WindowCentered;

        VisibleInTaskbar = windowConfiguration.VisibleInTaskbar;
        AllowClose = windowConfiguration.AllowClose;

        if (windowConfiguration.ModalParent is not null)
        {
            ModalParent = windowConfiguration.ModalParent;
        }
        else if (windowConfiguration.SubwindowParent is not null)
        {
            Parent = windowConfiguration.SubwindowParent;
        }

        var windowOptions = windowConfiguration.CustomWindowOptions ?? WindowOptions.Default with
        {
            Size = new Vector2D<int>((int)windowConfiguration.Size.Width, (int)windowConfiguration.Size.Height),
            Title = windowConfiguration.Title,
            WindowBorder = windowConfiguration.WindowBorder,
            WindowState = windowConfiguration.WindowState,
            IsEventDriven = windowConfiguration.UseDirtyRendering,
            VSync = windowConfiguration.VSync,
            TopMost = windowConfiguration.TopMost,
            FramesPerSecond = windowConfiguration.VSync ? 0 : windowConfiguration.FramesPerSecond,
            SharedContext = windowConfiguration.SharedContext,
            TransparentFramebuffer = windowConfiguration.TransparentFrameBuffer,
        };

        _pumpThread = new Thread(() => PumpLoop(windowConfiguration, windowOptions))
        {
            IsBackground = true,
            Name = windowConfiguration.CustomPumpThreadName ?? $"HandyWindow-{windowConfiguration.Title}"
        };

        if (OperatingSystem.IsWindows())
            _pumpThread.SetApartmentState(ApartmentState.STA);

        _pumpThread.Start();

        _windowCreated.Wait();
    }

    #endregion

    #region Loop Pumping Function

    private void PumpLoop(WindowConfiguration windowConfiguration, WindowOptions windowOptions)
    {
        if (windowConfiguration.AutoInitGlfw)
            GlfwTool.EnsureGlfwInitialized();

        IWindow window;
        lock (WindowCreationLock)
        {
            window = Window.Create(windowOptions);
        }

        InternalWindow = window;

        #region Load Handler

        window.Load += () =>
        {
            Size = new SKSize(windowOptions.Size.X, windowOptions.Size.Y);
            SetSizeLimits(windowConfiguration.MinSize, windowConfiguration.MaxSize);

            _isInitialized = true;
            _initSignal.Set();

            if (_centerOnStartup)
                window.Center();

            OnLoad?.Invoke();
        };

        _windowCreated.Set();

        #endregion

        #region Close Handler

        window.Closing += () =>
        {
            if (_closingHandled) return;

            if (!AllowClose && !_programmaticClose)
            {
                SilkWindowHelper.TryCancelClose(window);
                return;
            }

            _closingHandled = true;

            window.IsVisible = false;
            _closeRequested = true;

            if (ModalParent is not null)
                ModalParent = null;

            _modalChild?.InternalWindow?.Close();

            Parent = null;

            foreach (var child in _children.ToArray())
                child.InternalWindow?.Close();

            OnClosing?.Invoke();

            _closedTcs?.TrySetResult();
            _closedSignal.Set();
        };

        #endregion

        #region Move Handler

        window.Move += newPos =>
        {
            if (InvalidateParentOnMove)
                InvalidateParent();

            if (_moveInProgress)
                return;

            _moveInProgress = true;

            try
            {
                if (!IsMovable && _previousPosition is not null)
                    window.Position = _previousPosition.ToVector2D();

                else
                    OnMove?.Invoke(newPos.ToSKPoint());
            }
            catch { _moveInProgress = false; /* do not crash */ }
            finally { _moveInProgress = false; }

            if (_moveTick++ == 4)
            {
                _moveTick = 0;
                RenderNow();
            }
            else
            {
                Invalidate();
            }
        };

        #endregion

        #region Resize Handler

        window.Resize += newSize =>
        {
            if (InvalidateParentOnMove)
                InvalidateParent();

            if (_resizeInProgress)
                return;

            _resizeInProgress = true;

            try { OnResize?.Invoke(newSize.ToSKSize()); }
            catch { _resizeInProgress = false; /* do not crash */ }
            finally { _resizeInProgress = false; }

            if (_resizeTick++ == 4)
            {
                _resizeTick = 0;
                RenderNow();
            }
            else
            {
                Invalidate();
            }
        };

        #endregion

        #region Other Handlers

        window.StateChanged += s => OnStateChanged?.Invoke(s);

        window.FocusChanged += f =>
        {
            if (f && _modalChild is not null)
            {
                try { _modalChild.Invoke(() => _modalChild.InternalWindow?.Focus()); }
                catch { /* do not crash */ }
            }

            OnFocusChanged?.Invoke(f);
        };

        #endregion

        #region Loop pumping

        var vsync = windowOptions.VSync;
        var targetFps = windowOptions.FramesPerSecond;

        var glfw = Silk.NET.GLFW.Glfw.GetApi();

        const double IdleRefreshSeconds = 1.0 / 5.0;

        _pumpStopwatch.Restart();
        _lastFrameTime = 0;

        try
        {
            while (!_closeRequested && !IsDisposed)
            {
                var frameStart = _pumpStopwatch.Elapsed.TotalSeconds;

                var hadWork = false;

                while (_workQueue.TryDequeue(out var work))
                {
                    hadWork = true;
                    try { work(); }
                    catch {  /* do not crash */ }
                }

                if (!window.IsInitialized)
                {
                    if (_autoInit || _initRequested)
                    {
                        try { window.Initialize(); }
                        catch { /* do not crash */ }
                    }

                    Thread.Sleep(1);
                    continue;
                }

                try { window.DoEvents(); }
                catch { /* do not crash */ }

                if (IsInitialized && _parentNeedsApply)
                {
                    var desired = Parent;

                    if (desired is null || desired.IsInitialized)
                    {
                        ApplyParentToWindow(desired);
                        _parentNeedsApply = false;
                    }
                }

                if (IsInitialized && _modalStateNeedsApply)
                {
                    ApplyModalState();
                }

                if (IsInitialized && _visibleInTaskbarNeedsApply)
                {
                    ApplyVisibleInTaskbar();
                }

                if (_invalidateRequested)
                {
                    _invalidateRequested = false;
                    RendererAdapter?.Invalidate();
                }

                var useDirty = RendererAdapter?.UseDirtyRendering ?? UseDirtyRendering;
                var invalidated = RendererAdapter?.IsInvalidated == true;

                var shouldRender = !useDirty || invalidated
                                || (frameStart - _lastFrameTime) >= IdleRefreshSeconds;

                if (shouldRender)
                {
                    try { RendererAdapter?.RenderFrame(); }
                    catch { /* do not crash */ }

                    try { window.GLContext?.SwapBuffers(); }
                    catch { /* do not crash */ }

                    _lastFrameTime = _pumpStopwatch.Elapsed.TotalSeconds;
                }

                if (!useDirty)
                {
                    if (!vsync && targetFps > 0)
                    {
                        var target = 1.0 / targetFps;
                        var elapsed = _pumpStopwatch.Elapsed.TotalSeconds - frameStart;
                        var wait = target - elapsed;

                        if (wait > 0.0005)
                            Thread.Sleep(TimeSpan.FromSeconds(wait));
                    }
                }
                else
                {
                    if (!invalidated && !hadWork)
                    {
                        try
                        {
                            if (_invalidateRequested) { glfw.PollEvents(); }
                            else { glfw.WaitEventsTimeout(IdleRefreshSeconds); }
                        }
                        catch { /* do not crash */ }
                    }
                }
            }
        }
        finally
        {
            _programmaticClose = true;

            try { window.Close(); }
            catch { /* do not crash */ }

            try { window.Dispose(); }
            catch { /* do not crash */ }

            InternalWindow = null;

            _closedTcs?.TrySetResult();
            _closedSignal.Set();
        }

        #endregion
    }

    #endregion

    #region Internal Functions

    [DllImport("user32.dll")]
    private static extern bool InvalidateRect(nint hWnd, nint lpRect, bool bErase);

    private void InvalidateParent()
    {
        if (InvalidateParentOnMove && Parent?.InternalWindow?.Native?.Win32?.Hwnd is nint parentHwnd && parentHwnd != nint.Zero)
        {
            InvalidateRect(parentHwnd, nint.Zero, false);
        }

        Parent?.Invalidate();
    }

    private void ApplyParentToWindow(HandyWindow? value)
    {
        if (OperatingSystem.IsWindows() && InternalWindow?.Native?.Win32?.Hwnd is nint childHwnd && childHwnd != nint.Zero)
        {
            var parentHwnd = value?.InternalWindow?.Native?.Win32?.Hwnd ?? nint.Zero;
            SetParent(childHwnd, parentHwnd);

            HandyWindowHelper.ApplyClipChildren(value);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    private const uint WM_NULL = 0x0000;

    private void WakePumpThread()
    {
        if (InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
        {
            try { PostMessage(hwnd, WM_NULL, 0, 0); }
            catch { /* do not crash */ }
        }
    }

    private void RenderNow()
    {
        if (IsDisposed || !_isInitialized)
            return;

        if (InternalWindow is null || RendererAdapter is null)
            return;

        if (!IsOnPumpThread)
            return;

        try
        {
            RendererAdapter.Invalidate();
            RendererAdapter.RenderFrame();
            InternalWindow.GLContext?.SwapBuffers();
        }
        catch { /* do not crash */ }
    }

    private void ApplySizeLimitsInternal()
    {
        Initialize();

        if (InternalWindow?.Native?.Glfw is not nint rawHandle || rawHandle == nint.Zero)
            return;

        var glfw = Silk.NET.GLFW.Glfw.GetApi();

        unsafe
        {
            var windowPtr = (Silk.NET.GLFW.WindowHandle*)rawHandle;
            var minW = MinSize.HasValue ? (int)MinSize.Value.Width : Silk.NET.GLFW.Glfw.DontCare;
            var minH = MinSize.HasValue ? (int)MinSize.Value.Height : Silk.NET.GLFW.Glfw.DontCare;
            var maxW = MaxSize.HasValue ? (int)MaxSize.Value.Width : Silk.NET.GLFW.Glfw.DontCare;
            var maxH = MaxSize.HasValue ? (int)MaxSize.Value.Height : Silk.NET.GLFW.Glfw.DontCare;

            glfw.SetWindowSizeLimits(windowPtr, minW, minH, maxW, maxH);
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    private static nint GetWindowLongPtr(nint hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
    }

    private static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
    {
        return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : SetWindowLong32(hWnd, nIndex, (int)dwNewLong);
    }

    private const int GWL_EXSTYLE = -20;
    private const nint WS_EX_TOOLWINDOW = 0x00000080;
    private const nint WS_EX_APPWINDOW = 0x00040000;

    private void ApplyVisibleInTaskbar()
    {
        if (IsDisposed) return;

        if (!OperatingSystem.IsWindows()) return;

        if (!_isInitialized || InternalWindow?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == nint.Zero)
        {
            _visibleInTaskbarNeedsApply = true;
            return;
        }

        void apply()
        {
            if (InternalWindow?.Native?.Win32?.Hwnd is nint h && h != nint.Zero)
            {
                try
                {
                    var exStyle = GetWindowLongPtr(h, GWL_EXSTYLE);

                    if (VisibleInTaskbar)
                    {
                        exStyle &= ~WS_EX_TOOLWINDOW;
                        exStyle |= WS_EX_APPWINDOW;
                    }
                    else
                    {
                        exStyle |= WS_EX_TOOLWINDOW;
                        exStyle &= ~WS_EX_APPWINDOW;
                    }

                    SetWindowLongPtr(h, GWL_EXSTYLE, exStyle);
                }
                catch { /* do not crash */ }
            }
        }

        if (IsOnPumpThread)
        {
            apply();
        }
        else if (!_closeRequested)
        {
            try { Invoke(apply); }
            catch { /* do not crash */ }
        }

        _visibleInTaskbarNeedsApply = false;
    }

    #endregion

    #region Marshalling

    internal void Invoke(Action action)
    {
        if (IsOnPumpThread)
        {
            action();
            return;
        }

        if (_closeRequested)
            return;

        using var done = new ManualResetEventSlim(false);
        Exception? ex = null;

        _workQueue.Enqueue(() =>
        {
            try
            {
                action();
            }

            catch (Exception e)
            {
                ex = e;
            }

            finally
            {
                done.Set();
            }
        });

        try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
        catch { /* do not crash */ }

        if (!done.Wait(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("HandyWindow pump thread did not process the work item.");

        if (ex is not null)
            throw ex;
    }

    internal T Invoke<T>(Func<T> func)
    {
        if (IsOnPumpThread)
            return func();

        T result = default!;
        Invoke(() =>
        {
            result = func();
        });

        return result;
    }

    #endregion

    #region Running Functions

    public void WaitForInitialization(int timeoutMilliseconds = Timeout.Infinite)
    {
        if (InternalWindow is null)
            throw new InvalidOperationException("InternalWindow has not been created.");
        if (InternalWindow.IsInitialized) return;
        _initSignal.Wait(timeoutMilliseconds);
    }

    public void Initialize()
    {
        PlatformTools.EnsureSupportedPlatform();
        GlfwTool.EnsureGlfwInitialized();
        _initRequested = true;
        WaitForInitialization();
    }

    public void Run()
    {
        Initialize();
        _closedSignal.Wait();
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (InternalWindow is null || IsDisposed)
            throw new InvalidOperationException("Cannot run a disposed or uninitialized window.");

        Initialize();

        if (_closedTcs is null)
        {
            _closedTcs = new TaskCompletionSource();
            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(Close);
        }

        return _closedTcs.Task;
    }

    public void Close()
    {
        _programmaticClose = true;
        _closeRequested = true;

        try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
        catch { /* do not crash */ }
    }

    #endregion

    #region Windowing Functions

    public void Invalidate()
    {
        if (IsDisposed) return;

        _invalidateRequested = true;

        WakePumpThread();
    }

    public void SetIcon(SKImage newIcon)
    {
        ArgumentNullException.ThrowIfNull(newIcon);
        if (InternalWindow is null) throw new InvalidOperationException("Window not created.");

        Initialize();

        Invoke(() =>
        {
            using var bmp = SKBitmap.FromImage(newIcon);
            using var rgba = new SKBitmap(new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Rgba8888));
            bmp.CopyTo(rgba);

            var icon = new RawImage(rgba.Width, rgba.Height, rgba.Bytes);
            InternalWindow!.SetWindowIcon(ref icon);
        });
    }

    public void SetSizeLimits(SKSize? minSize, SKSize? maxSize)
    {
        MinSize = minSize;
        MaxSize = maxSize;
    }

    #endregion

    #region Disposal Functions

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            Close();

            if (!IsOnPumpThread)
                _closedSignal.Wait(2000);
        }

        IsDisposed = true;
    }

    ~HandyWindow() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}