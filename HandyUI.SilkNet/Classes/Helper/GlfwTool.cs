using Silk.NET.Windowing;

namespace HandyUI.SilkNet.Classes.Helper;

internal static class GlfwTool
{
    private static readonly Lock GlfwCreationLock = new();
    private static bool _isGlfwInitialized;

    public static void EnsureGlfwInitialized()
    {
        if (_isGlfwInitialized) return;

        lock (GlfwCreationLock)
        {
            if (!_isGlfwInitialized)
            {
                Window.PrioritizeGlfw();
                _isGlfwInitialized = true;
            }
        }
    }
}
