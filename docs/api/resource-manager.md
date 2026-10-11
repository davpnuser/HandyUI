# `ResourceManager`

`HandyUI.Core.Components.ResourceManager`

A static helper for loading resources from disk **or** from embedded
assembly resources.

```csharp
public static class ResourceManager
```

!!! info "Lookup order"
    For a relative path (e.g. `Assets/logo.png`) the manager tries, in order:

    1. `AppDomain.CurrentDomain.BaseDirectory/<relativePath>` on disk.
    2. The **entry assembly**, the **executing assembly** (`HandyUI.Core`),
       and any assemblies registered via `RegisterAssembly`, matching a
       manifest resource whose name ends with `Assets.logo.png`.

    The first hit wins.

## Properties

### `OnError`

```csharp
public static Action<string, string>? OnError { get; set; }
```

Optional callback invoked when a lookup fails and `ignoreErrors` was `false`.
Receives `(detail, path)`. If `null`, the manager throws a
`FileNotFoundException` instead.

```csharp
ResourceManager.OnError = (detail, path) =>
    Console.Error.WriteLine($"Couldn't load '{path}': {detail}");
```

## Methods

### `RegisterAssembly(Assembly assembly)`

```csharp
public static void RegisterAssembly(Assembly assembly)
```

Adds an assembly to the global search list. Idempotent.

```csharp
ResourceManager.RegisterAssembly(typeof(MyPlugin).Assembly);
```

### `GetResourceByPath(...)`

```csharp
public static bool GetResourceByPath(
    string relativePath,
    out Stream? resourceStream,
    bool ignoreErrors = true,
    IEnumerable<Assembly>? additionalAssemblies = null)
```

| Parameter | Description |
| --- | --- |
| `relativePath` | Path like `Assets/icon.png` or a resource name. |
| `resourceStream` | On success, a *seekable* `MemoryStream` at position `0`. |
| `ignoreErrors` | If `false`, failures go through `OnError` (or throw). |
| `additionalAssemblies` | Extra assemblies to check **in addition to** the global registry. |

Returns `true` if a resource was found.

!!! tip "Stream is a snapshot"
    Embedded resources are copied into a `MemoryStream` before being returned,
    so the stream is seekable and independent of the raw manifest stream -
    safe to hand to SkiaSharp.

```csharp
if (ResourceManager.GetResourceByPath("Assets/logo.png", out var stream))
{
    using var data = SKData.Create(stream!);
    using var image = SKImage.FromEncodedData(data);
    // ...
}
```