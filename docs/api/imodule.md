# `IModule`

`HandyUI.Core.Abstractions.Interface.IModule`

Contract for logic-only components. Modules run **before** controls in the
input pipeline - but they **cannot consume events**. Their input handlers are
purely observational: a module always sees the event, and processing always
continues to the controls below.

```csharp
public interface IModule : IDisposable
```

## Properties

```csharp
string? Name         { get; set; }
bool    AcceptsInput { get; set; }   // default: true
bool    Enabled      { get; set; }   // default: true
bool    IsDisposed   { get; }
```

### `AcceptsInput`

When `false`, `UIRenderer` skips the module during the input pipeline. The
module still ticks in `UpdateModules`. Set this on modules that only animate.

!!! warning "Modules cannot consume input"
    `ProcessMouseEvent` and `ProcessKeyEvent` return `void` in v4. A module
    observes input but always lets it fall through to the controls. If you
    need a component that can consume input, use a control
    ([`IControl`](icontrol.md)) or handle the event through a focused control.

## Methods

```csharp
abstract void OnUpdate(UpdateContext updateContext);

void Start();   // Enabled = true, calls OnStart()
void Stop();    // Enabled = false, calls OnStop()

void ProcessMouseEvent(MouseEventContext eventContext);
void ProcessKeyEvent(KeyEventContext eventContext);
```

`ProcessMouseEvent` / `ProcessKeyEvent` are no-ops when `Enabled` is `false`
or `AcceptsInput` is `false`.

```csharp
public void ProcessMouseEvent(MouseEventContext eventContext)
{
    if (Enabled) OnMouse(eventContext);
}

public void ProcessKeyEvent(KeyEventContext eventContext)
{
    if (Enabled) OnKey(eventContext);
}
```