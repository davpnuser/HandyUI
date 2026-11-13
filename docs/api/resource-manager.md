# `ResourceManager`

`HandyUI.Core.Helpers.ResourceManager`

Loads resources from disk **or** embedded assembly resources.

```csharp
public static class ResourceManager
```

## Lookup order

1. `AppDomain.CurrentDomain.BaseDirectory/<relativePath>` on disk.
2. The **entry assembly**, the **executing assembly** (`HandyUI.Core`), and
   any registered assemblies - matching a manifest resource whose name ends
   with `Assets.logo.png`.

First hit wins.

## Properties

### `WhenErrored`

```csharp
public static Action<string, string>? WhenErrored { get; set; }
```

Called when `ignoreErrors = false` and a lookup fails, with `(detail, path)`.
If `null`, throws `FileNotFoundException`.

## Methods

### `RegisterAssembly(Assembly)`

```csharp
public static void RegisterAssembly(Assembly assembly);
```

Adds an assembly to the search list. Idempotent.

### `GetResourceByPath(...)`

```csharp
public static bool GetResourceByPath(
    string relativePath,
    out Stream? resourceStream,
    bool ignoreErrors = true,
    IEnumerable<Assembly>? additionalAssemblies = null);
```

Returns a **seekable** `MemoryStream` on success - safe to hand to SkiaSharp.

```csharp
if (ResourceManager.GetResourceByPath("Assets/logo.png", out var stream))
{
    using var data = SKData.Create(stream!);
    using var image = SKImage.FromEncodedData(data);
}
```