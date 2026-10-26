# `BaseModule`

`HandyUI.Core.Base.BaseModule`

Base class for **logic-only** components. Modules tick once per frame via
`UIRenderer.UpdateModules` but do not draw.

```csharp
public abstract class BaseModule : IModule
```

## Properties

### `IsEnabled`

```csharp
public bool IsEnabled { get; protected set; }
```

Read-only outside the module. `true` while running. Call `Start()` / `Stop()`
to change it - that's also what triggers the `OnStart` / `OnStop` hooks.

## Methods

### `Start()`

```csharp
public void Start();
```

Sets `IsEnabled = true` and calls `OnStart()`.

### `Stop()`

```csharp
public void Stop();
```

Sets `IsEnabled = false` and calls `OnStop()`.

### `OnUpdate(float)`

```csharp
public abstract void OnUpdate(float deltaTime);
```

Called once per frame while enabled.

## Virtual hooks

```csharp
protected virtual void OnStart() { }
protected virtual void OnStop()  { }
protected virtual void OnDispose() { }
```

## Disposal

```csharp
public void Dispose();   // idempotent
```

Override `OnDispose` to clean up resources.

## Fluent API: `BaseModule<TSelf>`

```csharp
public abstract class BaseModule<TSelf> : BaseModule
    where TSelf : BaseModule<TSelf>
```

| Method | Effect |
| --- | --- |
| `WithIsEnabled(bool)` | Sets `IsEnabled` directly (bypasses `OnStart`/`OnStop`). |
| `WithAddToRenderer(UIRenderer)` | `renderer.AddModule(this)`. |

## Example

```csharp
public sealed class FrameCounter : BaseModule<FrameCounter>
{
    public int Frames { get; private set; }

    protected override void OnStart() => Console.WriteLine("started");

    public override void OnUpdate(float dt) => Frames++;
}

var counter = new FrameCounter().WithAddToRenderer(renderer);
counter.Start();
```