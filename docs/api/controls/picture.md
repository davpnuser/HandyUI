# `Picture`

`HandyUI.Controls.Controls.Picture`

Non-interactive image with four stretch modes.

```csharp
public class Picture : BaseControl<Picture>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto → image size) |
| `Image` | `SKImage?` | `null` |
| `StretchMode` | `StretchMode` | `Uniform` |

## Events

```csharp
public event Action? WhenClicked;
public event Action<SKImage?>? WhenImageChanged;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
```

## Fluent API

```csharp
WithWidth, WithHeight, WithImage, WithStretchMode,
WithWhenClicked, WithWhenImageChanged,
WithWhenHoverEntered, WithWhenHoverExited
```

## Example

```csharp
new Picture()
    .WithImage(myImage)
    .WithWidth(256)
    .WithHeight(128)
    .WithStretchMode(StretchMode.Uniform)   // letterbox
    .WithLocation(32, 32)
    .WithAddToRenderer(renderer);
```