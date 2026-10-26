# `Button`

`HandyUI.Controls.Controls.Button`

A filled text button with rounded corners, elevation shadow, hover color
blending, and a click ripple.

```csharp
public class Button : BaseControl<Button>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` (the default) means *auto-size* to content.

### `Text`

```csharp
public string Text { get; set; }   // default: string.Empty
```

### `CornerRadius`

```csharp
public float CornerRadius { get; set; }   // default: 2f
```

### `Elevation`

```csharp
public int Elevation { get; set; }   // default: 2
```

Shadow offset in pixels. `0` disables the shadow. Grows slightly on hover.

### `FontSize` / `IsBold`

```csharp
public float FontSize { get; set; }   // default: 14f
public bool  IsBold   { get; set; }
```

### `HorizontalPadding` / `VerticalPadding`

```csharp
public float HorizontalPadding { get; set; }   // default: 16f
public float VerticalPadding   { get; set; }   // default: 8f
```

Used to size the content area when `Width`/`Height` are `0`.

## Events

### `WhenClicked`

```csharp
public event Action? WhenClicked;
```

Raised on `Click` (which the renderer synthesizes on `MouseUp` inside the
button).

## Methods

### `DoesIntersect(SKPoint?)`

```csharp
public override bool DoesIntersect(SKPoint? clientPoint);
```

### `OnUpdate(float, SKPoint?)`

```csharp
public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition);
```

Updates the hover amount when `IsHovered` changes.

### `OnDraw(SKCanvas)`

```csharp
public override void OnDraw(SKCanvas canvas);
```

Draws the shadow, accent-colored fill (blended with `AccentHover` on hover),
the ripple (if active), and centered text.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

- On `MouseDown`, starts the ripple at the click position.
- On `Click`, raises `WhenClicked`.

## Fluent API

```csharp
public Button WithWidth(float width);
public Button WithHeight(float height);
public Button WithText(string text);
public Button WithCornerRadius(float radius);
public Button WithElevation(int elevation);
public Button WithFontSize(float size);
public Button WithIsBold(bool bold);
public Button WithPadding(float horizontal, float vertical);
public Button OnClicked(Action handler);
```

## Example

```csharp
var save = new Button()
    .WithText("Save")
    .WithCornerRadius(6)
    .WithElevation(3)
    .WithFontSize(15)
    .WithIsBold(true)
    .WithPadding(20, 10)
    .OnClicked(() => Save())
    .WithLocation(24, 80)
    .WithAddToRenderer(renderer);
```