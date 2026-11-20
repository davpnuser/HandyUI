using SkiaSharp;

namespace HandyUI.Core.Data;

public enum ClipboardItemType
{
    None = 0,
    Text = 1,
    Image = 2,
    Files = 3
}

public sealed record ClipboardContents(
    ClipboardItemType Type,
    string? Text = null,
    SKImage? Image = null,
    IReadOnlyList<string>? Files = null);