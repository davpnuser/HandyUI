namespace HandyUI.SilkNet.Classes.Tools;

internal static class PlatformTool
{
    // Currently only supports Windows.
    // -> Help us improve the cross-platform support of HandyUI as a community.
    // -> Contributing is free!
    // -> You can ignore this, though.
    // -> You aren't forced to contribute.

    #region Please read the notice text first!

    public static void EnsureSupportedPlatform()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("The HandyUI adapter for Silk.NET is currently not available on your platform. You can go to the HandyUI repository to help contribute in a fix.");
        }
    }

    #endregion
}