using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Extensions;
using HandyUI.SilkNet.Helper;
using HandyUI.SilkNet.Tool;
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
    internal RendererAdapter? RendererAdapter { get; set; }

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
                InternalWindow.Size = new Vector2D<int>((int)value.Value.Width, (int)value.Value.Height);
        }
    }

    public float Opacity
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            _opacityNeedsApply = true;
        }
    }

    public SKSize? MaxSize
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            ApplySizeLimitsInternal();
        }
    } = null;

    public SKSize? MinSize
    {
        get;
        set
        {
            if (field == value)
                return;

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

    public bool VisibleInTaskbar
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            ApplyVisibleInTaskbar();
        }
    } = true;

    public bool IsResizable
    {
        get;
        set
        {
            field = value;
            InternalWindow?.WindowBorder = field ? Border : WindowBorder.Fixed;
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

    public bool IsCloseable { get; set; } = true;

    public bool IsMaximizable
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            ApplyWindowButtons();
        }
    } = true;

    public bool IsMinimizable
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            ApplyWindowButtons();
        }
    } = true;

    public SKImage? Icon
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            _iconNeedsApply = true;
        }
    }

    public bool TopMost
    {
        get => InternalWindow?.TopMost ?? false;
        set => InternalWindow?.TopMost = value;
    }

    private readonly List<HandyWindow> _children = [];

    public HandyWindow? SubwindowParent
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

            if (value is not null && value == SubwindowParent)
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
                try { Win32Helper.SetEnabled(h, _modalChild is null); }
                catch { }
            }
        }

        if (IsOnPumpThread)
        {
            apply();
        }
        else if (!_closeRequested)
        {
            try { Invoke(apply); }
            catch { }
        }

        _modalStateNeedsApply = false;
    }

    public bool InvalidateParentOnMove { get; set; } = true;

    public bool IsDragging
    {
        get => _isDragging;
        set
        {
            if (_isDragging == value || IsDisposed) return;

            _isDragging = value;
            _dragOffset = null;

            if (value)
                Invalidate();
        }
    }

    public bool IsHidden => _isHidden;

    public TrayIcon TrayIcon
    {
        get
        {
            if (_trayIcon is null)
                _trayIcon = new TrayIcon(this);
            return _trayIcon;
        }
    }

    #endregion

    #region Events

    public event Action? WhenLoaded;
    public event Action? WhenClosing;
    public event Action<SKPoint>? WhenMoved;
    public event Action<SKSize>? WhenResized;
    public event Action<WindowState>? WhenStateChanged;
    public event Action<bool>? WhenFocusChanged;

    #endregion

    #region Internal Fields

    private volatile HandyWindow? _modalChild;
    private volatile bool _isDragging;
    private volatile bool _isHidden;
    private SKPoint? _dragOffset;
    private bool _visibleInTaskbarBeforeHide;
    private TrayIcon? _trayIcon;

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
    private volatile bool _iconNeedsApply;
    private volatile bool _opacityNeedsApply;
    private volatile bool _parentNeedsApply;
    private volatile bool _modalStateNeedsApply;
    private volatile bool _visibleInTaskbarNeedsApply;
    private volatile bool _windowButtonsNeedApply;
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
        PlatformTool.EnsureSupportedPlatform();
        ThemeHelper.EnsureInitialized();

        IsMovable = true;
        IsResizable = true;
        UseDirtyRendering = windowConfiguration.UseDirtyRendering;
        _autoInit = windowConfiguration.AutoInitWindow;
        InvalidateParentOnMove = windowConfiguration.InvalidateParentOnMove;
        Opacity = windowConfiguration.Opacity;
        _centerOnStartup = windowConfiguration.IsCenteredOnStartup;

        VisibleInTaskbar = windowConfiguration.IsVisibleInTaskbar;
        IsResizable = windowConfiguration.IsResizable;
        IsMovable = windowConfiguration.IsMovable;
        IsCloseable = windowConfiguration.IsCloseable;
        IsMaximizable = windowConfiguration.IsMaximizable;
        IsMinimizable = windowConfiguration.IsMinimizable;
        Icon = windowConfiguration.Icon;

        if (windowConfiguration.ModalParent is not null)
            ModalParent = windowConfiguration.ModalParent;

        else if (windowConfiguration.SubwindowParent is not null)
            SubwindowParent = windowConfiguration.SubwindowParent;

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
            Name = windowConfiguration.PumpThreadName
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

            WhenLoaded?.Invoke();
        };

        _windowCreated.Set();

        #endregion

        #region Close Handler

        window.Closing += () =>
        {
            if (_closingHandled) return;

            if (!IsCloseable && !_programmaticClose)
            {
                WindowHelper.TryCancelClose(window);
                return;
            }

            _closingHandled = true;

            ThemeHelper.UnregisterWindow(this);

            window.IsVisible = false;
            _closeRequested = true;

            if (ModalParent is not null)
                ModalParent = null;

            _modalChild?.InternalWindow?.Close();

            SubwindowParent = null;

            foreach (var child in _children.ToArray())
                child.InternalWindow?.Close();

            _trayIcon?.Dispose();
            _trayIcon = null;

            WhenClosing?.Invoke();

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
                if (!IsMovable && !_isDragging && _previousPosition is not null)
                    window.Position = _previousPosition.ToVector2D();

                else
                    WhenMoved?.Invoke(newPos.ToSKPoint());
            }
            catch { _moveInProgress = false; }
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

            try { WhenResized?.Invoke(newSize.ToSKSize()); }
            catch { _resizeInProgress = false; }
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

        window.StateChanged += s => WhenStateChanged?.Invoke(s);

        window.FocusChanged += f =>
        {
            if (f && _modalChild is not null)
            {
                try { _modalChild.Invoke(() => _modalChild.InternalWindow?.Focus()); }
                catch { }
            }

            WhenFocusChanged?.Invoke(f);
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
                    catch { }
                }

                if (!window.IsInitialized)
                {
                    if (_autoInit || _initRequested)
                    {
                        try { window.Initialize(); }
                        catch { }
                    }

                    Thread.Sleep(1);
                    continue;
                }

                try { window.DoEvents(); }
                catch { }

                #region Drag Update

                if (_isDragging)
                {
                    if (_dragOffset is null)
                        TryComputeDragOffset();

                    if (_dragOffset is not null)
                        UpdateDragPosition();
                }

                #endregion

                #region Things need applying

                if (IsInitialized && _parentNeedsApply)
                {
                    var desired = SubwindowParent;

                    if (desired is null || desired.IsInitialized)
                    {
                        ApplyParentToWindow(desired);
                        _parentNeedsApply = false;
                    }
                }

                if (IsInitialized && _opacityNeedsApply)
                {
                    WindowHelper.TrySetOpacity(InternalWindow!, Opacity);
                }

                if (IsInitialized && _modalStateNeedsApply)
                {
                    ApplyModalState();
                }

                if (IsInitialized && _visibleInTaskbarNeedsApply)
                {
                    ApplyVisibleInTaskbar();
                }

                if (IsInitialized && _iconNeedsApply)
                {
                    ApplyIcon();
                }

                if (IsInitialized && _windowButtonsNeedApply)
                {
                    ApplyWindowButtons();
                }

                if (_invalidateRequested)
                {
                    _invalidateRequested = false;
                    RendererAdapter?.Invalidate();
                }

                #endregion

                var useDirty = RendererAdapter?.UseDirtyRendering ?? UseDirtyRendering;
                var invalidated = RendererAdapter?.IsInvalidated == true;

                var shouldRender = !_isHidden && (!useDirty || invalidated
                                || (frameStart - _lastFrameTime) >= IdleRefreshSeconds);

                if (shouldRender)
                {
                    try { RendererAdapter?.RenderFrame(); }
                    catch { }

                    try { window.GLContext?.SwapBuffers(); }
                    catch { }

                    _lastFrameTime = _pumpStopwatch.Elapsed.TotalSeconds;
                }

                if (_isDragging)
                {
                    try { glfw.PollEvents(); }
                    catch { }

                    Thread.Sleep(1);
                }
                else if (!useDirty)
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
                        catch { }
                    }
                }
            }
        }
        finally
        {
            _programmaticClose = true;

            try { window.Close(); }
            catch { }

            try { window.Dispose(); }
            catch { }

            InternalWindow = null;

            _closedTcs?.TrySetResult();
            _closedSignal.Set();
        }

        #endregion
    }

    #endregion

    #region Drag Helpers

    private bool TryComputeDragOffset()
    {
        if (InternalWindow is null) return false;
        if (!OperatingSystem.IsWindows()) return false;
        if (!Win32Helper.GetCursorPos(out var pt)) return false;

        var pos = InternalWindow.Position;
        _dragOffset = new SKPoint(pt.X - pos.X, pt.Y - pos.Y);

        return true;
    }

    private void UpdateDragPosition()
    {
        if (InternalWindow is null || _dragOffset is null) return;
        if (!OperatingSystem.IsWindows()) return;
        if (!Win32Helper.GetCursorPos(out var pt)) return;

        var newX = pt.X - (int)_dragOffset.Value.X;
        var newY = pt.Y - (int)_dragOffset.Value.Y;

        var pos = InternalWindow.Position;
        if (newX == pos.X && newY == pos.Y) return;

        InternalWindow.Position = new Vector2D<int>(newX, newY);
        _previousPosition = new SKPoint(newX, newY);
    }

    #endregion

    #region Internal Functions

    private void InvalidateParent()
    {
        if (OperatingSystem.IsWindows() && InvalidateParentOnMove && SubwindowParent?.InternalWindow?.Native?.Win32?.Hwnd is nint parentHwnd && parentHwnd != nint.Zero)
        {
            Win32Helper.InvalidateWindow(parentHwnd, false);
        }

        SubwindowParent?.Invalidate();
    }

    private void ApplyParentToWindow(HandyWindow? value)
    {
        if (!OperatingSystem.IsWindows()) return;

        if (InternalWindow?.Native?.Win32?.Hwnd is nint childHwnd && childHwnd != nint.Zero)
        {
            var parentHwnd = value?.InternalWindow?.Native?.Win32?.Hwnd ?? nint.Zero;
            Win32Helper.ApplyParent(childHwnd, parentHwnd);
            Win32Helper.ApplyClipChildren(value);
        }
    }

    private void WakePumpThread()
    {
        if (!OperatingSystem.IsWindows()) return;

        if (InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
        {
            try { Win32Helper.WakeWindow(hwnd); }
            catch { }
        }
    }

    private void RenderNow()
    {
        if (IsDisposed || !_isInitialized || _isHidden)
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
        catch { }
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
                try { Win32Helper.ApplyVisibleInTaskbar(h, VisibleInTaskbar); }
                catch { }
            }
        }

        if (IsOnPumpThread)
        {
            apply();
        }
        else if (!_closeRequested)
        {
            try { Invoke(apply); }
            catch { }
        }

        _visibleInTaskbarNeedsApply = false;
    }

    private void ApplyIcon()
    {
        if (IsDisposed) return;

        if (!OperatingSystem.IsWindows()) return;

        if (!_isInitialized)
        {
            _iconNeedsApply = true;
            return;
        }

        void apply()
        {
            if (Icon is null) return;

            using var bmp = SKBitmap.FromImage(Icon);

            var info = new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            using var rgba = new SKBitmap(info);
            if (!bmp.CopyTo(rgba)) return;

            var px = MemoryMarshal.Cast<byte, uint>(rgba.GetPixelSpan());
            for (var i = 0; i < px.Length; i++)
            {
                px[i] = (px[i] & 0xFF00FF00u)
                      | ((px[i] >> 16) & 0x000000FFu)
                      | ((px[i] & 0x000000FFu) << 16);
            }

            var icon = new RawImage(rgba.Width, rgba.Height, rgba.Bytes);
            InternalWindow!.SetWindowIcon(ref icon);
        }

        if (IsOnPumpThread)
        {
            apply();
        }
        else if (!_closeRequested)
        {
            try { Invoke(apply); }
            catch { }
        }

        _iconNeedsApply = false;
    }

    private void ApplyWindowButtons()
    {
        if (IsDisposed) return;

        if (!OperatingSystem.IsWindows()) return;

        if (!_isInitialized || InternalWindow?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == nint.Zero)
        {
            _windowButtonsNeedApply = true;
            return;
        }

        void apply()
        {
            if (InternalWindow?.Native?.Win32?.Hwnd is nint h && h != nint.Zero)
            {
                try
                {
                    Win32Helper.ApplyMinimizeBox(h, IsMinimizable);
                    Win32Helper.ApplyMaximizeBox(h, IsMaximizable);
                }
                catch { }
            }
        }

        if (IsOnPumpThread)
        {
            apply();
        }
        else if (!_closeRequested)
        {
            try { Invoke(apply); }
            catch { }
        }

        _windowButtonsNeedApply = false;
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
        catch { }

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
        PlatformTool.EnsureSupportedPlatform();
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
        catch { }
    }

    #endregion

    #region Windowing Functions

    public void Invalidate()
    {
        if (IsDisposed) return;

        _invalidateRequested = true;

        WakePumpThread();
    }

    public void SetSizeLimits(SKSize? minSize, SKSize? maxSize)
    {
        MinSize = minSize;
        MaxSize = maxSize;
    }

    public void BeginDrag()
    {
        if (IsDisposed) return;

        if (InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
        {
            if (IsOnPumpThread)
                Win32Helper.StartWindowDrag(hwnd);
            else
                Invoke(() => Win32Helper.StartWindowDrag(hwnd));
        }
    }

    public void Hide()
    {
        if (IsDisposed) return;

        _visibleInTaskbarBeforeHide = VisibleInTaskbar;
        VisibleInTaskbar = false;
        _isHidden = true;

        Invoke(() =>
        {
            ApplyVisibleInTaskbar();
            if (InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
                Win32Helper.ShowWindowVisible(hwnd, false);
            else
                InternalWindow!.IsVisible = false;
        });
    }

    public void Show()
    {
        if (IsDisposed) return;

        VisibleInTaskbar = _visibleInTaskbarBeforeHide;
        _isHidden = false;

        Invoke(() =>
        {
            ApplyVisibleInTaskbar();
            if (InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
                Win32Helper.ShowWindowVisible(hwnd, true);
            else
                InternalWindow!.IsVisible = true;
        });
    }

    #endregion

    #region Disposal Functions

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            ThemeHelper.UnregisterWindow(this);

            _trayIcon?.Dispose();
            _trayIcon = null;

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