# `FontManager`

`HandyUI.Core.Helpers.FontManager`

Loads and caches `SKTypeface` fonts from disk or embedded resources.

```csharp
public static class FontManager
{
    public const string DefaultFontPath = "Resources/Roboto.ttf";

    public static SKTypeface DefaultFont { get; set; }

    public static SKTypeface GetTypeface(string relativePath, int index = 0,
        IEnumerable<Assembly>? additionalAssemblies = null);

    public static bool TryGetTypeface(string relativePath, out SKTypeface? typeface,
        int index = 0, IEnumerable<Assembly>? additionalAssemblies = null);

    public static bool SetDefaultFont(string relativePath, int index = 0,
        IEnumerable<Assembly>? additionalAssemblies = null);

    public static void ClearCache();
}
```

### `DefaultFont`

Lazily resolves the embedded `Resources/Roboto.ttf`; falls back to
`SKTypeface.Default` if missing. Every built-in control uses this unless you
assign `Typeface` explicitly.

```csharp
FontManager.DefaultFont = FontManager.GetTypeface("Fonts/Inter-Regular.ttf");
```

### `GetTypeface` / `TryGetTypeface`

Cached. `index` selects a face within a collection (`.ttc`).
`GetTypeface` throws `FileNotFoundException` on failure; `TryGetTypeface`
returns `false`.

### `SetDefaultFont`

Convenience wrapper - loads the font and sets `DefaultFont`.

### `ClearCache`

Empties the cache. Live `SKTypeface` references held by controls are
unaffected.