# `ThemeHelper`

`HandyUI.SilkNet.Helper.ThemeHelper`

Detects the operating system's accent color and light/dark mode, feeds them
into [`ThemeManager`](theme-manager.md), and re-invalidates all open
windows when the OS theme changes.

```csharp
public static class ThemeHelper
{
    public static event Action<Appearance> AppearanceChanged;

    public static void EnsureInitialized();
    public static void Deinitialize();

    public static void RegisterWindow(HandyWindow window);
    public static void UnregisterWindow(HandyWindow window);

    public static SKColor GetColor();
    public static ThemeMode GetTheme();
    public static Appearance GetAppearance();
}
```

!!! info "Windows and Linux"
    - **Windows 10+**: reads `HKCU\Software\Microsoft\Windows\DWM\AccentColor`
      and `AppsUseLightTheme`, and watches `SystemEvents.UserPreferenceChanged`.
    - **Linux**: reads `org.freedesktop.appearance` over the XDG portal D-Bus
      (`accent-color` and `color-scheme`), and subscribes to `SettingChanged`.
    - **Other platforms**: returns `FallbackColor` / `ThemeMode.Unknown`.

## Appearance record

```csharp
public readonly record struct Appearance(SKColor AccentColor, ThemeMode Theme);
```

Bundles the two OS values.

## Events

### `AppearanceChanged`

```csharp
public static event Action<Appearance> AppearanceChanged;
```

Raised whenever the OS accent or light/dark mode changes. Subscribing
automatically starts the watcher; unsubscribing the last handler stops it.

## Methods

### `EnsureInitialized()`

```csharp
public static void EnsureInitialized();
```

Idempotent. Reads the current appearance, subscribes `ThemeManager` to
`AppearanceChanged`, and starts the OS watcher. `HandyWindow`'s constructor
calls this for you - you only need to call it explicitly if you're using
`HandyUI.Core` without the Silk.NET adapter.

### `Deinitialize()`

```csharp
public static void Deinitialize();
```

Unsubscribes `ThemeManager` and stops the watcher if no other handlers are
attached. Rarely needed - the process exiting does this for you.

### `RegisterWindow(HandyWindow)` / `UnregisterWindow(HandyWindow)`

```csharp
public static void RegisterWindow(HandyWindow window);
public static void UnregisterWindow(HandyWindow window);
```

Registers a window to be re-invalidated whenever the OS theme changes.
[`RendererHelper.Attach`](silknet-helpers.md#rendererhelper) registers the
window automatically; `HandyWindow`'s close handler unregisters it.

### `GetColor()` / `GetTheme()` / `GetAppearance()`

```csharp
public static SKColor    GetColor();       // OS accent
public static ThemeMode  GetTheme();       // Dark / Light / Unknown
public static Appearance GetAppearance();  // both
```

Immediate reads (not cached). Useful for one-shot queries:

```csharp
var appearance = ThemeHelper.GetAppearance();
Console.WriteLine($"Accent: {appearance.AccentColor}, Mode: {appearance.Theme}");
```

!!! tip "Apply once at startup"
    If you're not using the Silk.NET adapter, apply the OS theme manually:

    ```csharp
    var a = ThemeHelper.GetAppearance();
    ThemeManager.Instance.SetTheme(a.AccentColor.Red, a.AccentColor.Green, a.AccentColor.Blue, a.Theme);
    ```