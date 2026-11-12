# `TrayMenuItem`

`HandyUI.SilkNet.Components.TrayMenuItem`

One entry in a [`TrayIcon`](tray-icon.md) context menu.

```csharp
public sealed class TrayMenuItem
```

## Constructors

```csharp
public TrayMenuItem();
public TrayMenuItem(string text, Action? onClick = null);
```

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Text` | `string` | `""` | Display text. |
| `IsSeparator` | `bool` | `false` | Renders a horizontal separator (`init`-only). |
| `IsDefault` | `bool` | `false` | Marks the default item. |
| `IsEnabled` | `bool` | `true` | When `false` → greyed out. |
| `IsChecked` | `bool` | `false` | Draws a checkmark. |
| `OnClick` | `Action?` | `null` | Invoked when picked. |

## Static helpers

```csharp
public static TrayMenuItem Separator();
```

## Example

```csharp
var mute = new TrayMenuItem("Mute", ToggleMute)
{
    IsChecked = config.Muted,
    IsEnabled = audioDeviceAvailable
};
window.TrayIcon.MenuItems.Add(mute);
```