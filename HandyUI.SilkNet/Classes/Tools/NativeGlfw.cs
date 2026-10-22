using Silk.NET.GLFW;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Classes.Tools;

internal static unsafe class NativeGlfw
{
    #region Fields

    private static delegate* unmanaged<IntPtr> _x11Display;
    private static delegate* unmanaged<IntPtr, ulong> _x11Window;
    private static delegate* unmanaged<IntPtr> _waylandDisplay;
    private static delegate* unmanaged<IntPtr, IntPtr> _waylandSurface;

    private static readonly Lock LoadLock = new();
    private static volatile bool _initialized;

    #endregion

    #region Initialization

    private static void EnsureInitialized()
    {
        if (_initialized) return;

        lock (LoadLock)
        {
            if (_initialized) return;

            var x11Display = FindProc("glfwGetX11Display");
            var x11Window = FindProc("glfwGetX11Window");
            var waylandDisplay = FindProc("glfwGetWaylandDisplay");
            var waylandSurface = FindProc("glfwGetWaylandSurface");

            _x11Display = x11Display != 0
                ? (delegate* unmanaged<IntPtr>)x11Display
                : null;

            _x11Window = x11Window != 0
                ? (delegate* unmanaged<IntPtr, ulong>)x11Window
                : null;

            _waylandDisplay = waylandDisplay != 0
                ? (delegate* unmanaged<IntPtr>)waylandDisplay
                : null;

            _waylandSurface = waylandSurface != 0
                ? (delegate* unmanaged<IntPtr, IntPtr>)waylandSurface
                : null;

            _initialized = true;
        }
    }

    private static nint FindProc(string name)
    {
        var addr = LookupInProcess(name);
        if (addr != 0) return addr;

        addr = LookupViaGlfwProc(name);
        return addr != 0 ? addr : LookupViaDlOpen(name);
    }

    private static nint LookupInProcess(string name)
    {
        try
        {
            if (NativeLibrary.TryGetExport(NativeLibrary.GetMainProgramHandle(), name, out var addr) && addr != 0)
                return addr;
        }
        catch { }

        return 0;
    }

    private static nint LookupViaGlfwProc(string name)
    {
        try
        {
            var glfw = Glfw.GetApi();
            var addr = glfw.GetProcAddress(name);
            if (addr != 0) return addr;
        }
        catch { }

        return 0;
    }

    private static nint LookupViaDlOpen(string name)
    {
        ReadOnlySpan<string> candidates =
        [
            "libglfw.so.3", "libglfw.so", "glfw3", "glfw",
            "libglfw.3.dylib", "libglfw.dylib", "glfw3.dll", "glfw.dll"
        ];

        foreach (var candidate in candidates)
        {
            try
            {
                if (NativeLibrary.TryLoad(candidate, out var handle)
                    && NativeLibrary.TryGetExport(handle, name, out var addr)
                    && addr != 0)
                {
                    return addr;
                }
            }
            catch { }
        }

        return 0;
    }

    #endregion

    #region Public Accessors

    public static IntPtr GetX11Display()
    {
        EnsureInitialized();
        if (_x11Display == null) return IntPtr.Zero;

        try { return _x11Display(); }
        catch { return IntPtr.Zero; }
    }

    public static ulong GetX11Window(IntPtr glfwWindow)
    {
        EnsureInitialized();
        if (_x11Window == null || glfwWindow == IntPtr.Zero) return 0;

        try { return _x11Window(glfwWindow); }
        catch { return 0; }
    }

    public static IntPtr GetWaylandDisplay()
    {
        EnsureInitialized();
        if (_waylandDisplay == null) return IntPtr.Zero;

        try { return _waylandDisplay(); }
        catch { return IntPtr.Zero; }
    }

    public static IntPtr GetWaylandSurface(IntPtr glfwWindow)
    {
        EnsureInitialized();
        if (_waylandSurface == null || glfwWindow == IntPtr.Zero) return IntPtr.Zero;

        try { return _waylandSurface(glfwWindow); }
        catch { return IntPtr.Zero; }
    }

    #endregion
}