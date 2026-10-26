# `ResourceManager`

`HandyUI.Core.Components.ResourceManager`

Loads resources from disk **or** from embedded assembly resources.

```csharp
public static class ResourceManager
```

!!! info "Lookup order"
    For a path like `Assets/logo.png`, the manager tries:

    1. `AppDomain.CurrentDomain.BaseDirectory/Assets/logo.png` on disk.
    2. The **entry assembly**, the **executing assembly** (`HandyUI.Core`),
       and any registered assemblies - matching a manifest resource whose
       name ends with `Assets.logo.png`.

    First hit wins.

## Properties

### `WhenErrored`

```csharp
public static Action<string, string>? WhenErrored { get; set; }
```

Optional callback invoked when a lookup fails and `ignoreErrors` was `false`.
Receives `(detail, path)`. If `null`, the manager throws
`FileNotFoundException`.

```csharp
ResourceManager.WhenErrored = (detail, path) =>
    Console.Error.WriteLine($"Couldn't load '{path}': {detail}");
```

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

| Parameter | Description |
| --- | --- |
| `relativePath` | `Assets/icon.png` or a manifest resource name. |
| `resourceStream` | On success, a **seekable** `MemoryStream` at position `0`. |
| `ignoreErrors` | If `false`, failures go through `WhenErrored` (or throw). |
| `additionalAssemblies` | Extra assemblies checked **in addition to** the global registry. |

```csharp
if (ResourceManager.GetResourceByPath("Assets/logo.png", out var stream))
{
    using var data = SKData.Create(stream!);
    using var image = SKImage.FromEncodedData(data);
}
```

!!! tip "Stream is a snapshot"
    Embedded resources are copied into a `MemoryStream` before being
    returned - safe to hand to SkiaSharp regardless of the source.