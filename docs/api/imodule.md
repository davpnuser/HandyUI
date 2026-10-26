# `IModule`

`HandyUI.Core.Interface.IModule`

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

## Members

| Member | Type | Description |
| --- | --- | --- |
| `IsEnabled` | `bool` | Read-only. When `false`, `OnUpdate` is skipped. |
| `Start()` | `void` | Turns the module on. |
| `Stop()` | `void` | Turns the module off. |
| `OnUpdate(float)` | `void` | Per-frame update while enabled. |

## Fluent API: `IModule<TSelf>`

```csharp
public interface IModule<TSelf> : IModule where TSelf : IModule<TSelf>
{
    TSelf WithIsEnabled(bool enabled);
    TSelf WithAddToRenderer(UIRenderer renderer);
}
```