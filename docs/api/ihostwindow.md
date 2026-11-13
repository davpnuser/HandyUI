# `IHostWindow`

`HandyUI.Core.Abstractions.Interface.IHostWindow`

Hosting contract for a `UIRenderer`. `HandyUI.Core` only knows about this
interface — it does not depend on any particular windowing system.
`HandyUI.SilkNet` ships the reference implementation
([`HandyWindow`](handy-window.md)), but the contract is intentionally
minimal so any host that owns a rendering surface and an input loop can
implement it and drive `UIRenderer` itself.

```csharp
public interface IHostWindow
```

## Properties

### Identity / chrome

```csharp
string   Title { get; set; }
SKImage? Icon  { get; set; }
SKSize   Size  { get; set; }
```

### Rendering

```csharp
bool UseDirtyRendering { get; set; }
bool VSync             { get; set; }
int  FramesPerSecond   { get; set; }
TweenManager TweenManager { get; set; }
UIRenderer? Renderer { get; }
```

The host owns the renderer's lifetime and assigns it back via `Renderer`.
`TweenManager` is a shared instance that the host sets and that
`UpdateContext.TweenManager` reflects per frame.

### Environment

```csharp
CursorType         Cursor      { get; set; }
bool               IsForeground { get; }
DeviceType         DeviceType { get; }
DisplayOrientation Orientation { get; }
float              DpiScale    { get; }
float              DisplayDist { get; }
double             TotalTime   { get; }
```

| Property | Meaning |
| --- | --- |
| `Cursor` | Current mouse cursor. Setting it triggers a host-specific cursor change. |
| `IsForeground` | Whether the host has focus. |
| `DeviceType` | Form factor (`Desktop`, `Mobile`, `Embedded`, …). |
| `Orientation` | Landscape / portrait / unknown. |
| `DpiScale` | Effective DPI scale (1.0 = 96 dpi). |
| `DisplayDist` | Approximate physical display diagonal, in inches. `0` when unknown. |
| `TotalTime` | Total elapsed seconds since the renderer started ticking. |

## Events

```csharp
event Action<DisplayOrientation>? WhenOrientationChanged;
```

## Methods

```csharp
void Invalidate();
```

Requests a re-render on the host's pump loop.