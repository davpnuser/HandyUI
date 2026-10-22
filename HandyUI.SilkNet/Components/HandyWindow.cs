using HandyUI.SilkNet.Classes.Extensions;
using HandyUI.SilkNet.Classes.Helper;
using HandyUI.SilkNet.Classes.Records;
using HandyUI.SilkNet.Classes.Tools;
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
    internal GCHandle ClickThroughHookHandle { get; set; }

    #endregion

    #region Properties

    public string Title
    {
        get => InternalWindow is null ? "" : InternalWindow.Title;
        set
        {
            if (InternalWindow is null) return;
            InternalWindow.Title = value;
        }
    }

    public WindowBorder Border
    {
        get => InternalWindow is null ? WindowBorder.Hidden : InternalWindow.WindowBorder;
        set
        {
            if (InternalWindow is null) return;
            InternalWindow.WindowBorder = value;
        }
    }

    public WindowState State
    {
        get => InternalWindow is null ? WindowState.Normal : InternalWindow.WindowState;
        set
        {
            if (InternalWindow is null) return;
            InternalWindow.WindowState = value;
        }
    }

    public bool VSync
    {
        get => InternalWindow is not null && InternalWindow.VSync;
        set
        {
            if (InternalWindow is null) return;
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
            if (field == value) return;
            field = value;
            _opacityNeedsApply = true;
        }
    }

    public SKSize? MaxSize
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ApplySizeLimitsInternal();
        }
    } = null;

    public SKSize? MinSize
    {
        get;
        set
        {
            if (field == value) return;
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
            if (field == value) return;
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
            if (field == value) return;
            field = value;
            ApplyWindowButtons();
        }
    } = true;

    public bool IsMinimizable
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ApplyWindowButtons();
        }
    } = true;

    public SKImage? Icon
    {
        get;
        set
        {
            if (field == value) return;
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

    public bool InvalidateParentOnMove { get; set; } = true;

    private volatile bool _isClickThroughAuto;
    private bool _isClickThroughApplied;
    private bool _clickThroughHookInstalled;

    public bool IsClickThrough
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PostIfNeeded(ApplyEffectiveClickThrough);
        }
    }

    public bool AutoClickThrough { get; set; }

    public Func<SKPoint, bool>? ClickThroughTest { get; set; }

    private bool _isDragging;
    private bool _dragInFlight;
    private DateTime _dragStartedAt;

    public bool IsDragging
    {
        get => _isDragging;
        set
        {
            if (value == _isDragging) return;
            _isDragging = value;
            if (value) StartDragInternal();
        }
    }

    private void StartDragInternal()
    {
        var window = InternalWindow;
        if (IsDisposed || window is null || _closeRequested) { _isDragging = false; return; }

        if (_dragInFlight && (DateTime.UtcNow - _dragStartedAt).TotalMilliseconds < 400)
        {
            _isDragging = false;
            return;
        }

        if (IsOnPumpThread)
        {
            try
            {
                SyscallHelper.TryStartDrag(window);
                _dragInFlight = true;
                _dragStartedAt = DateTime.UtcNow;
            }
            catch { /* do not crash */ }
            finally { _isDragging = false; }
        }
        else
        {
            Post(() =>
            {
                try
                {
                    SyscallHelper.TryStartDrag(window);
                    _dragInFlight = true;
                    _dragStartedAt = DateTime.UtcNow;
                }
                catch { /* do not crash */ }
                finally { _isDragging = false; }
            });
        }
    }

    internal nint? HandleNcHitTest(SKPoint clientPos)
    {
        if (IsClickThrough) return SyscallHelper.HTTRANSPARENT;
        if (!AutoClickThrough) return null;

        var test = ClickThroughTest;
        if (test is null) return null;

        var swallows = test(clientPos);
        return swallows ? null : SyscallHelper.HTTRANSPARENT;
    }

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
    private int _moveTick;
    private bool _moveInProgress;
    private int _resizeTick;
    private bool _resizeInProgress;

    private readonly bool _centerOnStartup;

    #endregion

    #region Constructor

    public HandyWindow(WindowConfiguration windowConfiguration)
    {
        PlatformTool.EnsureSupportedPlatform();

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

            OnLoad?.Invoke();
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

            if (_dragInFlight
                && (DateTime.UtcNow - _dragStartedAt).TotalSeconds < 5
                && !OperatingSystem.IsWindows())
            {
                try { SyscallHelper.TryCancelDrag(window); }
                catch { /* do not crash */ }
            }
            _dragInFlight = false;

            if (_isClickThroughApplied && !OperatingSystem.IsWindows())
            {
                try { SyscallHelper.ApplyClickThrough(window, false); }
                catch { /* do not crash */ }
                _isClickThroughApplied = false;
            }

            foreach (var child in _children.ToArray())
            {
                if (child.InternalWindow is not null)
                {
                    try { SyscallHelper.ApplyParent(child.InternalWindow, null); }
                    catch { /* do not crash */ }
                }
                child.SubwindowParent = null;
            }

            window.IsVisible = false;
            _closeRequested = true;

            if (ModalParent is not null)
                ModalParent = null;

            _modalChild?.InternalWindow?.Close();

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
            _dragInFlight = false;

            if (InvalidateParentOnMove)
                InvalidateParent();

            if (_moveInProgress) return;
            _moveInProgress = true;

            try
            {
                if (!IsMovable && _previousPosition is not null)
                    window.Position = _previousPosition.ToVector2D();
                else
                    OnMove?.Invoke(newPos.ToSKPoint());
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

            if (_resizeInProgress) return;
            _resizeInProgress = true;

            try { OnResize?.Invoke(newSize.ToSKSize()); }
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

        #region Loop Pumping

        var vsync = windowOptions.VSync;
        var targetFps = windowOptions.FramesPerSecond;

        var glfw = Silk.NET.GLFW.Glfw.GetApi();

        const double IdleRefreshSeconds = 1.0 / 5.0;
        const double ClickThroughPollSeconds = 1.0 / 30.0;

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
                    catch { /* do not crash */ }
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

                if (!_clickThroughHookInstalled && OperatingSystem.IsWindows())
                {
                    try
                    {
                        SyscallHelper.InstallClickThroughHook(this, window);
                        _clickThroughHookInstalled = true;
                    }
                    catch { /* do not crash */ }
                }

                try { window.DoEvents(); }
                catch { /* do not crash */ }

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
                    var iw = InternalWindow;
                    if (iw is not null)
                        WindowHelper.TrySetOpacity(iw, Opacity);
                }

                if (IsInitialized && _modalStateNeedsApply)
                    ApplyModalState();

                if (IsInitialized && _visibleInTaskbarNeedsApply)
                    ApplyVisibleInTaskbar();

                if (IsInitialized && _iconNeedsApply)
                    ApplyIcon();

                if (IsInitialized && _windowButtonsNeedApply)
                    ApplyWindowButtons();

                if (_invalidateRequested)
                {
                    _invalidateRequested = false;
                    RendererAdapter?.Invalidate();
                }

                #endregion

                #region Auto Click-Through (Non-Windows)

                var test = ClickThroughTest;
                if (IsInitialized && AutoClickThrough && test is not null
                    && !OperatingSystem.IsWindows())
                {
                    if (SyscallHelper.TryGetPointerClientPosition(window, out var ptrPos))
                    {
                        var swallows = test(ptrPos);
                        var shouldClickThrough = !swallows;
                        if (_isClickThroughAuto != shouldClickThrough)
                        {
                            _isClickThroughAuto = shouldClickThrough;
                            ApplyEffectiveClickThrough();
                        }
                    }
                }

                #endregion

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
                            var effectiveWait = (test is not null
                                                 && !OperatingSystem.IsWindows())
                                ? ClickThroughPollSeconds
                                : IdleRefreshSeconds;

                            if (_invalidateRequested) glfw.PollEvents();
                            else glfw.WaitEventsTimeout(effectiveWait);
                        }
                        catch { /* do not crash */ }
                    }
                }
            }
        }
        finally
        {
            _programmaticClose = true;
            _closeRequested = true;

            if (_clickThroughHookInstalled)
            {
                try { SyscallHelper.UninstallClickThroughHook(this, window); }
                catch { /* do not crash */ }
                _clickThroughHookInstalled = false;
            }

            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    var display = NativeGlfw.GetX11Display();
                    if (display != IntPtr.Zero)
                    {
                        if (_isClickThroughApplied)
                        {
                            try { SyscallHelper.ApplyClickThrough(window, false); }
                            catch { /* do not crash */ }
                            _isClickThroughApplied = false;
                        }
                        SyscallHelper.FlushX11(display);
                    }
                }
                catch { /* do not crash */ }
            }

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

    private void ApplyEffectiveClickThrough()
    {
        var iw = InternalWindow;
        if (iw is null || !_isInitialized || _closeRequested) return;

        var effective = IsClickThrough;
        if (!OperatingSystem.IsWindows())
            effective = effective || _isClickThroughAuto;

        try
        {
            SyscallHelper.ApplyClickThrough(iw, effective);
            _isClickThroughApplied = effective;
        }
        catch { /* do not crash */ }
    }

    private void PostIfNeeded(Action action)
    {
        if (IsOnPumpThread) action();
        else Post(action);
    }

    private void InvalidateParent()
    {
        if (OperatingSystem.IsWindows() && InvalidateParentOnMove
            && SubwindowParent?.InternalWindow is { Native.Win32.Hwnd: nint parentHwnd }
            && parentHwnd != 0)
        {
            SyscallHelper.InvalidateWindow(SubwindowParent.InternalWindow, false);
        }

        SubwindowParent?.Invalidate();
    }

    private void ApplyParentToWindow(HandyWindow? value)
    {
        var iw = InternalWindow;
        if (iw is null) return;
        SyscallHelper.ApplyParent(iw, value?.InternalWindow);
        if (value?.InternalWindow is not null)
            SyscallHelper.ApplyClipChildren(value.InternalWindow);
    }

    private static void WakePumpThread()
    {
        try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
        catch { /* do not crash */ }
    }

    private void RenderNow()
    {
        if (IsDisposed || !_isInitialized) return;

        var window = InternalWindow;
        var adapter = RendererAdapter;
        if (window is null || adapter is null) return;
        if (!IsOnPumpThread) return;

        try
        {
            adapter.Invalidate();
            adapter.RenderFrame();
            window.GLContext?.SwapBuffers();
        }
        catch { /* do not crash */ }
    }

    private void ApplySizeLimitsInternal()
    {
        Initialize();

        if (InternalWindow?.Native?.Glfw is not nint rawHandle || rawHandle == 0)
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

    private void ApplyModalState()
    {
        if (IsDisposed) return;

        var iw = InternalWindow;
        if (!_isInitialized || iw is null)
        {
            _modalStateNeedsApply = true;
            return;
        }

        void apply()
        {
            try { SyscallHelper.SetEnabled(iw, _modalChild is null); }
            catch { /* do not crash */ }
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

    private void ApplyVisibleInTaskbar()
    {
        if (IsDisposed) return;

        var iw = InternalWindow;
        if (!_isInitialized || iw is null)
        {
            _visibleInTaskbarNeedsApply = true;
            return;
        }

        void apply()
        {
            try { SyscallHelper.ApplyVisibleInTaskbar(iw, VisibleInTaskbar); }
            catch { /* do not crash */ }
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

    private void ApplyIcon()
    {
        if (IsDisposed) return;
        if (!_isInitialized)
        {
            _iconNeedsApply = true;
            return;
        }

        void apply()
        {
            var icon = Icon;
            var iw = InternalWindow;
            if (icon is null || iw is null) return;

            using var bmp = SKBitmap.FromImage(icon);
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

            var rawIcon = new RawImage(rgba.Width, rgba.Height, rgba.Bytes);
            iw.SetWindowIcon(ref rawIcon);
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

        _iconNeedsApply = false;
    }

    private void ApplyWindowButtons()
    {
        if (IsDisposed) return;

        var iw = InternalWindow;
        if (!_isInitialized || iw is null)
        {
            _windowButtonsNeedApply = true;
            return;
        }

        void apply()
        {
            try
            {
                SyscallHelper.ApplyMinimizeBox(iw, IsMinimizable);
                SyscallHelper.ApplyMaximizeBox(iw, IsMaximizable);
            }
            catch { /* do not crash */ }
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

        _windowButtonsNeedApply = false;
    }

    #endregion

    #region Marshalling

    internal void Post(Action action)
    {
        if (IsDisposed || _closeRequested) return;
        _workQueue.Enqueue(action);
        try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
        catch { /* do not crash */ }
    }

    internal void Invoke(Action action)
    {
        if (IsOnPumpThread)
        {
            action();
            return;
        }

        if (_closeRequested) return;

        using var done = new ManualResetEventSlim(false);
        Exception? ex = null;

        _workQueue.Enqueue(() =>
        {
            try { action(); }
            catch (Exception e) { ex = e; }
            finally { done.Set(); }
        });

        try { Silk.NET.GLFW.Glfw.GetApi().PostEmptyEvent(); }
        catch { /* do not crash */ }

        if (!done.Wait(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("HandyWindow pump thread did not process the work item.");

        if (ex is not null) throw ex;
    }

    internal T Invoke<T>(Func<T> func)
    {
        if (IsOnPumpThread) return func();

        T result = default!;
        Invoke(() => { result = func(); });
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