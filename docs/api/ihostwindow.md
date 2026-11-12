# `IHostWindow`

`HandyUI.Core.Abstractions.Interface.IHostWindow`

Hosting contract for a `UIRenderer`. `HandyUI.Core` only knows about this
interface; the Silk.NET package ships a reference implementation
([`HandyWindow`](handy-window.md)). Custom hosts (Avalonia, WPF, MAUI, SDL,
…) implement `IHostWindow` and drive `UIRenderer` themselves.

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