# `ThemeManager`

`HandyUI.Core.Helpers.ThemeManager`

Singleton that generates a complete color system from a base color (usually
the OS accent) and a light/dark mode. Uses **OKLCH** tonal mapping via
[`ColorEngine`](#colorengine).

```csharp
public sealed class ThemeManager
```

!!! info "No `Theme` record anymore"
    In v4, there is no per-control `Theme` property. Controls read colors
    from `ThemeManager.Instance` at draw time, so a theme change immediately
    recolors everything. The old `Theme.Default` / `DefaultTheme.GetTheme()`
    palette is gone - use `ThemeManager.Instance` directly.

## Singleton

```csharp
public static ThemeManager Instance { get; }
```

## Properties

### `Mode`

```csharp
public ThemeMode Mode { get; }
```

The active mode. Set via `SetTheme`.

### `ThemeChanged`

```csharp
public event Action? ThemeChanged;
```

Raised after `SetTheme` completes. `UIRenderer` subscribes to it so
`BackgroundColor` tracks the theme (until you override it).

## Methods

### `SetTheme(byte r, byte g, byte b, ThemeMode mode)`

```csharp
public void SetTheme(byte r, byte g, byte b, ThemeMode mode);
```

Replaces the base color + mode, clears caches, raises `ThemeChanged`. You
usually don't call this yourself - [`ThemeHelper`](theme-helper.md) does it
from the OS appearance.

### `GetTone(int tone)`

```csharp
public SKColor GetTone(int tone);   // tone in [0, 900]
```

Cached OKLCH tone generation. `0` ≈ white, `900` ≈ black, `500` = accent.

## Derived colors

All cached and cheap to read. Tone selection flips with `Mode`.

| Property | Light tone | Dark tone |
| --- | --- | --- |
| `BackgroundPrimary` | 50 | 900 |
| `BackgroundSecondary` | 100 | 800 |
| `BackgroundTertiary` | 200 | 700 |
| `SurfacePrimary` | 0 | 800 |
| `SurfaceSecondary` | 50 | 700 |
| `SurfaceTertiary` | 100 | 600 |
| `TextPrimary` | 900 | 50 |
| `TextSecondary` | 800 | 100 |
| `TextTertiary` | 750 | 150 |
| `Accent` | 500 | 500 |
| `AccentHover` | 600 | 400 |
| `Border` | 200 | 700 |
| `DisabledSurface` | 100 | 800 |

Two additional derived colors:

| Property | Description |
| --- | --- |
| `TextDisabled` | `TextPrimary` at 72 % transparency. |
| `Shadow` | Black at 80 % (light) / 62 % (dark) transparency. |

## `ColorEngine`

`HandyUI.Core.Helpers.ColorEngine`

```csharp
public static (byte R, byte G, byte B)
    GetToneRgb(float baseR, float baseG, float baseB, int requestedTone);
```

OKLCH-based tone generator. Internally used by `ThemeManager.GetTone`; you
can call it directly for ad-hoc tone generation from a different base color.

```csharp
var (r, g, b) = ColorEngine.GetToneRgb(255, 0, 0, 700);
```