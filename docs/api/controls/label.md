# `Label`

`HandyUI.Controls.Controls.Label`

A non-interactive text label with alignment and bold support. Auto-sizes its
bounds to fit the current text.

```csharp
public class Label : BaseControl<Label>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size to measured text.

### `Text`

```csharp
public string Text { get; set; }   // default: string.Empty
```

### `FontSize` / `IsBold`

```csharp
public float FontSize { get; set; }   // default: 14f
public bool  IsBold   { get; set; }
```

### `TextAlign`

```csharp
public SKTextAlign TextAlign { get; set; }   // default: Left
```

Horizontal alignment within `Bounds`: `Left`, `Center`, or `Right`.

## Events

```csharp
public event Action? WhenClicked;
public event Action? WhenHoverEntered;
public event Action? WhenHoverExited;
public event Action<string>? WhenTextChanged;
```

`WhenTextChanged` fires whenever `Text` is set (even to the same value).

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Raises `WhenClicked` on `Click`.

## Fluent API

```csharp
public Label WithWidth(float width);
public Label WithHeight(float height);
public Label WithText(string text);
public Label WithFontSize(float size);
public Label WithIsBold(bool bold);
public Label WithTextAlign(SKTextAlign align);
public Label OnClicked(Action handler);
public Label OnHoverEntered(Action handler);
public Label OnHoverExited(Action handler);
public Label OnTextChanged(Action<string> handler);
```

## Example

```csharp
var title = new Label()
    .WithText("Settings")
    .WithFontSize(22)
    .WithIsBold(true)
    .WithTextAlign(SKTextAlign.Center)
    .WithWidth(320)
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```