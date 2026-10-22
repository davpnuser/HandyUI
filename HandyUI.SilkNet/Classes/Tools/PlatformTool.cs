using Silk.NET.Windowing;

namespace HandyUI.SilkNet.Classes.Tools;

internal enum WindowPlatform
{
    Unknown,
    Windows,
    X11,
    Wayland
}

internal static class PlatformTool
{
    #region Platform Detection

    public static void EnsureSupportedPlatform()
    {
        if (OperatingSystem.IsWindows()) return;
        if (OperatingSystem.IsLinux()) return;
        throw new PlatformNotSupportedException(
            "The HandyUI adapter for Silk.NET currently supports Windows and Linux. " +
            "Community contributions for other platforms are welcome.");
    }

    public static WindowPlatform DetectPlatform(IWindow? window)
    {
        if (window is null) return WindowPlatform.Unknown;

        if (window.Native?.Win32?.Hwnd is nint hwnd && hwnd != 0)
            return WindowPlatform.Windows;

        if (window.Native?.Glfw is nint rawHandle && rawHandle != 0)
        {
            try
            {
                if (NativeGlfw.GetX11Window(rawHandle) != 0)
                    return WindowPlatform.X11;
            }
            catch { }

            try
            {
                if (NativeGlfw.GetWaylandSurface(rawHandle) != IntPtr.Zero)
                    return WindowPlatform.Wayland;
            }
            catch { }
        }

        return WindowPlatform.Unknown;
    }

    #endregion
}