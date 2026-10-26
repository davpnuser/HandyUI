# `SKColorHelper`

`HandyUI.Core.Helper.SKColorHelper`

Extensions for `SKColor`.

```csharp
public static class SKColorHelper
```

## Hue factories

```csharp
public static SKColor FromHue(float hue);   // [0, 360]
public static SKColor FromHue(int hue);
```

Fully saturated, 50 % lightness color at the given hue.

```csharp
var red   = SKColorHelper.FromHue(0f);
var green = SKColorHelper.FromHue(120);
```

## Extensions

### `WithBrightness(...)`

```csharp
public static SKColor WithBrightness(this SKColor color, float brightness = 0.5f);
```

Multiplies HSV **value** by `brightness * 2`. `0.5` is identity; `0` is black;
`1` is full brightness.

### `WithBrightnessHsl(...)`

```csharp
public static SKColor WithBrightnessHsl(this SKColor color, float brightness = 0.5f);
```

Like `WithBrightness`, but on HSL **lightness** - better for preserving
saturation.

### `WithTransparency(...)`

```csharp
public static SKColor WithTransparency(this SKColor color, float transparency = 0f);
```

`0` = opaque, `1` = fully transparent.

```csharp
var glass = color.WithTransparency(0.75f);
```

### `Lerp(...)`

```csharp
public static SKColor Lerp(this SKColor c1, SKColor c2, float t);
```

Linear interpolation between `c1` and `c2`. `t` is clamped to `[0, 1]`.

```csharp
var blend = SKColors.Red.Lerp(SKColors.Blue, 0.25f);
```