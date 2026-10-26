using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HandyUI.Core.Components;

public static class FontManager
{
    public const string DefaultFontPath = "Resources/Roboto.ttf";

    private static readonly Dictionary<FontKey, SKTypeface> Cache = [];
    private static readonly Lock Gate = new();

    private static SKTypeface? _defaultFont;

    public static SKTypeface DefaultFont
    {
        get
        {
            var cached = Volatile.Read(ref _defaultFont);
            if (cached is not null)
                return cached;

            lock (Gate)
            {
                return _defaultFont ??= TryGetTypeface(DefaultFontPath, out var typeface)
                    ? typeface
                    : SKTypeface.Default;
            }
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            lock (Gate) _defaultFont = value;
        }
    }

    public static SKTypeface GetTypeface(
        string relativePath,
        int index = 0,
        IEnumerable<Assembly>? additionalAssemblies = null)
    {
        return TryGetTypeface(relativePath, out var typeface, index, additionalAssemblies)
            ? typeface
            : throw new FileNotFoundException(
            $"Font Manager Error!\nPath: '{relativePath}'\nDetail: Font file or embedded resource not found, " +
            "or the data is not a valid font.",
            relativePath);
    }

    public static bool TryGetTypeface(
        string relativePath,
        [NotNullWhen(true)] out SKTypeface? typeface,
        int index = 0,
        IEnumerable<Assembly>? additionalAssemblies = null)
    {
        typeface = null;

        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var key = CreateKey(relativePath, index);

        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var cached))
            {
                typeface = cached;
                return true;
            }
        }

        var loaded = LoadTypeface(relativePath, index, additionalAssemblies);
        if (loaded is null)
            return false;

        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var existing))
            {
                loaded.Dispose();
                typeface = existing;
                return true;
            }

            Cache[key] = loaded;
            typeface = loaded;
            return true;
        }
    }

    public static bool SetDefaultFont(
        string relativePath,
        int index = 0,
        IEnumerable<Assembly>? additionalAssemblies = null)
    {
        if (!TryGetTypeface(relativePath, out var typeface, index, additionalAssemblies))
            return false;

        lock (Gate) _defaultFont = typeface;
        return true;
    }

    public static void ClearCache()
    {
        lock (Gate) Cache.Clear();
    }

    private static SKTypeface? LoadTypeface(
        string relativePath,
        int index,
        IEnumerable<Assembly>? additionalAssemblies)
    {
        if (!ResourceManager.GetResourceByPath(
                relativePath,
                out var stream,
                ignoreErrors: true,
                additionalAssemblies: additionalAssemblies))
        {
            return null;
        }

        using (stream)
        {
            using var data = SKData.Create(stream);
            return data is null || data.Size == 0 ? null : SKTypeface.FromData(data, index);
        }
    }

    private static FontKey CreateKey(string path, int index)
    {
        return new(path.Trim().Replace('\\', '/').ToLowerInvariant(), index);
    }

    private readonly record struct FontKey(string Path, int Index);
}