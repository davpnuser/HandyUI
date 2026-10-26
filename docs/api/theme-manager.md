# `ThemeManager`

`HandyUI.Core.Theming.ThemeManager`

A process-wide singleton that generates a full color system from a single
base color (usually the OS accent) and a light/dark mode, using **OKLCH**
tonal mapping. Every built-in control reads its colors from here at draw
time, so changing the theme updates all controls live.

```csharp
public sealed class ThemeManager
{
    public static ThemeManager Instance { get; }

    public ThemeMode Mode { get; }
    public event Action? ThemeChanged;

    public void SetTheme(byte r, byte g, byte b, ThemeMode mode);

    public SKColor GetTone(int tone);

    // Derived properties …
}
```

!!! tip "`ThemeManager` vs `Theme`"
    `ThemeManager` is *live* - it holds a base color + mode and generates
    tones on demand. [`Theme`](theme.md) is a *static palette*. Built-in
    controls use `ThemeManager` by default; `Theme` is available when you
    want to freeze a specific palette per-control.

## Singleton

### `Instance`

```csharp
public static ThemeManager Instance { get; }
```

The one and only instance. Thread-safe to access.

## Properties

### `Mode`

```csharp
public ThemeMode Mode { get; }
```

The current light/dark mode. Read-only; set via `SetTheme`.

### `ThemeChanged`

```csharp
public event Action? ThemeChanged;
```

Fires after `SetTheme` completes. `UIRenderer` subscribes to this to
auto-update its `BackgroundColor`, and `ThemeHelper` uses it to push OS
appearance changes.

## Methods

### `SetTheme(byte r, byte g, byte b, ThemeMode mode)`

```csharp
public void SetTheme(byte r, byte g, byte b, ThemeMode mode);
```

Replaces the base color and mode. Clears the tone and derived caches, then
raises `ThemeChanged`.

```csharp
ThemeManager.Instance.SetTheme(0x25, 0x63, 0xEB, ThemeMode.Light);
```

!!! warning "You usually don't call this"
    The recommended path is [`ThemeHelper.EnsureInitialized()`](theme-helper.md#ensureinitialized),
    which reads the OS appearance and calls `SetTheme` for you. Call
    `SetTheme` directly only when you want a custom accent independent of
    the OS.

### `GetTone(int tone)`

```csharp
public SKColor GetTone(int tone);   // tone in [0, 900]
```

Generates (and caches) a color from the current base at the given OKLCH
tone. `0` is nearly white, `900` is nearly black; `500` is the accent.
Internally calls [`ColorEngine.GetToneRgb`](#colorengine).

```csharp
var accent = ThemeManager.Instance.GetTone(500);
var surface = ThemeManager.Instance.GetTone(100);
```

## Derived color properties

All of these are computed from `Mode` + `GetTone(...)` and are cheap to
read (they hit the tone cache).

| Property | Light mode tone | Dark mode tone | Use |
| --- | --- | --- | --- |
| `BackgroundPrimary` | 50 | 900 | Window / app background. |
| `BackgroundSecondary` | 100 | 800 | Panel background. |
| `BackgroundTertiary` | 200 | 700 | Deeper surface. |
| `SurfacePrimary` | 0 | 800 | Card / input surface. |
| `SurfaceSecondary` | 50 | 700 | Alternate surface. |
| `SurfaceTertiary` | 100 | 600 | Alternate surface, deeper. |
| `TextPrimary` | 900 | 50 | Main text. |
| `TextSecondary` | 800 | 100 | Secondary text. |
| `TextTertiary` | 750 | 150 | Hint / placeholder text. |
| `Accent` | 500 | 500 | Brand / interactive. |
| `AccentHover` | 600 | 400 | Hovered accent. |
| `Border` | 200 | 700 | Strokes and separators. |
| `DisabledSurface` | 100 | 800 | Fill for disabled controls. |

Two more derived properties compute from cached colors:

| Property | Description |
| --- | --- |
| `TextDisabled` | `TextPrimary` at 72 % transparency. |
| `Shadow` | Black at 80 % (light) / 62 % (dark) transparency. |

## `ThemeMode`

`HandyUI.Core.Enums.ThemeMode`

```csharp
public enum ThemeMode
{
    Dark,
    Light,
    Unknown
}
```

`Unknown` is used when the OS appearance can't be detected (unsupported
platform, or the D-Bus/registry read failed). `ThemeManager` treats
`Unknown` the same as `Light` for tone selection.

## `ColorEngine`

`HandyUI.Core.Theming.ColorEngine`

Static OKLCH-based tone generator used internally by `ThemeManager`.

```csharp
public static class ColorEngine
{
    public static (byte R, byte G, byte B)
        GetToneRgb(float baseR, float baseG, float baseB, int requestedTone);
}
```

| Parameter | Range | Meaning |
| --- | --- | --- |
| `baseR/G/B` | 0-255 | Source color (usually the accent). |
| `requestedTone` | 0-900 | Tone. `0` ≈ white, `900` ≈ black, `500` = accent. |

`GetToneRgb` converts the base to OKLCH, keeps its hue, maps lightness to
the requested tone, and reduces chroma near the extremes so pure
white/black anchors look clean.

!!! info "You rarely call this"
    `ThemeManager.GetTone` already caches results. Use `ColorEngine` only if
    you need a tone from a *different* base color than the current theme.