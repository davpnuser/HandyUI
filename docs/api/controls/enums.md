# Controls Enums

`HandyUI.Controls.Enums`

## `BorderDirection`

```csharp
public enum BorderDirection
{
    Inside,
    Center,
    Outside
}
```

Where the border stroke is drawn relative to the control's bounds. Used by
[`Frame`](frame.md). `Inside` is best for crisp 1-pixel borders.

## `StretchMode`

```csharp
public enum StretchMode
{
    None,
    Fill,
    Uniform,
    UniformToFill
}
```

How [`Picture`](picture.md) and [`PictureButton`](picture-button.md) fit
their image:

| Value | Behavior |
| --- | --- |
| `None` | Natural size, anchored top-left. |
| `Fill` | Stretched to fill; aspect ratio **not** preserved. |
| `Uniform` | Scaled to fit inside; letterboxed. **Default.** |
| `UniformToFill` | Scaled to fill; cropped. |