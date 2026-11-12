# SkiaSharp Extensions

The v3 `SKColorHelper` / `SKRectHelper` classes are gone. In v4, helpers are
extension methods grouped by target type, all under
`HandyUI.Core.Helpers.Extensions`.

## `SKColorExtensions`

`HandyUI.Core.Helpers.Extensions.SKColorExtensions`

```csharp
public static class SKColorExtensions
{
    public static SKColor WithOpacity(this SKColor color, float opacity);
    public static SKColor WithTransparency(this SKColor color, float transparency);
    public static SKColor WithBrightness(this SKColor color, float brightness = 0.5f);
    public static SKColor WithBrightnessHsl(this SKColor color, float brightness = 0.5f);
    public static SKColor Lerp(this SKColor c1, SKColor c2, float t);
}
```

| Method | Notes |
| --- | --- |
| `WithOpacity(float)` | `0..1` multiplier on alpha. |
| `WithTransparency(float)` | `0` = opaque, `1` = fully transparent. |
| `WithBrightness(float)` | Multiplies HSV value by `brightness * 2`. `0.5` = identity. |
| `WithBrightnessHsl(float)` | Same idea on HSL lightness - better for saturated colors. |
| `Lerp(SKColor, float)` | Linear interpolation. |

```csharp
var dim = ThemeManager.Instance.Accent.WithBrightness(0.7f);
var glass = ThemeManager.Instance.TextPrimary.WithTransparency(0.5f);
```

## `SKPointExtensions`

```csharp
public static class SKPointExtensions
{
    public static SKPoint ToSKPoint(this SKRect rect);   // (Right, Bottom)
    public static SKPoint ToSKPoint(this SKSize size);   // (Width, Height)
}
```

## `SKRectExtensions`

```csharp
public static class SKRectExtensions
{
    public static SKRect ToSKRect(this SKPoint point);   // (0, 0, X, Y)
    public static SKRect ToSKRect(this SKSize size);     // (0, 0, W, H)

    public static SKRect TopHalf(this SKRect rect);
    public static SKRect BottomHalf(this SKRect rect);
    public static SKRect LeftHalf(this SKRect rect);
    public static SKRect RightHalf(this SKRect rect);

    public static SKRect TopLeft(this SKRect rect);
    public static SKRect TopRight(this SKRect rect);
    public static SKRect BottomLeft(this SKRect rect);
    public static SKRect BottomRight(this SKRect rect);

    public static SKRect[] DivideHorizontal(this SKRect rect, int count);
    public static SKRect   DivideHorizontal(this SKRect rect, int count, int index);

    public static SKRect[] DivideVertical(this SKRect rect, int count);
    public static SKRect   DivideVertical(this SKRect rect, int count, int index);
}
```

`DivideHorizontal` splits into equal-height strips (top → bottom);
`DivideVertical` splits into equal-width strips (left → right). The last
slice aligns exactly with `rect.Right` / `rect.Bottom` to avoid accumulated
floating-point drift.

## `SKSizeExtensions`

```csharp
public static class SKSizeExtensions
{
    public static SKSize ToSKSize(this SKRect rect);    // (Right, Bottom)
    public static SKSize ToSKSize(this SKPoint point);  // (X, Y)
}
```