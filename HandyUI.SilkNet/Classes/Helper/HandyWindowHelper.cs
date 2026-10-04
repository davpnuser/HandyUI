using HandyUI.SilkNet.Components;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Classes.Helper;

public static class HandyWindowHelper
{
    private const int GWL_STYLE = -16;
    private const uint WS_CLIPCHILDREN = 0x02000000;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    public static void ApplyClipChildren(HandyWindow? window)
    {
        if (window?.InternalWindow is not null && window.InternalWindow?.Native?.Win32?.Hwnd is nint hwnd && hwnd != nint.Zero)
        {
            var currentStyle = GetWindowLongPtr(hwnd, GWL_STYLE);
            var newStyle = currentStyle | (nint)WS_CLIPCHILDREN;

            SetWindowLongPtr(hwnd, GWL_STYLE, newStyle);
        }
    }
}