using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HandyUI.Core.Components;

public static class ResourceManager
{
    private static readonly List<Assembly> SearchAssemblies = [];

    static ResourceManager()
    {
        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            SearchAssemblies.Add(entryAssembly);
        }

        SearchAssemblies.Add(Assembly.GetExecutingAssembly());
    }

    public static Action<string, string>? OnError { get; set; }

    public static void RegisterAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (!SearchAssemblies.Contains(assembly))
        {
            SearchAssemblies.Add(assembly);
        }
    }

    public static bool GetResourceByPath(
        string relativePath,
        [NotNullWhen(true)] out Stream? resourceStream,
        bool ignoreErrors = true,
        IEnumerable<Assembly>? additionalAssemblies = null)
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

        var assembliesToSearch = additionalAssemblies != null
            ? SearchAssemblies.Concat(additionalAssemblies).Distinct()
            : SearchAssemblies;

        foreach (var assembly in assembliesToSearch)
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

        if (OnError != null)
        {
            OnError(detail, path);
        }
        else
        {
            throw new FileNotFoundException($"Resource Manager Error!\nPath: '{path}'\nDetail: {detail}", path);
        }
    }
}