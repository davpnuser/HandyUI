using HandyUI.Core.Enums;
using Microsoft.Win32;
using SkiaSharp;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;

namespace HandyUI.SilkNet.Helper;

public readonly record struct Appearance(SKColor AccentColor, ThemeMode Theme);

public static class ThemeHelper
{
    private static readonly SKColor FallbackColor = new(0, 120, 215);

    private static readonly Lock _gate = new();
    private static Action<Appearance>? _changed;
    private static SKColor _lastColor;
    private static ThemeMode _lastTheme;
    private static bool _watching;

    private static CancellationTokenSource? _linuxCts;
    private static DBusConnection? _linuxConnection;
    private static IDisposable? _linuxObserver;

    public static event Action<Appearance> AppearanceChanged
    {
        add
        {
            lock (_gate)
            {
                _changed += value;
                if (!_watching) StartWatchingNoLock();
            }
        }
        remove
        {
            lock (_gate)
            {
                _changed -= value;
                if (_changed is null) StopWatchingNoLock();
            }
        }
    }

    public static SKColor GetColor()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240) ? GetWindowsAccentColor()
        : OperatingSystem.IsLinux() ? GetLinuxAccentColor()
        : FallbackColor;
    }

    public static ThemeMode GetTheme()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240) ? GetWindowsTheme()
        : OperatingSystem.IsLinux() ? GetLinuxTheme()
        : ThemeMode.Unknown;
    }

    public static Appearance GetAppearance()
    {
        return new(GetColor(), GetTheme());
    }

    [SupportedOSPlatform("windows10.0.10240")]
    private static SKColor GetWindowsAccentColor()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            if (key?.GetValue("AccentColor") is int dword)
            {
                return new SKColor(
                    (byte)(dword & 0xFF),
                    (byte)((dword >> 8) & 0xFF),
                    (byte)((dword >> 16) & 0xFF));
            }
        }
        catch { }

        return FallbackColor;
    }

    [SupportedOSPlatform("windows10.0.10240")]
    private static ThemeMode GetWindowsTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int value)
                return value == 0 ? ThemeMode.Dark : ThemeMode.Light;
        }
        catch { }

        return ThemeMode.Unknown;
    }

    private static SKColor GetLinuxAccentColor()
    {
        return RunDBusQuery(ReadLinuxAccentColorAsync, FallbackColor);
    }

    private static ThemeMode GetLinuxTheme()
    {
        return RunDBusQuery(ReadLinuxThemeAsync, ThemeMode.Unknown);
    }

    private static T RunDBusQuery<T>(Func<DBusConnection, Task<T>> query, T fallback)
    {
        try
        {
            return Task.Run(async () =>
            {
                using var connection = new DBusConnection(DBusAddress.Session!);
                await connection.ConnectAsync().ConfigureAwait(false);
                return await query(connection).ConfigureAwait(false);
            }).GetAwaiter().GetResult();
        }
        catch
        {
            return fallback;
        }
    }

    private static async Task<VariantValue> ReadPortalSettingAsync(DBusConnection connection, string key)
    {
        MessageBuffer message;
        using (var writer = connection.GetMessageWriter())
        {
            writer.WriteMethodCallHeader(
                destination: "org.freedesktop.portal.Desktop",
                path: "/org/freedesktop/portal/desktop",
                @interface: "org.freedesktop.portal.Settings",
                member: "ReadOne",
                signature: "ss");
            writer.WriteString("org.freedesktop.appearance");
            writer.WriteString(key);
            message = writer.CreateMessage();
        }

        var reply = await connection.CallMethodAsync(message, (result, _) => result).ConfigureAwait(false);
        return reply.GetBodyReader().ReadVariantValue();
    }

    private static async Task<SKColor> ReadLinuxAccentColorAsync(DBusConnection connection)
    {
        return ParseAccentColor(await ReadPortalSettingAsync(connection, "accent-color").ConfigureAwait(false));
    }

    private static async Task<ThemeMode> ReadLinuxThemeAsync(DBusConnection connection)
    {
        return ParseTheme(await ReadPortalSettingAsync(connection, "color-scheme").ConfigureAwait(false));
    }

    private static SKColor ParseAccentColor(VariantValue value)
    {
        while (value.Type == VariantValueType.Variant)
            value = value.GetVariantValue();

        if (value.Type != VariantValueType.Struct || value.Count != 3)
            return FallbackColor;

        var r = value.GetItem(0).GetDouble();
        var g = value.GetItem(1).GetDouble();
        var b = value.GetItem(2).GetDouble();

        return r is < 0 or > 1 || g is < 0 or > 1 || b is < 0 or > 1
            ? FallbackColor
            : new SKColor(
                (byte)Math.Round(r * 255),
                (byte)Math.Round(g * 255),
                (byte)Math.Round(b * 255));
    }

    private static ThemeMode ParseTheme(VariantValue value)
    {
        while (value.Type == VariantValueType.Variant)
            value = value.GetVariantValue();

        return value.Type != VariantValueType.UInt32
            ? ThemeMode.Unknown
            : value.GetUInt32() switch
            {
                1 => ThemeMode.Dark,
                2 => ThemeMode.Light,
                _ => ThemeMode.Unknown,
            };
    }

    private static void StartWatchingNoLock()
    {
        var appearance = GetAppearance();
        _lastColor = appearance.AccentColor;
        _lastTheme = appearance.Theme;
        _watching = true;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        else if (OperatingSystem.IsLinux())
            _ = StartLinuxWatchingAsync();
    }

    private static void StopWatchingNoLock()
    {
        if (!_watching) return;
        _watching = false;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        else if (OperatingSystem.IsLinux())
            StopLinuxWatching();
    }

    private static void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        UpdateAppearance(GetColor(), GetTheme());
    }

    private static async Task StartLinuxWatchingAsync()
    {
        var cts = new CancellationTokenSource();
        _linuxCts = cts;

        DBusConnection? connection = null;
        IDisposable? observer = null;

        try
        {
            connection = new DBusConnection(DBusAddress.Session!);
            await connection.ConnectAsync().ConfigureAwait(false);
            cts.Token.ThrowIfCancellationRequested();

            var initialColor = await ReadLinuxAccentColorAsync(connection).ConfigureAwait(false);
            var initialTheme = await ReadLinuxThemeAsync(connection).ConfigureAwait(false);
            UpdateAppearance(initialColor, initialTheme);

            var rule = new MatchRule
            {
                Type = MessageType.Signal,
                Sender = "org.freedesktop.portal.Desktop",
                Path = "/org/freedesktop/portal/desktop",
                Interface = "org.freedesktop.portal.Settings",
                Member = "SettingChanged",
            };

            observer = await connection.AddMatchAsync(
                rule,
                static (message, _) => message,
                OnSettingChanged,
                emitOnCapturedContext: false).ConfigureAwait(false);

            cts.Token.ThrowIfCancellationRequested();

            lock (_gate)
            {
                if (!_watching)
                {
                    observer.Dispose();
                    connection.Dispose();
                    return;
                }

                _linuxConnection = connection;
                _linuxObserver = observer;
            }
        }
        catch
        {
            observer?.Dispose();
            connection?.Dispose();
        }
        finally
        {
            if (_linuxCts == cts)
                _linuxCts = null;
        }
    }

    private static void OnSettingChanged(Notification<Message> notification)
    {
        if (!notification.HasValue) return;

        try
        {
            var reader = notification.Value.GetBodyReader();
            if (reader.ReadString() != "org.freedesktop.appearance") return;

            var key = reader.ReadString();
            var value = reader.ReadVariantValue();

            switch (key)
            {
                case "accent-color":
                    UpdateAppearance(color: ParseAccentColor(value));
                    break;
                case "color-scheme":
                    UpdateAppearance(theme: ParseTheme(value));
                    break;
            }
        }
        catch { }
    }

    private static void StopLinuxWatching()
    {
        _linuxCts?.Cancel();
        _linuxCts?.Dispose();
        _linuxCts = null;

        _linuxObserver?.Dispose();
        _linuxObserver = null;

        _linuxConnection?.Dispose();
        _linuxConnection = null;
    }

    private static void UpdateAppearance(SKColor? color = null, ThemeMode? theme = null)
    {
        Action<Appearance>? handler;
        Appearance appearance;

        lock (_gate)
        {
            if (!_watching) return;

            var newColor = color ?? _lastColor;
            var newTheme = theme ?? _lastTheme;

            if (newColor == _lastColor && newTheme == _lastTheme) return;

            _lastColor = newColor;
            _lastTheme = newTheme;
            appearance = new(newColor, newTheme);
            handler = _changed;
        }

        handler?.Invoke(appearance);
    }
}