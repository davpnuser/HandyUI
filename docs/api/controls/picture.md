# `Picture`

`HandyUI.Controls.Controls.Picture`

A non-interactive image with four stretch modes.

```csharp
public class Picture : BaseControl<Picture>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized to the image when `0`. |
| `Image` | `SKImage?` | `null` | Setting it triggers `WhenImageChanged`. |
| `StretchMode` | `StretchMode` | `Uniform` | See [`StretchMode`](../stretch-mode.md). |

## Events

```csharp
public event Action? WhenClicked;
public event Action<SKImage?>? WhenImageChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public Picture WithWidth(float width);
public Picture WithHeight(float height);
public Picture WithImage(SKImage? image);
public Picture WithStretchMode(StretchMode mode);

public Picture WithWhenClicked(Action handler);
public Picture WithWhenImageChanged(Action<SKImage?> handler);
public Picture WithWhenHoverEntered(Action handler);
public Picture WithWhenHoverExited(Action handler);
```

## Example

```csharp
var logo = new Picture()
    .WithImage(myImage)
    .WithWidth(256)
    .WithHeight(128)
    .WithStretchMode(StretchMode.Uniform)
    .WithLocation(32, 32)
    .WithAddToRenderer(renderer);
```