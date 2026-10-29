# `CheckBox`

`HandyUI.Controls.Controls.CheckBox`

A checkbox rendered as a square box with an optional text label. The
checkmark draws progressively when checked, using an animated tween. Text can
be multi-line; the box aligns with the block of text vertically.

```csharp
public class CheckBox : BaseControl<CheckBox>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size.

### `Text`

```csharp
public string Text { get; set; }   // default: string.Empty
```

Label shown to the right of the box. Split on `'\n'` when drawn; the box is
vertically centered against the whole text block.

### `FontSize` / `Typeface`

```csharp
public float      FontSize { get; set; }   // default: 14f
public SKTypeface Typeface { get; set; }   // default: FontManager.DefaultFont
```

### `IsChecked`

```csharp
public bool IsChecked { get; set; }
```

Toggles the checkmark with an animation.

### `BoxSize` / `BorderWidth` / `TextGap`

```csharp
public float BoxSize     { get; set; }   // default: 18f
public float BorderWidth { get; set; }   // default: 2f
public float TextGap     { get; set; }   // default: 8f
```

## Events

### `WhenToggled`

```csharp
public event Action<bool>? WhenToggled;
```

Raised with the new state when the user clicks the checkbox. Setting
`IsChecked` programmatically does **not** raise this event.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Toggles `IsChecked` on `Click` when hovered.

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
public CheckBox OnToggled(Action<bool> handler);
```

## Example

```csharp
var agree = new CheckBox()
    .WithText("I agree to the terms\nand privacy policy")
    .WithIsChecked(true)
    .WithBoxSize(20)
    .OnToggled(checked => Console.WriteLine($"Agreed: {checked}"))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```