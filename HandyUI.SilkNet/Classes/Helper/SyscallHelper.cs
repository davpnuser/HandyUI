using HandyUI.SilkNet.Classes.Tools;
using HandyUI.SilkNet.Components;
using Silk.NET.GLFW;
using Silk.NET.Windowing;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Classes.Helper;

public static class SyscallHelper
{
    #region Constants

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    private const nint WS_CLIPCHILDREN = 0x02000000;
    private const nint WS_MINIMIZEBOX = 0x00020000;
    private const nint WS_MAXIMIZEBOX = 0x00010000;

    private const nint WS_EX_TOOLWINDOW = 0x00000080;
    private const nint WS_EX_APPWINDOW = 0x00040000;
    private const nint WS_EX_TRANSPARENT = 0x00000020;

    private const uint WM_NCLBUTTONDOWN = 0x00A1;
    private const nint HTCAPTION = 2;
    internal const nint HTTRANSPARENT = -1;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

    private const int ShapeInput = 2;
    private const int ShapeSet = 0;
    private const int Unsorted = 0;
    private const int MoveResizeCancel = 11;

    private const int XA_ATOM = 4;
    private const int PropModeReplace = 0;
    private const int ClientMessage = 33;
    private const long SubstructureNotifyMask = 0x00040000L;
    private const long SubstructureRedirectMask = 0x00080000L;

    #endregion

