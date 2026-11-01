# `TrayIcon`

`HandyUI.SilkNet.Components.TrayIcon`

A Windows notification-area (system tray) icon with click, double-click, and
right-click events, plus a Win32 popup context menu.

```csharp
public sealed class TrayIcon : IDisposable
```

!!! info "Obtain it from `HandyWindow`"
    You don't construct a `TrayIcon` yourself. Access it via
    [`HandyWindow.TrayIcon`](handy-window.md#trayicon), which lazily creates
    one bound to that window and disposes it when the window closes.

## Properties

### `Icon`

```csharp
public SKImage? Icon { get; set; }
```

The tray icon image. Assigning a new image recreates the underlying
`HICON`. Must be non-null for `Show()` / `Visible = true` to succeed.

```csharp
window.TrayIcon.Icon = SKImage.FromEncodedData("Assets/tray.png");
```

### `ToolTip`

```csharp
public string ToolTip { get; set; }   // default: ""
```

Tooltip shown on hover. Maximum useful length is 127 characters (Win32
`NOTIFYICONDATA.szTip`).

### `Visible`

```csharp
public bool Visible { get; set; }
```

Shows or hides the tray icon. Equivalent to `Show()` / `Hide()`.

### `ShowContextMenuOnRightClick`

```csharp
public bool ShowContextMenuOnRightClick { get; set; }   // default: true
```

When `true` and `MenuItems` is non-empty, right-clicking the tray icon
displays the context menu automatically.

### `MenuItems`

```csharp
public IList<TrayMenuItem> MenuItems { get; }
```

The right-click menu. Populate with [`TrayMenuItem`](tray-menu-item.md)
instances (including separators).

```csharp
var tray = window.TrayIcon;
tray.MenuItems.Add(new TrayMenuItem("Show", () => window.Show()));
tray.MenuItems.Add(new TrayMenuItem("Settings", OpenSettings));
tray.MenuItems.Add(TrayMenuItem.Separator());
tray.MenuItems.Add(new TrayMenuItem("Quit", () => window.Close()));
```

## Events

```csharp
public event Action? Click;
public event Action? DoubleClick;
public event Action? RightClick;
```

| Event | Fires on |
| --- | --- |
| `Click` | Single left-click release. |
| `DoubleClick` | Left double-click. |
| `RightClick` | Right-click release (before the context menu, if enabled). |

```csharp
tray.Click += () => window.Show();
tray.DoubleClick += () => BringToFront();
tray.RightClick += () => Console.WriteLine("right-clicked");
```

## Methods

### `Show()` / `Hide()`

```csharp
public void Show();
public void Hide();
```

Convenience wrappers around `Visible = true` / `false`.

### `ShowContextMenu()`

```csharp
public void ShowContextMenu();
```

Manually displays the context menu at the current cursor position. Normally
called automatically on right-click when `ShowContextMenuOnRightClick` is
`true`.

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
tray.MenuItems.Add(new TrayMenuItem("Show", () => window.Show()) { IsDefault = true });
tray.MenuItems.Add(new TrayMenuItem("Settings", OpenSettings));
tray.MenuItems.Add(TrayMenuItem.Separator());
tray.MenuItems.Add(new TrayMenuItem("Quit", () => window.Close()));
tray.Show();
```

!!! warning "Disposal is automatic"
    `HandyWindow` disposes the tray icon when the window closes or is
    disposed. Don't dispose it manually unless you're replacing it.