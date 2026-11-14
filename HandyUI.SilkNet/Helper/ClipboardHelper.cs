using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.Factories;
using ClipboardMonitor.Core.Interfaces;
using HandyUI.Core.Helpers;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using TextCopy;

namespace HandyUI.SilkNet.Helper;

public static class ClipboardHelper
{
    private static IClipboardListener? _clipboard;

    public static void EnsureInitialized()
    {
        _clipboard ??= ClipboardListenerFactory.CreateClipboardListener();
        _clipboard.ClipboardChanged += ClipboardChanged;
        _clipboard.Start();
        ClipboardManager.Instance.OnClipboardSet += OnClipboardSet;
    }

    private static void ClipboardChanged(object? sender, ClipboardMonitor.Core.EventArguments.ClipboardChangedEventArgs e)
    {
        if (e.DataType == ClipboardDataType.TEXT && !string.IsNullOrEmpty(e.ClipboardText))
        {
            ClipboardManager.Instance.ClipboardContentsText = e.ClipboardText;
        }
        else if (e.DataType == ClipboardDataType.IMAGE)
        {
            if (e.BmpClipboardImage != null)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1, 0, 0))
                {
                    var snapshot = e.BmpClipboardImage;

                    using var convertedBmp = new Bitmap(snapshot.Width, snapshot.Height, PixelFormat.Format32bppPArgb);
                    using (var g = Graphics.FromImage(convertedBmp))
                    {
                        g.DrawImage(snapshot, 0, 0, snapshot.Width, snapshot.Height);
                    }

                    var rect = new Rectangle(0, 0, convertedBmp.Width, convertedBmp.Height);
                    var data = convertedBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                    try
                    {
                        var info = new SKImageInfo(convertedBmp.Width, convertedBmp.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                        ClipboardManager.Instance.ClipboardContentsImage = SKImage.FromPixelCopy(info, data.Scan0, data.Stride);
                    }
                    finally
                    {
                        convertedBmp.UnlockBits(data);
                    }
                }
            }
            else if (e.ClipboardImage is IClipboardImage clipboardImage && clipboardImage.Data != null)
            {
                ClipboardManager.Instance.ClipboardContentsImage = SKImage.FromEncodedData(clipboardImage.Data);
            }
        }
    }

    private static void OnClipboardSet(string text)
    {
        ClipboardService.SetText(text);
    }
}