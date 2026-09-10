using HandyUI.Core.Classes.Base;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HandyUI.WinForms.Components;

public static class ResourceManager
{
    private static readonly Assembly[] TargetAssemblies =
    [
        Assembly.GetEntryAssembly()!,
        Assembly.GetExecutingAssembly(),
        typeof(UIControlBase).Assembly
    ];

    public static bool GetResourceByPath(
        string relativePath,
        [NotNullWhen(true)] out Stream? resourceStream,
        bool ignoreErrors = true)
    {
        resourceStream = null;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            ShowError(ignoreErrors, "No resource path provided.", relativePath);
            return false;
        }

        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (File.Exists(localFilePath))
        {
            try
            {
                resourceStream = File.OpenRead(localFilePath);
                return true;
            }
            catch (Exception ex)
            {
                ShowError(ignoreErrors, $"Failed to open disk file: {ex.Message}", relativePath);
                return false;
            }
        }

        var sanitizedSuffix = "." + relativePath.TrimStart('/', '\\')
                                               .Replace('/', '.')
                                               .Replace('\\', '.');

        foreach (var assembly in TargetAssemblies)
        {
            var manifestNames = assembly.GetManifestResourceNames();

            var matchedName = manifestNames.FirstOrDefault(name =>
                name.EndsWith(sanitizedSuffix, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(relativePath, StringComparison.OrdinalIgnoreCase));

            if (matchedName != null)
            {
                var rawStream = assembly.GetManifestResourceStream(matchedName);
                if (rawStream != null)
                {
                    var memoryStream = new MemoryStream();
                    rawStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    rawStream.Dispose();

                    resourceStream = memoryStream;
                    return true;
                }
            }
        }

        ShowError(ignoreErrors, "Embedded resource or disk file not found.", relativePath);
        return false;
    }

    private static void ShowError(bool ignoreErrors, string detail, string path)
    {
        if (ignoreErrors) return;

        MessageBox.Show(
            $"Resource Manager Error!\nPath: '{path}'\nDetail: {detail}",
            "HandyUI Resource Manager",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}