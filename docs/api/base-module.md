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

## Overridable members

```csharp
public abstract void OnUpdate(UpdateContext updateContext);

protected virtual void OnStart() { }
protected virtual void OnStop()  { }

protected virtual void OnMouse(MouseEventContext eventContext) { }
protected virtual void OnKey(KeyEventContext eventContext) { }
```

!!! warning "Input handlers return `void`"
    `OnMouse` and `OnKey` return `void` in v4. Modules observe input but
    cannot consume it - the event always continues down to the controls.
    If you need consuming behavior, use a control instead.

## Methods

```csharp
public void Start();   // Enabled = true, OnStart()
public void Stop();    // Enabled = false, OnStop()

public void ProcessMouseEvent(MouseEventContext eventContext);
public void ProcessKeyEvent(KeyEventContext eventContext);

public void Dispose();   // idempotent
```

## Fluent builder: `BaseModule<TSelf>`

```csharp
public abstract class BaseModule<TSelf> : BaseModule where TSelf : BaseModule<TSelf>
```

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

Observing input (no consuming):

```csharp
public sealed class InputLogger : BaseModule<InputLogger>
{
    public override void OnUpdate(UpdateContext updateContext) { }

    protected override void OnMouse(MouseEventContext eventContext)
    {
        Console.WriteLine($"mouse: {eventContext.Type}");
    }

    protected override void OnKey(KeyEventContext eventContext)
    {
        Console.WriteLine($"key: {eventContext.Type} ({eventContext.KeyCode})");
    }
}
```