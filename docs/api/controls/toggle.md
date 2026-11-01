# `Toggle`

`HandyUI.Controls.Controls.Toggle`

An on/off switch with an animated thumb, optional text label, and a subtle
thumb shadow. Text can be multi-line.

```csharp
public class Toggle : BaseControl<Toggle>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Multi-line. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | |
| `IsOn` | `bool` | `false` | Animates the thumb. |
| `TrackWidth` / `TrackHeight` | `float` | `36f` / `14f` | |
| `ThumbSize` | `float` | `20f` | |
| `TextGap` | `float` | `8f` | |

## Events

```csharp
public event Action<bool>? WhenToggled;
```

Raised on user click. Programmatic `IsOn` changes do **not** raise it.

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public Toggle WithWidth(float width);
public Toggle WithHeight(float height);
public Toggle WithText(string text);
public Toggle WithFontSize(float size);
public Toggle WithTypeface(SKTypeface? typeface);
public Toggle WithIsOn(bool isOn);
public Toggle WithTrackWidth(float width);
public Toggle WithTrackHeight(float height);
public Toggle WithThumbSize(float size);
public Toggle WithTextGap(float gap);

public Toggle WithWhenToggled(Action<bool> handler);
```

## Example

```csharp
var darkMode = new Toggle()
    .WithText("Enable dark mode\n(requires restart)")
    .WithIsOn(false)
    .WithWhenToggled(on => ApplyTheme(on ? ThemeMode.Dark : ThemeMode.Light))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```