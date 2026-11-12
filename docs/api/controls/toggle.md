# `Toggle`

`HandyUI.Controls.Controls.Toggle`

On/off switch with animated thumb, optional label, and thumb shadow.

```csharp
public class Toggle : BaseControl<Toggle>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto) |
| `Text` | `string` | `""` (multi-line) |
| `FontSize` | `float` | `14f` |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` |
| `IsOn` | `bool` | `false` |
| `TrackWidth` / `TrackHeight` | `float` | `36f` / `14f` |
| `ThumbSize` | `float` | `20f` |
| `TextGap` | `float` | `8f` |

## Events

```csharp
public event Action<bool>? WhenToggled;
```

Fires only on user click.

## Methods

`OnMouse` toggles on `ClickEvent`.

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithFontSize, WithTypeface,
WithIsOn, WithTrackWidth, WithTrackHeight, WithThumbSize, WithTextGap,
WithWhenToggled
```

## Example

```csharp
new Toggle()
    .WithText("Enable dark mode\n(requires restart)")
    .WithIsOn(false)
    .WithWhenToggled(on => ApplyTheme(on ? ThemeMode.Dark : ThemeMode.Light))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```