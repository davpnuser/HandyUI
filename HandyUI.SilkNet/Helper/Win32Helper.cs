using HandyUI.SilkNet.Components;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Helper;

public static class Win32Helper
{
    #region Constants

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    private const nint WS_CLIPCHILDREN = 0x02000000;
    private const nint WS_MINIMIZEBOX = 0x00020000;
    private const nint WS_MAXIMIZEBOX = 0x00010000;

    private const nint WS_EX_TOOLWINDOW = 0x00000080;
    private const nint WS_EX_APPWINDOW = 0x00040000;

    private const uint WM_NULL = 0x0000;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

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
    private static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter,
                                            int X, int Y, int cx, int cy, uint uFlags);

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

    /// <summary>
    /// Posts a WM_NULL message to the window, which forces the pump
    /// loop to wake up and process any pending work items.
    /// </summary>
    public static void WakeWindow(nint hWnd)
    {
        if (hWnd == nint.Zero) return;
        PostMessage(hWnd, WM_NULL, 0, 0);
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

    private static void ApplyStyleBit(nint hWnd, nint styleBit, bool enabled)
    {
        if (hWnd == nint.Zero) return;

        var style = GetWindowLongPtr(hWnd, GWL_STYLE);

        if (enabled) style |= styleBit;
        else style &= ~styleBit;

        SetWindowLongPtr(hWnd, GWL_STYLE, style);

        // Force the non-client area to redraw so the button state
        // visually updates immediately (no title-bar lag).
        SetWindowPos(hWnd, nint.Zero, 0, 0, 0, 0,
                     SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    #endregion
}