using HandyUI.SilkNet.Components;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Helper;

public static class Win32Helper
{
    #region Constants

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    public const int GWLP_WNDPROC = -4;

    private const nint WS_CLIPCHILDREN = 0x02000000;
    private const nint WS_MINIMIZEBOX = 0x00020000;
    private const nint WS_MAXIMIZEBOX = 0x00010000;
    private const nint WS_CAPTION = 0x00C00000;
    private const nint WS_SYSMENU = 0x00080000;
    private const nint WS_THICKFRAME = 0x00040000;

    private const nint WS_EX_TOOLWINDOW = 0x00000080;
    private const nint WS_EX_APPWINDOW = 0x00040000;

    public const uint WM_NULL = 0x0000;
    public const uint WM_COMMAND = 0x0111;
    private const uint WM_NCLBUTTONDOWN = 0x00A1;
    private const nint HTCAPTION = 0x0002;

    public const uint WM_NCCALCSIZE = 0x0083;
    public const uint WM_NCHITTEST = 0x0084;

    public const nint HTCLIENT = 1;
    public const nint HTLEFT = 10;
    public const nint HTRIGHT = 11;
    public const nint HTTOP = 12;
    public const nint HTTOPLEFT = 13;
    public const nint HTTOPRIGHT = 14;
    public const nint HTBOTTOM = 15;
    public const nint HTBOTTOMLEFT = 16;
    public const nint HTBOTTOMRIGHT = 17;

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;

    public static readonly nint HWND_MESSAGE = new(-3);

    public const uint NIM_ADD = 0x00000000;
    public const uint NIM_MODIFY = 0x00000001;
    public const uint NIM_DELETE = 0x00000002;

    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON = 0x00000002;
    public const uint NIF_TIP = 0x00000004;
    public const uint NIF_STATE = 0x00000008;
    public const uint NIF_INFO = 0x00000010;
    public const uint NIF_GUID = 0x00000020;
    public const uint NIF_REALTIME = 0x00000040;
    public const uint NIF_SHOWTIP = 0x00000080;

    public const uint WM_USER = 0x0400;
    public const uint WM_TRAYICON = WM_USER + 1;
    public const uint WM_LBUTTONUP = 0x0202;
    public const uint WM_LBUTTONDBLCLK = 0x0203;
    public const uint WM_RBUTTONUP = 0x0205;

    public const uint MF_STRING = 0x00000000;
    public const uint MF_SEPARATOR = 0x00000800;
    public const uint MF_CHECKED = 0x00000008;
    public const uint MF_UNCHECKED = 0x00000000;
    public const uint MF_GRAYED = 0x00000001;
    public const uint MF_DISABLED = 0x00000002;
    public const uint MF_ENABLED = 0x00000000;

    public const uint TPM_LEFTALIGN = 0x0000;
    public const uint TPM_CENTERALIGN = 0x0004;
    public const uint TPM_RIGHTALIGN = 0x0008;
    public const uint TPM_TOPALIGN = 0x0000;
    public const uint TPM_VCENTERALIGN = 0x0010;
    public const uint TPM_BOTTOMALIGN = 0x0020;
    public const uint TPM_LEFTBUTTON = 0x0000;
    public const uint TPM_RIGHTBUTTON = 0x0002;
    public const uint TPM_NONOTIFY = 0x0080;
    public const uint TPM_RETURNCMD = 0x0100;

    #endregion

