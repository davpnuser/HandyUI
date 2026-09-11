using HandyUI.Core.Components;

namespace HandyUI.WinForms.Classes.Helper;

public static class ResourceManagerHelper
{
    public static void InitializeResourceManager()
    {
        ResourceManager.OnError = (detail, path) =>
            MessageBox.Show($"Resource Manager Error!\nPath: '{path}'\nDetail: {detail}",
                    "HandyUI Resource Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
    }
}