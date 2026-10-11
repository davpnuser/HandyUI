# `BaseModule`

`HandyUI.Core.Classes.Base.BaseModule`

Base class for **logic‑only** components. Modules tick once per frame (via
`UIRenderer.UpdateModules`) but do not draw.

```csharp
public abstract class BaseModule : IModule
```

## Properties

### `IsEnabled`

```csharp
public bool IsEnabled { get; set; }
```

Toggling this raises `OnStateChanged`. Disabled modules are skipped by the
renderer's update loop.

## Events

### `OnStateChanged`

```csharp
public event Action<bool>? OnStateChanged;
```

Raised whenever `IsEnabled` changes.

## Methods

### `Start()` / `Stop()`

```csharp
public void Start() => IsEnabled = true;
public void Stop()  => IsEnabled = false;
```

### `Update(float deltaTime)`

```csharp
public abstract void Update(float deltaTime);
```

Called once per frame while `IsEnabled == true`.

## Disposal

```csharp
public void Dispose();
protected virtual void OnDispose();
```

Dispose is idempotent. Override `OnDispose` to clean up resources.

## Fluent API: `BaseModule<TSelf>`

```csharp
public abstract class BaseModule<TSelf> : BaseModule
    where TSelf : BaseModule<TSelf>
```

| Method | Effect |
| --- | --- |
| `WithIsEnabled(bool)` | Sets `IsEnabled`. |
| `WithOnStateChanged(Action<bool>)` | Subscribes to `OnStateChanged`. |
| `WithAddToRenderer(UIRenderer)` | Calls `renderer.AddModule(this)`. |

## Example

```csharp
public sealed class FrameCounter : BaseModule<FrameCounter>
{
    public int Frames { get; private set; }

    public override void Update(float dt) => Frames++;
}

var counter = new FrameCounter().WithAddToRenderer(renderer);
```

!!! tip
    Modules are disposed automatically when removed from the renderer or when
    the renderer itself is disposed.