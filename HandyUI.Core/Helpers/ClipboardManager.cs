using HandyUI.Core.Data.Enums;
using SkiaSharp;

namespace HandyUI.Core.Helpers;

public sealed class ClipboardManager : IDisposable
{
    private static readonly Lazy<ClipboardManager> _instance = new(() => new());
    public static ClipboardManager Instance => _instance.Value;

    private readonly Lock _lock = new();
    private SKImage? _clipboardContentsImage;

    public event Action<ClipboardItemType>? ClipboardUpdated;
    public event Action<string>? OnClipboardSet;

    public string? ClipboardContentsText
    {
        get
        {
            lock (_lock) { return field; }
        }
        set
        {
            lock (_lock) { field = value; }
            ClipboardUpdated?.Invoke(ClipboardItemType.Text);
        }
    }

    public SKImage? ClipboardContentsImage
    {
        get
        {
            lock (_lock) { return _clipboardContentsImage; }
        }
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

    public void SetClipboard(string text)
    {
        OnClipboardSet?.Invoke(text);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _clipboardContentsImage?.Dispose();
            _clipboardContentsImage = null;
        }
    }
}