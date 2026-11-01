# `Label`

`HandyUI.Controls.Controls.Label`

A non-interactive text label with alignment and bold support. Multi-line
aware.

```csharp
public class Label : BaseControl<Label>
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Width` / `Height` | `float` | `0` | Auto-sized when `0`. |
| `Text` | `string` | `""` | Split on `'\n'`; each line honors `TextAlign`. |
| `FontSize` | `float` | `14f` | |
| `Typeface` | `SKTypeface` | `FontManager.DefaultFont` | |
| `IsBold` | `bool` | `false` | |
| `TextAlign` | `SKTextAlign` | `Left` | `Left` / `Center` / `Right`. |

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<string>? WhenTextChanged;
```

## Methods

`DoesIntersect`, `OnUpdate`, `OnDraw`, `OnMouse` - standard overrides.

## Fluent API

```csharp
public Label WithWidth(float width);
public Label WithHeight(float height);
public Label WithText(string text);
public Label WithFontSize(float size);
public Label WithTypeface(SKTypeface? typeface);
public Label WithIsBold(bool bold);
public Label WithTextAlign(SKTextAlign align);

public Label WithWhenClicked(Action handler);
public Label WithWhenHoverEntered(Action handler);
public Label WithWhenHoverExited(Action handler);
public Label WithWhenTextChanged(Action<string> handler);
```

## Example

```csharp
var header = new Label()
    .WithText("Settings\nAppearance")
    .WithFontSize(22)
    .WithIsBold(true)
    .WithTextAlign(SKTextAlign.Center)
    .WithWidth(320)
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom typefaces"
    Load one through [`FontManager`](../font-manager.md) and pass it in:

    ```csharp
    var inter = FontManager.GetTypeface("Fonts/Inter-SemiBold.ttf");
    control.WithTypeface(inter);
    ```