# `CheckBox`

`HandyUI.Controls.Controls.CheckBox`

Square checkbox with animated checkmark and optional multi-line label.

```csharp
public class CheckBox : BaseControl<CheckBox>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto) |
| `Text` | `string` | `""` |
| `FontSize` | `float` | `14f` |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` |
| `IsChecked` | `bool` | `false` |
| `BoxSize` | `float` | `18f` (min `4`) |
| `BorderWidth` | `float` | `2f` (min `0.5`) |
| `TextGap` | `float` | `8f` |

## Events

```csharp
public event Action<bool>? WhenToggled;
```

Fires only on user click. Programmatic `IsChecked` changes do **not** raise it.

!!! warning "Checkmark is not animated in v4.0"
    `CheckBox.StartCheckAnimation` currently stores a *pending* target rather
    than starting the tween. The checkmark renders at the current
    `_checkAmount` - which snaps to 0 or 1 after the first `OnUpdate`. Expect
    a non-animated checkmark until this is fixed.

## Methods

Standard overrides. `OnMouse` toggles on `ClickEvent`.

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithFontSize, WithTypeface,
WithIsChecked, WithBoxSize, WithBorderWidth, WithTextGap,
WithWhenToggled(Action<bool>)
```

## Example

```csharp
var agree = new CheckBox()
    .WithText("I agree\nand accept")
    .WithIsChecked(true)
    .WithBoxSize(20)
    .WithWhenToggled(b => Console.WriteLine($"Agreed: {b}"))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```