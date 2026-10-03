namespace HandyUI.SilkNet.Classes.Helper;

internal static class PlatformTools
{
    public static void EnsureSupportedPlatform()
    {
        // Currently only supports Windows.
        // -> Help us improve the cross-platform support of HandyUI as a community.
        // -> Contributing is free!
        // -> You can ignore this, though.
        // -> You aren't forced to contribute.

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("The HandyUI adapter for Silk.NET is currently not available on your platform. You can go to the HandyUI repository to help contribute in a fix.");
        }
    }
}