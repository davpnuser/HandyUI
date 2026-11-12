# `ThemeHelper`

`HandyUI.SilkNet.Helper.ThemeHelper`

Reads the OS accent color and light/dark mode, feeds them into
[`ThemeManager`](theme-manager.md), and re-invalidates all open windows when
the OS theme changes.

```csharp
public static class ThemeHelper
{
    public static event Action<Appearance> AppearanceChanged;

    public static void EnsureInitialized();
    public static void Deinitialize();
    public static void RegisterWindow(HandyWindow window);
    public static void UnregisterWindow(HandyWindow window);

    public static SKColor    GetColor();
    public static ThemeMode  GetTheme();
    public static Appearance GetAppearance();
}
```

## `Appearance`

```csharp
public readonly record struct Appearance(SKColor AccentColor, ThemeMode Theme);
```

## Platform support

- **Windows 10+**: reads registry keys and watches `UserPreferenceChanged`.
- **Linux**: reads `org.freedesktop.appearance` via the XDG portal on D-Bus
  and subscribes to `SettingChanged`.
- **Others**: returns `FallbackColor` / `ThemeMode.Unknown`.

## Methods

- `EnsureInitialized()` - idempotent; called automatically by `HandyWindow`.
  Subscribe once and start watching.
- `Deinitialize()` - unsubscribe and stop watching.
- `RegisterWindow(HandyWindow)` / `UnregisterWindow(HandyWindow)` - track
  windows so they can be re-invalidated on theme change.
  `RendererHelper.Attach` registers automatically.
- `GetColor()` / `GetTheme()` / `GetAppearance()` - immediate reads.

## Example

```csharp
ThemeHelper.AppearanceChanged += appearance =>
{
    Console.WriteLine($"Accent: {appearance.AccentColor}, mode: {appearance.Theme}");
};
```