    #region Delegates

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public nint hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public nint hBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public nint hbmMask;
        public nint hbmColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    #region Native Methods

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
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter,
                                            int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW", SetLastError = true)]
    public static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint GetModuleHandle(string? lpModuleName);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern nint CreateBitmap(int nWidth, int nHeight, uint nPlanes, uint nBitCount, byte[]? lpBits);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(nint hObject);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateIconIndirect(ref ICONINFO piconinfo);

    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    private static extern bool DestroyIconNative(nint hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CreatePopupMenu();

    [DllImport("user32.dll", EntryPoint = "AppendMenuW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyMenu(nint hMenu);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint TrackPopupMenu(nint hMenu, uint uFlags, int x, int y, int nReserved, nint hWnd, nint prcRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetMenuDefaultItem(nint hMenu, uint uItem, uint fByPos);

    #endregion

    #region Cross-bitness Long Wrappers

    public static nint GetWindowLongPtr(nint hWnd, int nIndex)
    {
        return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, nIndex)
                : GetWindowLong32(hWnd, nIndex);
    }

    public static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
    {
        return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : SetWindowLong32(hWnd, nIndex, (int)dwNewLong);
    }

    #endregion

    #region Public Helpers

    public static void ApplyClipChildren(HandyWindow? window)
    {
        if (window?.InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
        {
            var currentStyle = GetWindowLongPtr(hwnd, GWL_STYLE);
            var newStyle = currentStyle | WS_CLIPCHILDREN;

            SetWindowLongPtr(hwnd, GWL_STYLE, newStyle);
        }
    }

    public static void ApplyParent(nint childHwnd, nint parentHwnd)
    {
        if (childHwnd == nint.Zero) return;
        SetParent(childHwnd, parentHwnd);
    }

    public static void SetEnabled(nint hWnd, bool enabled)
    {
        if (hWnd == nint.Zero) return;
        EnableWindow(hWnd, enabled);
    }

    public static void InvalidateWindow(nint hWnd, bool erase = false)
    {
        if (hWnd == nint.Zero) return;
        InvalidateRect(hWnd, nint.Zero, erase);
    }

    public static void WakeWindow(nint hWnd)
    {
        if (hWnd == nint.Zero) return;
        PostMessage(hWnd, WM_NULL, 0, 0);
    }

    public static bool SetWindowPosition(nint hWnd, int x, int y)
    {
        return hWnd != nint.Zero && SetWindowPos(hWnd, nint.Zero, x, y, 0, 0,
                            SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public static void ApplyVisibleInTaskbar(nint hWnd, bool visibleInTaskbar)
    {
        if (hWnd == nint.Zero) return;

        var exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE);

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

        SetWindowLongPtr(hWnd, GWL_EXSTYLE, exStyle);
    }

    public static void ApplyMinimizeBox(nint hWnd, bool enabled)
    {
        ApplyStyleBit(hWnd, WS_MINIMIZEBOX, enabled);
    }

    public static void ApplyMaximizeBox(nint hWnd, bool enabled)
    {
        ApplyStyleBit(hWnd, WS_MAXIMIZEBOX, enabled);
    }

    public static void ApplyTitleBarVisibility(
        nint hWnd,
        bool visible,
        bool resizable,
        bool minimizable,
        bool maximizable)
    {
        if (hWnd == nint.Zero) return;

        var style = GetWindowLongPtr(hWnd, GWL_STYLE);

        style &= ~(WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);

        if (visible)
        {
            style |= WS_CAPTION | WS_SYSMENU;

            if (resizable) style |= WS_THICKFRAME;
            if (minimizable) style |= WS_MINIMIZEBOX;
            if (maximizable) style |= WS_MAXIMIZEBOX;
        }
        else if (resizable)
        {
            style |= WS_THICKFRAME;
        }

        SetWindowLongPtr(hWnd, GWL_STYLE, style);

        SetWindowPos(hWnd, nint.Zero, 0, 0, 0, 0,
                     SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    public static void StartWindowDrag(nint hWnd)
    {
        if (hWnd == nint.Zero) return;

        ReleaseCapture();
        SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
    }

    public static void ShowWindowVisible(nint hWnd, bool visible)
    {
        if (hWnd == nint.Zero) return;
        ShowWindow(hWnd, visible ? SW_SHOW : SW_HIDE);
    }

    public static nint CreateHIconFromSKImage(SKImage image)
    {
        using var bitmap = SKBitmap.FromImage(image);
        var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var bgra = new SKBitmap(info);
        if (!bitmap.CopyTo(bgra, SKColorType.Bgra8888)) return nint.Zero;

        var width = bgra.Width;
        var height = bgra.Height;
        var pixels = bgra.Bytes;

        var colorBitmap = CreateBitmap(width, height, 1, 32, pixels);
        if (colorBitmap == nint.Zero) return nint.Zero;

        var maskBytes = new byte[(width + 7) / 8 * height];
        var maskBitmap = CreateBitmap(width, height, 1, 1, maskBytes);
        if (maskBitmap == nint.Zero)
        {
            DeleteObject(colorBitmap);
            return nint.Zero;
        }

        var iconInfo = new ICONINFO
        {
            fIcon = true,
            xHotspot = 0,
            yHotspot = 0,
            hbmMask = maskBitmap,
            hbmColor = colorBitmap
        };

        var hIcon = CreateIconIndirect(ref iconInfo);

        DeleteObject(colorBitmap);
        DeleteObject(maskBitmap);

        return hIcon;
    }

    public static void DestroyIcon(nint hIcon)
    {
        if (hIcon != nint.Zero)
            DestroyIconNative(hIcon);
    }

    private static void ApplyStyleBit(nint hWnd, nint styleBit, bool enabled)
    {
        if (hWnd == nint.Zero) return;

        var style = GetWindowLongPtr(hWnd, GWL_STYLE);

        if (enabled) style |= styleBit;
        else style &= ~styleBit;

        SetWindowLongPtr(hWnd, GWL_STYLE, style);

        SetWindowPos(hWnd, nint.Zero, 0, 0, 0, 0,
                     SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    #endregion
}