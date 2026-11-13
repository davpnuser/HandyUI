# `IModule`

`HandyUI.Core.Abstractions.Interface.IModule`

Contract for logic-only components. Modules run **before** controls in the
input pipeline, so a module can consume an event by returning `true` from
`ProcessMouseEvent` / `ProcessKeyEvent`.

```csharp
public interface IModule : IDisposable
```

## Properties

```csharp
string? Name        { get; set; }
bool    AcceptsInput { get; set; }   // default: true
bool    Enabled      { get; set; }   // default: true
bool    IsDisposed   { get; }
```

### `AcceptsInput`

When `false`, `UIRenderer` skips the module during the input pipeline (the
module still ticks in `UpdateModules`). Set this on modules that only need
to animate.

## Methods

```csharp
abstract void OnUpdate(UpdateContext updateContext);

void Start();   // sets Enabled = true, calls OnStart()
void Stop();    // sets Enabled = false, calls OnStop()

bool ProcessMouseEvent(MouseEventContext eventContext);
bool ProcessKeyEvent(KeyEventContext eventContext);
```

`ProcessMouseEvent` / `ProcessKeyEvent` short-circuit to `false` when
`Enabled` is `false` or `AcceptsInput` is `false`.