using HandyUI.Core.Theming;
using HandyUI.SilkNet.Helper;

namespace HandyUI.SilkNet.Tool;

public static class ThemeManagementTool
{
    private static bool _isAssigned;

    public static void SetTheme(Appearance appearance)
    {
        ThemeManager.Instance.SetTheme(
            appearance.AccentColor.Red,
            appearance.AccentColor.Green,
            appearance.AccentColor.Blue,
            appearance.Theme);
    }

    public static void EnsureEventAssigned()
    {
        if (_isAssigned) return;

        ThemeHelper.AppearanceChanged += SetTheme;
        _isAssigned = true;

        SetTheme(ThemeHelper.GetAppearance());
    }

    public static void DeassignEvent()
    {
        if (!_isAssigned) return;

        ThemeHelper.AppearanceChanged -= SetTheme;
        _isAssigned = false;
    }
}