    #region Win32 P/Invoke

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetParent(nint hWndChild, nint hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnableWindow(nint hWnd, bool bEnable);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool InvalidateRect(nint hWnd, nint lpRect, bool bErase);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter,
                                            int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ScreenToClient(nint hWnd, ref POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(nint hWnd, SubclassProc pfnSubclass,
                                                 nuint uIdSubclass, nint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(nint hWnd, SubclassProc pfnSubclass,
                                                    nuint uIdSubclass);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern nint DefSubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint SubclassProc(nint hWnd, uint uMsg, nint wParam,
                                       nint lParam, nuint uIdSubclass, nint dwRefData);

    #endregion

    #region X11 P/Invoke

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    [DllImport("libX11.so.6", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr XInternAtom(IntPtr display,
                                             [MarshalAs(UnmanagedType.LPStr)] string name,
                                             bool onlyIfExists);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XChangeProperty(IntPtr display, ulong window, IntPtr property,
                                              IntPtr type, int format, int mode, IntPtr data, int nelements);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XSendEvent(IntPtr display, ulong window, bool propagate,
                                         long eventMask, IntPtr sendEvent);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XReparentWindow(IntPtr display, ulong window, ulong parent, int x, int y);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XClearArea(IntPtr display, ulong window,
                                         int x, int y, uint width, uint height, bool exposures);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XFlush(IntPtr display);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XSync(IntPtr display, bool discard);

    [DllImport("libX11.so.6", SetLastError = true)]
    private static extern int XQueryPointer(IntPtr display, ulong window,
                                            out ulong rootReturn, out ulong childReturn,
                                            out int rootX, out int rootY,
                                            out int winX, out int winY, out uint maskReturn);

    [DllImport("libXext.so.6", SetLastError = true)]
    private static extern void XShapeCombineRectangles(IntPtr display, ulong window,
                                                       int destKind, int xOff, int yOff,
                                                       IntPtr rectangles, int nRects, int op, int ordering);

    [DllImport("libXext.so.6", SetLastError = true)]
    private static extern void XShapeCombineMask(IntPtr display, ulong window,
                                                 int destKind, int xOff, int yOff, ulong src, int op);

    #endregion

    #region Win32 Wrappers

    private static nint GetWindowLongPtr(nint hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
    }

    private static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
    {
        return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                                : SetWindowLong32(hWnd, nIndex, (int)dwNewLong);
    }

    #endregion

    #region X11 Helpers

    private static IntPtr GetX11Display()
    {
        return NativeGlfw.GetX11Display();
    }

    private static ulong GetX11Window(IWindow window)
    {
        return window.Native?.Glfw is nint raw && raw != 0 ? NativeGlfw.GetX11Window(raw) : 0;
    }

    private static unsafe int XSendClientMessage(IntPtr display, ulong targetWindow, long eventMask,
                                                 ulong window, IntPtr messageType, params long[] data)
    {
        var eventSize = IntPtr.Size * 24;
        var ev = Marshal.AllocHGlobal(eventSize);
        try
        {
            unsafe { var span = new Span<byte>((void*)ev, eventSize); span.Clear(); }

            if (IntPtr.Size == 8)
            {
                Marshal.WriteInt32(ev, 0, ClientMessage);
                Marshal.WriteInt64(ev, 8, 0);
                Marshal.WriteInt32(ev, 16, 1);
                Marshal.WriteInt64(ev, 24, display.ToInt64());
                Marshal.WriteInt64(ev, 32, (long)window);
                Marshal.WriteInt64(ev, 40, messageType.ToInt64());
                Marshal.WriteInt32(ev, 48, 32);
                for (var i = 0; i < Math.Min(data.Length, 5); i++)
                    Marshal.WriteInt64(ev, 56 + (i * 8), data[i]);
            }
            else
            {
                Marshal.WriteInt32(ev, 0, ClientMessage);
                Marshal.WriteInt32(ev, 4, 0);
                Marshal.WriteInt32(ev, 8, 1);
                Marshal.WriteInt32(ev, 12, display.ToInt32());
                Marshal.WriteInt32(ev, 16, (int)window);
                Marshal.WriteInt32(ev, 20, messageType.ToInt32());
                Marshal.WriteInt32(ev, 24, 32);
                for (var i = 0; i < Math.Min(data.Length, 5); i++)
                    Marshal.WriteInt32(ev, 28 + (i * 4), (int)data[i]);
            }

            return XSendEvent(display, targetWindow, false, eventMask, ev);
        }
        finally
        {
            Marshal.FreeHGlobal(ev);
        }
    }

    #endregion

    #region Public API

    public static void ApplyClipChildren(IWindow? window)
    {
        if (PlatformTool.DetectPlatform(window) != WindowPlatform.Windows) return;
        if (window?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;

        var currentStyle = GetWindowLongPtr(hwnd, GWL_STYLE);
        _ = SetWindowLongPtr(hwnd, GWL_STYLE, currentStyle | WS_CLIPCHILDREN);
    }

    public static void ApplyParent(IWindow? child, IWindow? parent)
    {
        if (child is null) return;

        switch (PlatformTool.DetectPlatform(child))
        {
            case WindowPlatform.Windows:
                if (child.Native?.Win32?.Hwnd is not nint childHwnd || childHwnd == 0) return;
                var parentHwnd = parent?.Native?.Win32?.Hwnd ?? 0;
                _ = SetParent(childHwnd, parentHwnd);
                break;

            case WindowPlatform.X11:
                var display = GetX11Display();
                var childX11 = GetX11Window(child);
                if (display == IntPtr.Zero || childX11 == 0) return;

                var parentX11 = parent is not null ? GetX11Window(parent) : 0;
                if (parentX11 == 0) parentX11 = (ulong)XDefaultRootWindow(display).ToInt64();

                _ = XReparentWindow(display, childX11, parentX11, 0, 0);
                _ = XSync(display, false);
                break;
        }
    }

    public static void SetEnabled(IWindow? window, bool enabled)
    {
        if (PlatformTool.DetectPlatform(window) != WindowPlatform.Windows) return;
        if (window?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;
        _ = EnableWindow(hwnd, enabled);
    }

    public static void InvalidateWindow(IWindow? window, bool erase = false)
    {
        if (window is null) return;

        switch (PlatformTool.DetectPlatform(window))
        {
            case WindowPlatform.Windows:
                if (window.Native?.Win32?.Hwnd is nint hwnd && hwnd != 0)
                    _ = InvalidateRect(hwnd, 0, erase);
                break;

            case WindowPlatform.X11:
                var display = GetX11Display();
                var x11 = GetX11Window(window);
                if (display != IntPtr.Zero && x11 != 0)
                {
                    _ = XClearArea(display, x11, 0, 0, 0, 0, true);
                    _ = XFlush(display);
                }
                break;
        }
    }

    public static void WakeWindow(IWindow? window)
    {
        if (window is null) return;
        try { Glfw.GetApi().PostEmptyEvent(); }
        catch { }
    }

    public static void ApplyVisibleInTaskbar(IWindow? window, bool visibleInTaskbar)
    {
        if (window is null) return;

        switch (PlatformTool.DetectPlatform(window))
        {
            case WindowPlatform.Windows:
                if (window.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;

                var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
                if (visibleInTaskbar)
                {
                    exStyle &= ~WS_EX_TOOLWINDOW;
                    exStyle |= WS_EX_APPWINDOW;
                }
                else
                {
                    exStyle |= WS_EX_TOOLWINDOW;
                    exStyle &= ~WS_EX_APPWINDOW;
                }
                _ = SetWindowLongPtr(hwnd, GWL_EXSTYLE, exStyle);
                break;

            case WindowPlatform.X11:
                var display = GetX11Display();
                var x11 = GetX11Window(window);
                if (display == IntPtr.Zero || x11 == 0) return;

                var wmState = XInternAtom(display, "_NET_WM_STATE", false);
                if (visibleInTaskbar)
                {
                    _ = XChangeProperty(display, x11, wmState, XA_ATOM, 32,
                                        PropModeReplace, IntPtr.Zero, 0);
                }
                else
                {
                    var skipTaskbar = XInternAtom(display, "_NET_WM_STATE_SKIP_TASKBAR", false);
                    var skipPager = XInternAtom(display, "_NET_WM_STATE_SKIP_PAGER", false);

                    var atoms = new long[] { skipTaskbar.ToInt64(), skipPager.ToInt64() };
                    var ptr = Marshal.AllocHGlobal(IntPtr.Size * 2);
                    try
                    {
                        if (IntPtr.Size == 8)
                        {
                            Marshal.WriteInt64(ptr, 0, atoms[0]);
                            Marshal.WriteInt64(ptr, 8, atoms[1]);
                        }
                        else
                        {
                            Marshal.WriteInt32(ptr, 0, (int)atoms[0]);
                            Marshal.WriteInt32(ptr, 4, (int)atoms[1]);
                        }
                        _ = XChangeProperty(display, x11, wmState, XA_ATOM, 32,
                                            PropModeReplace, ptr, 2);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
                _ = XFlush(display);
                break;
        }
    }

    public static void ApplyMinimizeBox(IWindow? window, bool enabled)
    {
        if (PlatformTool.DetectPlatform(window) != WindowPlatform.Windows) return;
        if (window?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;

        var style = GetWindowLongPtr(hwnd, GWL_STYLE);
        if (enabled) style |= WS_MINIMIZEBOX;
        else style &= ~WS_MINIMIZEBOX;
        _ = SetWindowLongPtr(hwnd, GWL_STYLE, style);
        _ = SetWindowPos(hwnd, 0, 0, 0, 0, 0,
                         SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    public static void ApplyMaximizeBox(IWindow? window, bool enabled)
    {
        if (PlatformTool.DetectPlatform(window) != WindowPlatform.Windows) return;
        if (window?.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;

        var style = GetWindowLongPtr(hwnd, GWL_STYLE);
        if (enabled) style |= WS_MAXIMIZEBOX;
        else style &= ~WS_MAXIMIZEBOX;
        _ = SetWindowLongPtr(hwnd, GWL_STYLE, style);
        _ = SetWindowPos(hwnd, 0, 0, 0, 0, 0,
                         SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    public static void ApplyClickThrough(IWindow? window, bool enabled)
    {
        if (window is null) return;

        switch (PlatformTool.DetectPlatform(window))
        {
            case WindowPlatform.Windows:
                if (window.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;

                var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
                if (enabled) exStyle |= WS_EX_TRANSPARENT;
                else exStyle &= ~WS_EX_TRANSPARENT;
                _ = SetWindowLongPtr(hwnd, GWL_EXSTYLE, exStyle);
                break;

            case WindowPlatform.X11:
                var display = GetX11Display();
                var x11 = GetX11Window(window);
                if (display == IntPtr.Zero || x11 == 0) return;

                if (enabled)
                    XShapeCombineMask(display, x11, ShapeInput, 0, 0, 0, ShapeSet);
                else
                {
                    XShapeCombineRectangles(display, x11, ShapeInput, 0, 0,
                                            IntPtr.Zero, 0, ShapeSet, Unsorted);
                }

                _ = XSync(display, false);
                break;
        }
    }

    public static void TryStartDrag(IWindow? window)
    {
        if (window is null) return;

        switch (PlatformTool.DetectPlatform(window))
        {
            case WindowPlatform.Windows:
                if (window.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;
                _ = ReleaseCapture();
                _ = SendMessage(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                break;

            case WindowPlatform.X11:
                var display = GetX11Display();
                var x11 = GetX11Window(window);
                if (display == IntPtr.Zero || x11 == 0) return;

                _ = XQueryPointer(display, x11, out _, out _,
                                  out var rootX, out var rootY,
                                  out _, out _, out _);

                var root = (ulong)XDefaultRootWindow(display).ToInt64();
                var moveAtom = XInternAtom(display, "_NET_WM_MOVERESIZE", false);
                if (moveAtom == IntPtr.Zero) return;

                _ = XSendClientMessage(display, root,
                                       SubstructureNotifyMask | SubstructureRedirectMask,
                                       x11, moveAtom,
                                       rootX, rootY, 8, 1, 1);
                _ = XSync(display, false);
                break;
        }
    }

    public static void TryCancelDrag(IWindow? window)
    {
        if (window is null) return;
        if (PlatformTool.DetectPlatform(window) != WindowPlatform.X11) return;

        var display = GetX11Display();
        var x11 = GetX11Window(window);
        if (display == IntPtr.Zero || x11 == 0) return;

        var root = (ulong)XDefaultRootWindow(display).ToInt64();
        var moveAtom = XInternAtom(display, "_NET_WM_MOVERESIZE", false);
        if (moveAtom == IntPtr.Zero) return;

        _ = XSendClientMessage(display, root,
                               SubstructureNotifyMask | SubstructureRedirectMask,
                               x11, moveAtom,
                               0, 0, MoveResizeCancel, 0, 0);
        _ = XFlush(display);
    }

    public static bool TryGetPointerClientPosition(IWindow? window, out SKPoint position)
    {
        position = default;
        if (window is null) return false;
        if (PlatformTool.DetectPlatform(window) != WindowPlatform.X11) return false;

        var display = GetX11Display();
        var x11 = GetX11Window(window);
        if (display == IntPtr.Zero || x11 == 0) return false;

        var sameScreen = XQueryPointer(display, x11, out _, out _,
                                       out _, out _,
                                       out var winX, out var winY, out _);
        if (sameScreen == 0) return false;

        position = new SKPoint(winX, winY);
        return true;
    }

    public static void FlushX11(IntPtr display)
    {
        if (display == IntPtr.Zero) return;
        try { _ = XSync(display, false); }
        catch { }
    }

    #endregion

    #region Click-Through Subclass (Windows)

    private static SubclassProc? _subclassProc;

    public static void InstallClickThroughHook(HandyWindow window, IWindow iWindow)
    {
        if (PlatformTool.DetectPlatform(iWindow) != WindowPlatform.Windows) return;
        if (iWindow.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;
        if (window.ClickThroughHookHandle.IsAllocated) return;

        _subclassProc ??= ClickThroughSubclassProc;

        var handle = GCHandle.Alloc(window);
        try
        {
            if (!SetWindowSubclass(hwnd, _subclassProc, 1, GCHandle.ToIntPtr(handle)))
            {
                handle.Free();
                return;
            }
            window.ClickThroughHookHandle = handle;
        }
        catch
        {
            handle.Free();
            throw;
        }
    }

    public static void UninstallClickThroughHook(HandyWindow window, IWindow iWindow)
    {
        if (PlatformTool.DetectPlatform(iWindow) != WindowPlatform.Windows) return;
        if (!window.ClickThroughHookHandle.IsAllocated) return;
        if (iWindow.Native?.Win32?.Hwnd is not nint hwnd || hwnd == 0) return;
        if (_subclassProc is null) return;

        try { _ = RemoveWindowSubclass(hwnd, _subclassProc, 1); }
        catch { }

        window.ClickThroughHookHandle.Free();
    }

    private static nint ClickThroughSubclassProc(nint hWnd, uint uMsg, nint wParam,
                                                 nint lParam, nuint uIdSubclass, nint dwRefData)
    {
        if (uMsg == 0x0084 && dwRefData != 0)
        {
            var handle = GCHandle.FromIntPtr(dwRefData);
            if (handle.Target is HandyWindow window)
            {
                var screenX = (short)(lParam.ToInt32() & 0xFFFF);
                var screenY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                var pt = new POINT { X = screenX, Y = screenY };

                if (ScreenToClient(hWnd, ref pt))
                {
                    var result = window.HandleNcHitTest(new SKPoint(pt.X, pt.Y));
                    if (result.HasValue) return result.Value;
                }
            }
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    #endregion
}