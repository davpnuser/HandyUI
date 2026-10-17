# `IModule`

`HandyUI.Core.Interfaces.IModule`

The contract for logic-only components ticked by the `UIRenderer`.

```csharp
public interface IModule : IDisposable
{
    bool IsEnabled { get; }

    void Start();
    void Stop();

    void OnUpdate(float deltaTime);
}
```

!!! info "Get-only `IsEnabled`"
    `IModule` exposes `IsEnabled` as **read-only**. To toggle it, call
    `Start()` / `Stop()` (or, in fluent code, `WithIsEnabled(...)` when the
    concrete builder supports it).

## Members

| Member | Type | Description |
| --- | --- | --- |
| `IsEnabled` | `bool` | When `false`, `OnUpdate` is skipped. |
| `Start()` | `void` | Turns the module on. |
| `Stop()` | `void` | Turns the module off. |
| `OnUpdate(float)` | `void` | Called once per frame while enabled. |

## Fluent API: `IModule<TSelf>`

```csharp
public interface IModule<TSelf> : IModule where TSelf : IModule<TSelf>
{
    TSelf WithIsEnabled(bool enabled);
    TSelf WithAddToRenderer(UIRenderer renderer);
}
```