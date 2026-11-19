using HandyUI.Core.Helpers;

namespace HandyUI.SilkNet.Helper;

public static class ClipboardHelper
{
    private static readonly Lock _gate = new();
    private static Win32ClipboardProvider? _provider;

    public static void EnsureInitialized()
    {
        if (!OperatingSystem.IsWindows()) return;

        lock (_gate)
        {
            if (_provider is not null) return;

            var provider = new Win32ClipboardProvider();
            _provider = provider;
            ClipboardManager.Instance.RegisterProvider(provider);
        }
    }
}