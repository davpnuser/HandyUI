using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using SkiaSharp;

namespace HandyUI.Core.Helpers;

public sealed class ClipboardManager : IDisposable
{
    private static readonly Lazy<ClipboardManager> _instance = new(() => new());
    public static ClipboardManager Instance => _instance.Value;

    private readonly Lock _lock = new();
    private SKImage? _clipboardContentsImage;
    private bool _disposed;

    public event Action<ClipboardItemType>? ClipboardUpdated;
    public event Action<ClipboardContents>? Copied;
    public event Action<string>? OnClipboardSet;

    public IClipboardProvider? Provider { get; private set; }

    public string? ClipboardContentsText
    {
        get { lock (_lock) { return field; } }
        set
        {
            lock (_lock) { field = value; }
            ClipboardUpdated?.Invoke(ClipboardItemType.Text);
        }
    }

    public SKImage? ClipboardContentsImage
    {
        get { lock (_lock) { return _clipboardContentsImage; } }
        set
        {
            SKImage? oldImage;
            lock (_lock)
            {
                oldImage = _clipboardContentsImage;
                _clipboardContentsImage = value;
            }

            oldImage?.Dispose();
            ClipboardUpdated?.Invoke(ClipboardItemType.Image);
        }
    }

    public IReadOnlyList<string>? ClipboardContentsFiles
    {
        get { lock (_lock) { return field; } }
        set
        {
            lock (_lock) { field = value; }
            ClipboardUpdated?.Invoke(ClipboardItemType.Files);
        }
    }

    public void RegisterProvider(IClipboardProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (_disposed) throw new ObjectDisposedException(nameof(ClipboardManager));
        if (ReferenceEquals(Provider, provider)) return;

        UnregisterProviderInternal();

        Provider = provider;
        Provider.Copied += OnProviderCopied;
        Provider.StartMonitoring();
    }

    public void UnregisterProvider(IClipboardProvider provider)
    {
        if (provider is null || !ReferenceEquals(Provider, provider)) return;
        UnregisterProviderInternal();
    }

    private void UnregisterProviderInternal()
    {
        var provider = Provider;
        Provider = null;
        if (provider is null) return;

        provider.Copied -= OnProviderCopied;
        try { provider.StopMonitoring(); } catch { }
    }

    private void OnProviderCopied(ClipboardContents contents)
    {
        switch (contents.Type)
        {
            case ClipboardItemType.Text:
                ClipboardContentsText = contents.Text;
                break;
            case ClipboardItemType.Image:
                ClipboardContentsImage = contents.Image;
                break;
            case ClipboardItemType.Files:
                ClipboardContentsFiles = contents.Files;
                break;
        }

        Copied?.Invoke(contents);
    }

    public void SetClipboard(string text)
    {
        OnClipboardSet?.Invoke(text);
        Provider?.WriteText(text);
    }

    public void SetClipboard(SKImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        Provider?.WriteImage(image);
    }

    public void SetClipboard(IReadOnlyList<string> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        Provider?.WriteFiles(files);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        UnregisterProviderInternal();

        lock (_lock)
        {
            _clipboardContentsImage?.Dispose();
            _clipboardContentsImage = null;
        }
    }
}