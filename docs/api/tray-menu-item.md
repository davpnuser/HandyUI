# `TrayMenuItem`

`HandyUI.SilkNet.Components.TrayMenuItem`

A single entry in a [`TrayIcon`](tray-icon.md) context menu. Also
represents separators.

```csharp
public sealed class TrayMenuItem
```

## Constructors

```csharp
public TrayMenuItem();
public TrayMenuItem(string text, Action? onClick = null);
```

```csharp
var exit = new TrayMenuItem("Quit", () => window.Close());
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Text` | `string` | `""` | Display text. Ignored when `IsSeparator` is `true`. |
| `IsSeparator` | `bool` | `false` | Renders a horizontal separator line. `init`-only. |
| `IsDefault` | `bool` | `false` | Marks the menu's default item (activated by Enter / double-click of the tray icon). |
| `IsEnabled` | `bool` | `true` | When `false`, the item appears greyed and cannot be clicked. |
| `IsChecked` | `bool` | `false` | Draws a checkmark next to the item. |
| `OnClick` | `Action?` | `null` | Invoked when the user picks the item. |

### Setting flags

```csharp
var autostart = new TrayMenuItem("Launch at startup", ToggleAutostart)
{
    IsChecked = config.LaunchAtStartup
};
```

## Static helpers

### `Separator()`

```csharp
public static TrayMenuItem Separator();
```

Returns a new `TrayMenuItem` with `IsSeparator = true`.

```csharp
tray.MenuItems.Add(TrayMenuItem.Separator());
```

## Example

```csharp
window.TrayIcon.MenuItems.Add(new TrayMenuItem("Show", () => window.Show()) { IsDefault = true });
window.TrayIcon.MenuItems.Add(new TrayMenuItem("Mute", ToggleMute) { IsChecked = config.Muted });
window.TrayIcon.MenuItems.Add(TrayMenuItem.Separator());
window.TrayIcon.MenuItems.Add(new TrayMenuItem("Disabled item") { IsEnabled = false });
window.TrayIcon.MenuItems.Add(TrayMenuItem.Separator());
window.TrayIcon.MenuItems.Add(new TrayMenuItem("Quit", () => window.Close()));
```