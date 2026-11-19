using HandyUI.Core.Data.Enums;
using SkiaSharp;

namespace HandyUI.Core.Data;

public sealed record ClipboardContents(
    ClipboardItemType Type,
    string? Text = null,
    SKImage? Image = null,
    IReadOnlyList<string>? Files = null);