using HandyUI.Core.Data;
using SkiaSharp;

namespace HandyUI.Core.Abstractions.Interface;

public interface IClipboardProvider : IDisposable
{
    event Action<ClipboardContents>? Copied;

    string? ReadText();
    SKImage? ReadImage();
    IReadOnlyList<string>? ReadFiles();

    void WriteText(string text);
    void WriteImage(SKImage image);
    void WriteFiles(IReadOnlyList<string> files);

    void StartMonitoring();
    void StopMonitoring();
}