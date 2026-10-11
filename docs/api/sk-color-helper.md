# `SKColorHelper`

`HandyUI.Core.Classes.Helper.SKColorHelper`

Small, focused helpers for `SKColor`.

```csharp
public static class SKColorHelper
```

## Hue factories

```csharp
public static SKColor FromHue(float hue);   // hue in [0, 360]
public static SKColor FromHue(int hue);
```

Creates a fully saturated, 50 % lightness color at the given hue.

```csharp
var red   = SKColorHelper.FromHue(0f);
var green = SKColorHelper.FromHue(120);
```

## Extensions

### `WithBrightness(this SKColor, float brightness = 0.5f)`

```csharp
public static SKColor WithBrightness(this SKColor color, float brightness = 0.5f)
```

Multiplies HSV **value** by `brightness * 2`. `0.5` is identity; `0` is black;
`1` is full brightness.

```csharp
var dim   = color.WithBrightness(0.25f);
var light = color.WithBrightness(1.0f);
```

### `WithBrightnessHsl(this SKColor, float brightness = 0.5f)`

```csharp
public static SKColor WithBrightnessHsl(this SKColor color, float brightness = 0.5f)
```

Like `WithBrightness` but operates on HSL **lightness**, which better
preserves saturation.

### `WithTransparency(this SKColor, float transparency = 0f)`

```csharp
public static SKColor WithTransparency(this SKColor color, float transparency = 0f)
```

`0` = opaque, `1` = fully transparent.

```csharp
var glass = color.WithTransparency(0.75f);
```

!!! warning "Naming gotcha"
    `WithTransparency` uses *transparency* (inverse of alpha). Don't confuse
    it with `WithAlpha`, which takes alpha directly.