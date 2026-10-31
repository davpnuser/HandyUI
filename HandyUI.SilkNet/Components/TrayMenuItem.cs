namespace HandyUI.SilkNet.Components;

public sealed class TrayMenuItem
{
    #region Properties

    public string Text { get; set; } = "";
    public bool IsSeparator { get; init; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsChecked { get; set; }
    public Action? OnClick { get; set; }

    #endregion

    #region Constructors

    public TrayMenuItem()
    {
    }

    public TrayMenuItem(string text, Action? onClick = null)
    {
        Text = text;
        OnClick = onClick;
    }

    #endregion

    #region Static Helpers

    public static TrayMenuItem Separator()
    {
        return new() { IsSeparator = true };
    }

    #endregion
}