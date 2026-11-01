# `CheckBox`

`HandyUI.Controls.Controls.CheckBox`

A checkbox with a square box, animated checkmark, and multi-line text label.

```csharp
public class CheckBox : BaseControl<CheckBox>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Multi-line label. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | |
| `IsChecked` | `bool` | `false` | Animates the checkmark. |
| `BoxSize` | `float` | `18f` | Side length (min `4`). |
| `BorderWidth` | `float` | `2f` | Min `0.5`. |
| `TextGap` | `float` | `8f` | Space between box and text. |

## Events

### `WhenToggled`

```csharp
public event Action<bool>? WhenToggled;
```

Raised when the user clicks the checkbox. Programmatic `IsChecked` changes
do **not** raise it.

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public CheckBox WithWidth(float width);
public CheckBox WithHeight(float height);
public CheckBox WithText(string text);
public CheckBox WithFontSize(float size);
public CheckBox WithTypeface(SKTypeface? typeface);
public CheckBox WithIsChecked(bool isChecked);
public CheckBox WithBoxSize(float size);
public CheckBox WithBorderWidth(float width);
public CheckBox WithTextGap(float gap);

public CheckBox WithWhenToggled(Action<bool> handler);
```

## Example

```csharp
var agree = new CheckBox()
    .WithText("I agree to the terms\nand privacy policy")
    .WithIsChecked(true)
    .WithBoxSize(20)
    .WithWhenToggled(checked => Console.WriteLine($"Agreed: {checked}"))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```