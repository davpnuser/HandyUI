# `IModule`

`HandyUI.Core.Interfaces.IModule`

The contract for logic‑only components ticked by the `UIRenderer`.

```csharp
public interface IModule : IDisposable
```

## Members

| Member | Type | Description |
| --- | --- | --- |
| `IsEnabled` | `bool` | When `false`, `Update` is skipped. |
| `OnStateChanged` | `event Action<bool>` | Raised on `IsEnabled` changes. |
| `Start()` | `void` | Typically sets `IsEnabled = true`. |
| `Stop()` | `void` | Typically sets `IsEnabled = false`. |
| `Update(float)` | `void` | Called once per frame while enabled. |

## Fluent API: `IModule<TSelf>`

```csharp
public interface IModule<TSelf> : IModule where TSelf : IModule<TSelf>
{
    TSelf WithIsEnabled(bool enabled);
    TSelf WithOnUpdate(Action<float>? action);
    TSelf WithOnStateChanged(Action<bool>? action);
    TSelf WithAddToRenderer(UIRenderer renderer);
}
```

!!! note
    `WithOnUpdate` is declared on the interface. `BaseModule<TSelf>` does not
    currently implement it in source - override `Update` or implement it in
    your own module if you need it.