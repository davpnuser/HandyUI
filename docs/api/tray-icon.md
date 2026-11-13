# `TrayIcon`

`HandyUI.SilkNet.Components.TrayIcon`

Windows notification-area icon with click / double-click / right-click events
and a Win32 popup context menu.

```csharp
public sealed class TrayIcon : IDisposable
```

!!! info "Obtain from `HandyWindow`"
    Access via [`HandyWindow.TrayIcon`](handy-window.md#trayicon). It is
    disposed automatically when the window closes.

## Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Icon` | `SKImage?` | `null` | Tray image. Assigning refreshes the `HICON`. |
| `ToolTip` | `string` | `""` | Hover tooltip (max 127 chars). |
| `Visible` | `bool` | `false` | Shows / hides the tray icon. |
| `ShowContextMenuOnRightClick` | `bool` | `true` | Auto-show menu on right-click. |
| `MenuItems` | `IList<TrayMenuItem>` | empty | Context menu entries. |

## Events

```csharp
public event Action? Click;
public event Action? DoubleClick;
public event Action? RightClick;
```

## Methods

```csharp
public void Show();
public void Hide();
public void ShowContextMenu();
```

`ShowContextMenu()` normally fires automatically on right-click when
`ShowContextMenuOnRightClick` is `true` and `MenuItems` is non-empty.

## Example

```csharp
window.WhenClosing += () =>
{
    WindowHelper.TryCancelClose(window.InternalWindow!);
    window.Hide();
};

var tray = window.TrayIcon;
tray.Icon = SKImage.FromEncodedData("Assets/tray.png");
tray.ToolTip = "Chat - click to show";
tray.Click += () => window.Show();
tray.MenuItems.Add(new TrayMenuItem("Show", () => window.Show()) { IsDefault = true });
tray.MenuItems.Add(new TrayMenuItem("Settings", OpenSettings));
tray.MenuItems.Add(TrayMenuItem.Separator());
tray.MenuItems.Add(new TrayMenuItem("Quit", () => window.Close()));
tray.Show();
```