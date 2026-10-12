# `CheckBox`

`HandyUI.Controls.CheckBox`

A two-state toggle rendered as a square box. Optionally displays a primary
color fill when checked. Clicking the box toggles `IsChecked` and raises
`OnCheckChanged`.

```csharp
public class CheckBox : UIControlBase<CheckBox>
```

## Properties

### `IsChecked`

```csharp
public bool IsChecked { get; set; }   // default: false
```

Whether the checkbox is currently checked. Setting this invalidates the
control - it does **not** raise `OnCheckChanged` (only user interaction
does).

### `BoxSize`

```csharp
public float BoxSize { get; set; }   // default: 20f
```

Side length of the box in pixels. Changing this recalculates `Bounds`.

### `Spacing`

```csharp
public float Spacing { get; set; }   // default: 5f
```

Inner padding between the border and the checked fill.

### `BorderWidth`

```csharp
public float BorderWidth { get; set; }   // default: 2f
```

Stroke width of the box border.

### `Theme`

```csharp
public Theme Theme { get; set; }
```

Palette used for the primary fill, background, and border colors. Defaults
to `Theme.Default`.

### `IsEnabled` (shadowed)

```csharp
public new bool IsEnabled { get; set; }
```

Shadows `UIControlBase.IsEnabled` so the brushes refresh automatically when
the state changes. Behaves identically otherwise.

### `OnCheckChanged`

```csharp
public Action<bool>? OnCheckChanged { get; set; }
```

Invoked with the new state whenever the user toggles the checkbox.

!!! tip "Read vs. write"
    Assigning `IsChecked = true` programmatically does **not** fire
    `OnCheckChanged`. This mirrors most UI frameworks and avoids surprising
    re-entrancy.

## Methods

### `Intersects(SKPoint)`

```csharp
public override bool Intersects(SKPoint clientPoint)
```

Returns `true` when the point lies inside the box's `Bounds`.

### `Update(float, SKPoint)`

```csharp
public override void Update(float deltaTime, SKPoint clientMousePosition)
```

No-op except for the first frame, where it lazily initializes the brushes.

### `Draw(SKCanvas)`

```csharp
public override void Draw(SKCanvas canvas)
```

Draws the background (lighter when hovered), the border, and the primary
fill when checked.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext mouseContext)
```

Toggles `IsChecked` on `MouseUp` while hovered and enabled. Returns `true`
when it handled the event.

## Fluent API

```csharp
public CheckBox WithIsChecked(bool isChecked);
public CheckBox WithBoxSize(float size);
public CheckBox WithTheme(Theme theme);
public CheckBox WithOnCheckChanged(Action<bool> onCheckChanged);
```

## Example

```csharp
var dark = Theme.Default with { PrimaryColor = SKColors.Orange };

var agree = new CheckBox()
    .WithIsChecked(true)
    .WithBoxSize(22f)
    .WithTheme(dark)
    .WithOnCheckChanged(checked =>
        Console.WriteLine($"Agreed: {checked}"))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```

!!! warning "Disposal"
    `CheckBox` disposes its four internal paints in `OnDispose`. Do not
    share them with other controls.