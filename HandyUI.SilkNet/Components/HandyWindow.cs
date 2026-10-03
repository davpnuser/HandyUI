using HandyUI.SilkNet.Classes.Extensions;
using HandyUI.SilkNet.Classes.Helper;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Components;

public class HandyWindow : IDisposable
{
    // Public properties
    public string Title { get; set; } = "";
    public WindowBorder Border { get; set; } = WindowBorder.Resizable;
    public bool VSync { get; set; } = true;
    public bool UseDirtyRendering { get; set; } = true;

    // Other public values
    public bool IsDisposed { get; private set; }
    public readonly IWindow? InternalWindow = null;

    private SKSize? _previousSize = null;
    public SKSize? Size
    {
        get => InternalWindow is not null ? new SKSize(InternalWindow.Size.X, InternalWindow.Size.Y) : null;
        set
        {
            if (InternalWindow is not null && value is not null)
            {
                InternalWindow.Size = new Vector2D<int>((int)value.Value.Width, (int)value.Value.Height);
                _previousSize = value;
            }
        }
    }

    public SKSize? MaxSize
    {
        get;
        set
        {
            if (value == field)
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
            if (value == field)
                return;

            field = value;
            ApplySizeLimitsInternal();
        }
    } = null;

    private SKPoint? _previousPosition = null;
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
            {
                _previousPosition = Position.Value;
            }
        }
    }

    public bool IsResizable
    {
        get;
        set
        {
            field = value;

            if (!field && Size.HasValue)
            {
                _previousSize = Size.Value;
            }

            InternalWindow?.WindowBorder = field ? Border : WindowBorder.Fixed;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetParent(nint hWndChild, nint hWndNewParent);

    private readonly List<HandyWindow> _children = [];
    public HandyWindow? Parent
    {
        get;
        set
        {
            if (InternalWindow is null)
                return;

            field?._children.Remove(this);

            field = value;

            value?._children.Add(this);

            if (OperatingSystem.IsWindows() && InternalWindow.Native?.Win32?.Hwnd is nint childHwnd && childHwnd != nint.Zero)
            {
                var parentHwnd = value?.InternalWindow?.Native?.Win32?.Hwnd ?? nint.Zero;
                SetParent(childHwnd, parentHwnd);
            }
        }
    }

    // Events
    public event Action? OnClosing;
    public event Action<SKPoint>? OnMove;
    public event Action<SKSize>? OnResize;
    public event Action<WindowState>? OnStateChanged;
    public event Action<bool>? OnFocusChanged;

    // Misc
    private readonly ManualResetEventSlim _initSignal = new(false);
    private bool _moveInProgress = false;
    private bool _resizeInProgress = false;

    #region Constructor

    public HandyWindow(WindowOptions windowOptions, SKSize? minSize = null, SKSize? maxSize = null, bool AutoInitWindow = true, bool AutoInitGlfw = true)
    {
        IsMovable = true;
        IsResizable = true;

        if (AutoInitGlfw)
            GlfwTool.EnsureGlfwInitialized();

        var window = Window.Create(windowOptions);
        InternalWindow = window;

        Size = new SKSize(windowOptions.Size.X, windowOptions.Size.Y);

        #region Close handler

        window.Closing += () =>
        {
            InternalWindow.IsVisible = false;

            foreach (var child in _children.ToArray())
            {
                child.InternalWindow?.Close();
            }

            OnClosing?.Invoke();
        };

        #endregion

        #region Move handler

        window.Move += (newPos) =>
        {
            if (_moveInProgress is true)
                return;

            _moveInProgress = true;
            if (IsMovable is false && _previousPosition is not null)
                window.Position = _previousPosition.ToVector2D();
            else
                OnMove?.Invoke(newPos.ToSKPoint());

            _moveInProgress = false;
        };

        #endregion

        #region Resize handler

        window.Resize += (newSize) =>
        {
            if (_resizeInProgress is true)
                return;

            _resizeInProgress = true;

            OnResize?.Invoke(newSize.ToSKSize());
            _resizeInProgress = false;
        };

        #endregion

        #region State change handler

        window.StateChanged += (newState) =>
        {
            OnStateChanged?.Invoke(newState);
        };

        #endregion

        #region Focus change handler

        window.FocusChanged += (newFocus) =>
        {
            OnFocusChanged?.Invoke(newFocus);
        };

        #endregion

        window.Load += () =>
        {
            _initSignal.Set();
            SetSizeLimits(minSize, maxSize);
        };

        if (AutoInitWindow)
            window.Initialize();
    }

    #endregion

    #region Initialization functions

    public void WaitForInitialization(int timeoutMilliseconds = Timeout.Infinite)
    {
        if (InternalWindow is null)
            throw new InvalidOperationException("InternalWindow has not been created.");

        if (InternalWindow.IsInitialized)
            return;

        _initSignal.Wait(timeoutMilliseconds);
    }

    public void Initialize()
    {
        PlatformTools.EnsureSupportedPlatform();
        GlfwTool.EnsureGlfwInitialized();

        if (InternalWindow is not null && IsDisposed is false)
        {
            PlatformTools.EnsureSupportedPlatform();
            GlfwTool.EnsureGlfwInitialized();

            if (!InternalWindow.IsInitialized)
            {
                InternalWindow.Initialize();
                WaitForInitialization();
            }
        }
    }

    public void Run()
    {
        if (InternalWindow is not null && IsDisposed is false)
        {
            Initialize();
            InternalWindow.Run();
        }
    }

    private TaskCompletionSource<bool>? _runTaskCompletionSource;

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (InternalWindow is null || IsDisposed)
            throw new InvalidOperationException("Cannot run a disposed or uninitialized window.");

        _runTaskCompletionSource = new TaskCompletionSource<bool>();

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                InternalWindow?.Close();
            });
        }

        var windowThread = new Thread(() =>
        {
            try
            {
                Initialize();

                InternalWindow.Run();

                _runTaskCompletionSource.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _runTaskCompletionSource.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
            Name = $"HandyWindowThread_{Title}"
        };

        if (OperatingSystem.IsWindows())
        {
            windowThread.SetApartmentState(ApartmentState.STA);
        }

        windowThread.Start();

        return _runTaskCompletionSource.Task;
    }

    #endregion

    public void SetIcon(SKImage newIcon)
    {
        // Checks
        ArgumentNullException.ThrowIfNull(newIcon);
        ArgumentNullException.ThrowIfNull(InternalWindow);

        Initialize();

        // Steps
        using var bmp = SKBitmap.FromImage(newIcon);
        using var rgba = new SKBitmap(new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Rgba8888));
        bmp.CopyTo(rgba);

        var icon = new RawImage(rgba.Width, rgba.Height, rgba.Bytes);
        InternalWindow.SetWindowIcon(ref icon);
    }

    #region Size limits

    // Unsafe
    public void SetSizeLimits(SKSize? minSize, SKSize? maxSize)
    {
        MinSize = minSize;
        MaxSize = maxSize;
    }

    private void ApplySizeLimitsInternal()
    {
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

    #endregion

    #region Disposal

    // Disposal
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                InternalWindow?.Close();
                InternalWindow?.Dispose();

                MaxSize = null;
                MinSize = null;
            }

            IsDisposed = true;
        }
    }

    ~HandyWindow()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}