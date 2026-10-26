# `FontManager`

`HandyUI.Core.Components.FontManager`

Loads `SKTypeface` fonts from disk or embedded resources, caches them, and
exposes a shared default font used by every built-in control.

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

## Properties

### `DefaultFont`

```csharp
public static SKTypeface DefaultFont { get; set; }
```

The font used by every built-in control when you don't provide a typeface
explicitly. Defaults to the embedded `Resources/Roboto.ttf`; falls back to
`SKTypeface.Default` if the resource can't be loaded.

Setting `DefaultFont` to `null` throws `ArgumentNullException`.

```csharp
FontManager.DefaultFont = FontManager.GetTypeface("Fonts/Inter-Regular.ttf");
```

## Methods

### `GetTypeface(...)`

```csharp
public static SKTypeface GetTypeface(
    string relativePath, int index = 0,
    IEnumerable<Assembly>? additionalAssemblies = null);
```

Returns a cached `SKTypeface` loaded from `relativePath`. Throws
`FileNotFoundException` if the file isn't found or isn't a valid font.

`index` selects a face within a font collection (`.ttc`).

```csharp
var inter = FontManager.GetTypeface("Fonts/Inter-Bold.ttf");
```

### `TryGetTypeface(...)`

```csharp
public static bool TryGetTypeface(
    string relativePath, out SKTypeface? typeface,
    int index = 0, IEnumerable<Assembly>? additionalAssemblies = null);
```

Like `GetTypeface` but returns `false` instead of throwing.

### `SetDefaultFont(...)`

```csharp
public static bool SetDefaultFont(
    string relativePath, int index = 0,
    IEnumerable<Assembly>? additionalAssemblies = null);
```

Convenience wrapper: loads the font and, if successful, assigns it to
`DefaultFont`. Returns `false` on failure.

### `ClearCache()`

```csharp
public static void ClearCache();
```

Empties the typeface cache. Live `SKTypeface` references held by controls are
unaffected; new lookups will reload from disk.

!!! tip "Resource loading"
    `FontManager` uses [`ResourceManager`](resource-manager.md) internally,
    so anything `ResourceManager.GetResourceByPath` can find, `FontManager`
    can load.