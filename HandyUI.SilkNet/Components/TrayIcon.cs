using HandyUI.SilkNet.Helper;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Components;

public sealed class TrayIcon : IDisposable
{
    #region Internal Fields

    private readonly HandyWindow _window;
    private nint _messageWindow;
    private nint _originalWndProc;
    private Win32Helper.WndProc? _wndProc;
    private nint _hIcon;
    private bool _disposed;

    private const uint TrayIconId = 1;
    private const uint MenuIdBase = 1000;

    #endregion

    #region Events

    public event Action? Click;
    public event Action? DoubleClick;
    public event Action? RightClick;

    #endregion

    #region Constructor

    internal TrayIcon(HandyWindow window)
    {
        _window = window;
    }

    #endregion

    #region Properties

    public SKImage? Icon
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            RefreshIcon();
        }
    }

    public string ToolTip
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateToolTip();
        }
    } = "";

    public bool Visible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            if (value)
                AddIcon();
            else
                DeleteIcon();
        }
    }

    public bool ShowContextMenuOnRightClick { get; set; } = true;

    public IList<TrayMenuItem> MenuItems { get; } = [];

    #endregion

    #region Public Methods

    public void Show()
    {
        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
    }

    public void ShowContextMenu()
    {
        if (_disposed || _messageWindow == nint.Zero || MenuItems.Count == 0)
            return;

        if (!Win32Helper.GetCursorPos(out var pt))
            return;

        var hMenu = Win32Helper.CreatePopupMenu();
        if (hMenu == nint.Zero)
            return;

        var itemMap = new Dictionary<uint, TrayMenuItem>();

        try
        {
            var nextId = MenuIdBase;
            uint defaultId = 0;

            foreach (var item in MenuItems)
            {
                if (item.IsSeparator)
                {
                    Win32Helper.AppendMenu(hMenu, Win32Helper.MF_SEPARATOR, nint.Zero, null);
                    continue;
                }

                var id = nextId++;
                var flags = Win32Helper.MF_STRING;

                if (item.IsChecked)
                    flags |= Win32Helper.MF_CHECKED;

                if (!item.IsEnabled)
                    flags |= Win32Helper.MF_GRAYED;

                Win32Helper.AppendMenu(hMenu, flags, (nint)id, item.Text ?? "");
                itemMap[id] = item;

                if (item.IsDefault && defaultId == 0)
                    defaultId = id;
            }

            if (defaultId != 0)
                Win32Helper.SetMenuDefaultItem(hMenu, defaultId, 0);

            Win32Helper.SetForegroundWindow(_messageWindow);

            var selected = Win32Helper.TrackPopupMenu(
                hMenu,
                Win32Helper.TPM_RIGHTBUTTON | Win32Helper.TPM_RETURNCMD,
                pt.X, pt.Y, 0, _messageWindow, nint.Zero);

            Win32Helper.PostMessage(_messageWindow, Win32Helper.WM_NULL, 0, 0);

            if (selected != 0 && itemMap.TryGetValue((uint)selected, out var clicked))
            {
                try { clicked.OnClick?.Invoke(); }
                catch { }
            }
        }
        finally
        {
            Win32Helper.DestroyMenu(hMenu);
        }
    }

    #endregion

    #region Internal Functions

    private void EnsureMessageWindow()
    {
        if (_messageWindow != nint.Zero) return;

        _window.Invoke(() =>
        {
            if (_messageWindow != nint.Zero) return;

            var hInstance = Win32Helper.GetModuleHandle(null);
            _messageWindow = Win32Helper.CreateWindowEx(
                0,
                "STATIC",
                "",
                0,
                0, 0, 0, 0,
                Win32Helper.HWND_MESSAGE,
                nint.Zero,
                hInstance,
                nint.Zero);

            if (_messageWindow == nint.Zero) return;

            _wndProc = MessageWindowProc;
            _originalWndProc = Win32Helper.SetWindowLongPtr(
                _messageWindow,
                Win32Helper.GWLP_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_wndProc!));
        });
    }

    private nint MessageWindowProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == Win32Helper.WM_TRAYICON)
        {
            switch ((uint)lParam)
            {
                case Win32Helper.WM_LBUTTONUP:
                    Click?.Invoke();
                    return nint.Zero;
                case Win32Helper.WM_LBUTTONDBLCLK:
                    DoubleClick?.Invoke();
                    return nint.Zero;
                case Win32Helper.WM_RBUTTONUP:
                    RightClick?.Invoke();
                    if (ShowContextMenuOnRightClick && MenuItems.Count > 0)
                        ShowContextMenu();
                    return nint.Zero;
            }
        }

        return Win32Helper.CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    private void AddIcon()
    {
        if (_disposed || Icon is null) return;

        EnsureMessageWindow();
        if (_messageWindow == nint.Zero) return;

        CreateHIcon();
        if (_hIcon == nint.Zero) return;

        _window.Invoke(() =>
        {
            var data = CreateNotifyIconData();
            data.uFlags = Win32Helper.NIF_MESSAGE | Win32Helper.NIF_ICON | Win32Helper.NIF_TIP;
            data.uCallbackMessage = Win32Helper.WM_TRAYICON;
            data.hIcon = _hIcon;
            data.szTip = ToolTip;

            Win32Helper.Shell_NotifyIcon(Win32Helper.NIM_ADD, ref data);
        });
    }

    private void UpdateToolTip()
    {
        if (_disposed || !Visible || _messageWindow == nint.Zero) return;

        _window.Invoke(() =>
        {
            var data = CreateNotifyIconData();
            data.uFlags = Win32Helper.NIF_TIP;
            data.szTip = ToolTip;
            Win32Helper.Shell_NotifyIcon(Win32Helper.NIM_MODIFY, ref data);
        });
    }

    private void RefreshIcon()
    {
        if (_disposed) return;

        if (!Visible)
        {
            DestroyHIcon();
            return;
        }

        if (_messageWindow == nint.Zero || Icon is null) return;

        DestroyHIcon();
        CreateHIcon();
        if (_hIcon == nint.Zero) return;

        _window.Invoke(() =>
        {
            var data = CreateNotifyIconData();
            data.uFlags = Win32Helper.NIF_ICON;
            data.hIcon = _hIcon;
            Win32Helper.Shell_NotifyIcon(Win32Helper.NIM_MODIFY, ref data);
        });
    }

    private Win32Helper.NOTIFYICONDATA CreateNotifyIconData()
    {
        return new Win32Helper.NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<Win32Helper.NOTIFYICONDATA>(),
            hWnd = _messageWindow,
            uID = TrayIconId,
            szTip = "",
            szInfo = "",
            szInfoTitle = "",
        };
    }

    private void CreateHIcon()
    {
        if (Icon is null) return;
        _hIcon = Win32Helper.CreateHIconFromSKImage(Icon);
    }

    private void DestroyHIcon()
    {
        if (_hIcon == nint.Zero) return;
        Win32Helper.DestroyIcon(_hIcon);
        _hIcon = nint.Zero;
    }

    private void DeleteIcon()
    {
        if (_messageWindow == nint.Zero) return;

        _window.Invoke(() =>
        {
            var data = CreateNotifyIconData();
            Win32Helper.Shell_NotifyIcon(Win32Helper.NIM_DELETE, ref data);
        });
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DeleteIcon();
        DestroyHIcon();

        if (_messageWindow != nint.Zero)
        {
            Win32Helper.SetWindowLongPtr(_messageWindow, Win32Helper.GWLP_WNDPROC, _originalWndProc);
            Win32Helper.DestroyWindow(_messageWindow);
            _messageWindow = nint.Zero;
        }
    }

    #endregion
}