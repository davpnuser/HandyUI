# `Label`

`HandyUI.Controls.Controls.Label`

Non-interactive text label with alignment, bold, and multi-line support.

```csharp
public class Label : BaseControl<Label>
```

## Properties

| Property | Type | Default |
| --- | --- | --- |
| `Width` / `Height` | `float` | `0` (auto) |
| `Text` | `string` | `""` |
| `FontSize` | `float` | `14f` |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` |
| `IsBold` | `bool` | `false` |
| `TextAlign` | `SKTextAlign` | `Left` |

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<string>? WhenTextChanged;
```

## Methods

Standard overrides. `OnMouse` fires `WhenClicked` on `ClickEvent`.

## Fluent API

```csharp
WithWidth, WithHeight, WithText, WithFontSize, WithTypeface,
WithIsBold, WithTextAlign,
WithWhenClicked, WithWhenHoverEntered, WithWhenHoverExited,
WithWhenTextChanged
```

## Example

```csharp
new Label()
    .WithText("Settings\nAppearance")
    .WithFontSize(22)
    .WithIsBold(true)
    .WithTextAlign(SKTextAlign.Center)
    .WithWidth(320)
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```