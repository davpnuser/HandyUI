# `StretchMode`

`HandyUI.Controls.Enums.StretchMode`

Controls how [`Picture`](controls/picture.md) and
[`PictureButton`](controls/picture-button.md) fit their image into the
control's bounds.

```csharp
public enum StretchMode
{
    None,
    Fill,
    Uniform,
    UniformToFill
}
```

| Value | Behavior |
| --- | --- |
| `None` | Draw at natural size, anchored top-left. |
| `Fill` | Stretch to fill `Bounds` exactly (aspect ratio **not** preserved). |
| `Uniform` | Scale to fit inside `Bounds`, preserving aspect ratio. Letterboxed. **Default.** |
| `UniformToFill` | Scale to fill `Bounds`, preserving aspect ratio. Cropped as needed. |