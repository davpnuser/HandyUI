# `Toggle`

`HandyUI.Controls.Controls.Toggle`

An on/off switch with an animated thumb, optional text label, and a subtle
thumb-shadow.

```csharp
public class Toggle : BaseControl<Toggle>
```

## Properties

### `Width` / `Height`

```csharp
public float Width  { get; set; }
public float Height { get; set; }
```

`0` means auto-size to `TrackWidth` (plus `TextGap` and text width when
`Text` is set).

### `Text` / `FontSize`

```csharp
public string Text     { get; set; }   // default: string.Empty
public float  FontSize { get; set; }   // default: 14f
```

### `IsOn`

```csharp
public bool IsOn { get; set; }
```

Animates the thumb between off and on positions.

### `TrackWidth` / `TrackHeight` / `ThumbSize`

```csharp
public float TrackWidth  { get; set; }   // default: 36f
public float TrackHeight { get; set; }   // default: 14f
public float ThumbSize   { get; set; }   // default: 20f
```

### `TextGap`

```csharp
public float TextGap { get; set; }   // default: 8f
```

Space between the track and the text label.

## Events

### `WhenToggled`

```csharp
public event Action<bool>? WhenToggled;
```

Raised with the new state when the user clicks the toggle. Assigning
`IsOn` programmatically does **not** raise this event.

## Methods

### `DoesIntersect(SKPoint?)` / `OnUpdate(float, SKPoint?)` / `OnDraw(SKCanvas)`

Standard `BaseControl` overrides.

### `OnMouse(MouseEventContext)` - override hook

```csharp
protected override bool OnMouse(MouseEventContext ctx);
```

Toggles `IsOn` on `Click` when hovered.

## Fluent API

```csharp
public Toggle WithWidth(float width);
public Toggle WithHeight(float height);
public Toggle WithText(string text);
public Toggle WithFontSize(float size);
public Toggle WithIsOn(bool isOn);
public Toggle WithTrackWidth(float width);
public Toggle WithTrackHeight(float height);
public Toggle WithThumbSize(float size);
public Toggle WithTextGap(float gap);
public Toggle OnToggled(Action<bool> handler);
```

## Example

```csharp
var darkMode = new Toggle()
    .WithText("Enable dark mode")
    .WithIsOn(false)
    .OnToggled(on => ApplyTheme(on ? ThemeMode.Dark : ThemeMode.Light))
    .WithLocation(24, 24)
    .WithAddToRenderer(renderer);
```