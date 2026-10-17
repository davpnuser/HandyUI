# `BaseModule`

`HandyUI.Core.Classes.Base.BaseModule`

Base class for **logic-only** components. Modules tick once per frame (via
`UIRenderer.UpdateModules`) but do not draw.

```csharp
public abstract class BaseModule : IModule
```

!!! info "No `OnStateChanged` event"
    Unlike earlier versions, `BaseModule` no longer exposes an `OnStateChanged`
    event and `IsEnabled` has a **protected setter**. Enable / disable from
    the outside via `Start()` / `Stop()`, or subclass and expose your own
    state API.

## Properties

### `IsEnabled`

```csharp
public bool IsEnabled { get; protected set; }
```

`true` while the module is running. Read-only from the outside - call
`Start()` / `Stop()` (or `WithIsEnabled(...)` on the fluent builder) to
change it.

## Methods

### `Start()`

```csharp
public void Start()
```

Sets `IsEnabled = true` and invokes `OnStart()`.

### `Stop()`

```csharp
public void Stop()
```

Sets `IsEnabled = false` and invokes `OnStop()`.

### `OnUpdate(float deltaTime)`

```csharp
public abstract void OnUpdate(float deltaTime);
```

Called once per frame while `IsEnabled == true`.

## Virtual hooks

```csharp
protected virtual void OnStart() { }
protected virtual void OnStop()  { }
protected virtual void OnDispose() { }
```

## Disposal

```csharp
public void Dispose();
```

Idempotent. Override `OnDispose` to clean up resources.

## Fluent API: `BaseModule<TSelf>`

```csharp
public abstract class BaseModule<TSelf> : BaseModule
    where TSelf : BaseModule<TSelf>
```

| Method | Effect |
| --- | --- |
| `WithIsEnabled(bool)` | Directly sets `IsEnabled` (bypasses `OnStart`/`OnStop`). |
| `WithAddToRenderer(UIRenderer)` | Calls `renderer.AddModule(this)`. |

## Example

```csharp
public sealed class FrameCounter : BaseModule<FrameCounter>
{
    public int Frames { get; private set; }

    protected override void OnStart() => Console.WriteLine("starting");

    public override void OnUpdate(float dt) => Frames++;
}

var counter = new FrameCounter().WithAddToRenderer(renderer);

// later:
counter.Start();
```

!!! tip
    Modules are disposed automatically when removed from the renderer or when
    the renderer itself is disposed.