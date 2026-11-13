# `BaseModule`

`HandyUI.Core.Abstractions.Base.BaseModule`

Base class for logic-only modules.

```csharp
public abstract class BaseModule : IModule
```

## Properties

```csharp
public string? Name         { get; set; }
public bool    Enabled      { get; set; } = true;
public bool    AcceptsInput { get; set; } = true;
public bool    IsDisposed   { get; private set; }
```

## Methods

```csharp
public abstract void OnUpdate(UpdateContext updateContext);

protected virtual void OnStart() { }
protected virtual void OnStop()  { }
protected virtual void OnDispose() { }

protected virtual bool OnMouse(MouseEventContext eventContext) => false;
protected virtual bool OnKey(KeyEventContext eventContext) => false;

public void Start();   // Enabled = true, OnStart()
public void Stop();    // Enabled = false, OnStop()

public bool ProcessMouseEvent(MouseEventContext eventContext);
public bool ProcessKeyEvent(KeyEventContext eventContext);

public void Dispose();   // idempotent
```

## Fluent builder: `BaseModule<TSelf>`

| Method | Effect |
| --- | --- |
| `DoAndReturn(Action<TSelf>)` | Chains arbitrary mutation. |
| `WithName(string)` | `Name` |
| `WithEnabled(bool)` | `Enabled` |
| `WithAddToRenderer(UIRenderer)` | `renderer.AddModule(this)` |

## Example

```csharp
public sealed class FrameCounter : BaseModule<FrameCounter>
{
    public int Frames { get; private set; }

    public override void OnUpdate(UpdateContext updateContext) => Frames++;
}

var counter = new FrameCounter().WithAddToRenderer(renderer);
```