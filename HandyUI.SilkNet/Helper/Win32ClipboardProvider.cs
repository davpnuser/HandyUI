using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.SilkNet.Tool;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace HandyUI.SilkNet.Helper;

internal sealed class Win32ClipboardProvider : IClipboardProvider
{
    private readonly ManualResetEventSlim _monitorReady = new(false);

    private Thread? _monitorThread;
    private uint _monitorThreadId;
    private nint _hwnd;
    private nint _originalWndProc;
    private Win32Helper.WndProc? _wndProc;

    private volatile bool _monitoring;
    private volatile bool _monitorRunning;
    private volatile bool _disposed;

    public event Action<ClipboardContents>? Copied;

    public void StartMonitoring()
    {
        PlatformTool.EnsureSupportedPlatform();

        if (_monitoring || _disposed) return;

        _monitorRunning = true;
        _monitorReady.Reset();

        _monitorThread = new Thread(MonitorThreadProc)
        {
            IsBackground = true,
            Name = "HandyUI-ClipboardMonitor"
        };
#pragma warning disable CA1416
        _monitorThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416
        _monitorThread.Start();

        _monitorReady.Wait();
        _monitoring = true;
    }

    public void StopMonitoring()
    {
        if (!_monitoring) return;
        _monitoring = false;
        _monitorRunning = false;

        if (_hwnd != nint.Zero)
        {
            try { Win32Helper.RemoveClipboardFormatListener(_hwnd); } catch { }
        }

        if (_monitorThreadId != 0)
        {
            try { Win32Helper.PostThreadMessage(_monitorThreadId, Win32Helper.WM_QUIT, 0, 0); } catch { }
        }

        _monitorThread?.Join(2000);
        _monitorThread = null;
        _monitorThreadId = 0;
        _hwnd = nint.Zero;
    }

    private void MonitorThreadProc()
    {
        _monitorThreadId = Win32Helper.GetCurrentThreadId();

        _hwnd = Win32Helper.CreateWindowEx(
            0,
            "STATIC",
            "",
            0,
            0, 0, 0, 0,
            Win32Helper.HWND_MESSAGE,
            nint.Zero,
            Win32Helper.GetModuleHandle(null),
            nint.Zero);

        if (_hwnd == nint.Zero)
        {
            _monitorReady.Set();
            return;
        }

        _wndProc = MessageWindowProc;
        _originalWndProc = Win32Helper.SetWindowLongPtr(
            _hwnd,
            Win32Helper.GWLP_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(_wndProc));

        Win32Helper.AddClipboardFormatListener(_hwnd);
        _monitorReady.Set();

        while (_monitorRunning)
        {
            var r = Win32Helper.GetMessage(out var msg, nint.Zero, 0, 0);
            if (r <= 0) break;

            Win32Helper.TranslateMessage(ref msg);
            Win32Helper.DispatchMessage(ref msg);
        }

        if (_hwnd != nint.Zero)
        {
            try { Win32Helper.RemoveClipboardFormatListener(_hwnd); } catch { }
            try { Win32Helper.DestroyWindow(_hwnd); } catch { }
        }

        _hwnd = nint.Zero;
    }

    private nint MessageWindowProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == Win32Helper.WM_CLIPBOARDUPDATE)
        {
            HandleClipboardUpdate();
            return nint.Zero;
        }

        return Win32Helper.CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    private void HandleClipboardUpdate()
    {
        if (!_monitoring || _disposed) return;

        try
        {
            if (Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_HDROP))
            {
                var files = ReadFiles();
                if (files is { Count: > 0 })
                {
                    Copied?.Invoke(new ClipboardContents(ClipboardItemType.Files, Files: files));
                    return;
                }
            }

            if (Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_UNICODETEXT))
            {
                var text = ReadText();
                if (!string.IsNullOrEmpty(text))
                {
                    Copied?.Invoke(new ClipboardContents(ClipboardItemType.Text, Text: text));
                    return;
                }
            }

            if (Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_DIB) ||
                Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_DIBV5))
            {
                var image = ReadImage();
                if (image is not null)
                    Copied?.Invoke(new ClipboardContents(ClipboardItemType.Image, Image: image));
            }
        }
        catch { }
    }

    public string? ReadText()
    {
        if (!Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_UNICODETEXT)) return null;
        if (!Win32Helper.OpenClipboard(nint.Zero)) return null;

        try
        {
            var hData = Win32Helper.GetClipboardData(Win32Helper.CF_UNICODETEXT);
            if (hData == nint.Zero) return null;

            var ptr = Win32Helper.GlobalLock(hData);
            if (ptr == nint.Zero) return null;

            try { return Marshal.PtrToStringUni(ptr); }
            finally { Win32Helper.GlobalUnlock(hData); }
        }
        finally { Win32Helper.CloseClipboard(); }
    }

    public SKImage? ReadImage()
    {
        if (!Win32Helper.OpenClipboard(nint.Zero)) return null;

        try
        {
            var format = Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_DIBV5)
                ? Win32Helper.CF_DIBV5
                : Win32Helper.CF_DIB;

            var hData = Win32Helper.GetClipboardData(format);
            if (hData == nint.Zero) return null;

            var ptr = Win32Helper.GlobalLock(hData);
            if (ptr == nint.Zero) return null;

            try { return DecodeDib(ptr); }
            finally { Win32Helper.GlobalUnlock(hData); }
        }
        finally { Win32Helper.CloseClipboard(); }
    }

    private static SKImage? DecodeDib(nint ptr)
    {
        var biSize = (uint)Marshal.ReadInt32(ptr, 0);
        var biWidth = Marshal.ReadInt32(ptr, 4);
        var biHeight = Marshal.ReadInt32(ptr, 8);
        var biBitCount = (ushort)Marshal.ReadInt16(ptr, 14);
        var biCompression = (uint)Marshal.ReadInt32(ptr, 16);

        if (biCompression != 0) return null;
        if (biWidth <= 0 || biHeight == 0) return null;

        var topDown = biHeight < 0;
        var height = Math.Abs(biHeight);

        var srcRowBytes = biBitCount switch
        {
            32 => biWidth * 4,
            24 => ((biWidth * 3) + 3) & ~3,
            _ => 0
        };

        if (srcRowBytes == 0) return null;

        var pixelOffset = (int)biSize;

        var info = new SKImageInfo(biWidth, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var bmp = new SKBitmap(info);

        var dstSpan = bmp.GetPixelSpan();
        var dstRowBytes = biWidth * 4;
        var rowBuffer = new byte[dstRowBytes];

        for (var y = 0; y < height; y++)
        {
            var srcY = topDown ? y : (height - 1 - y);
            var srcRowPtr = ptr + pixelOffset + (srcY * srcRowBytes);
            var dstOffset = y * dstRowBytes;

            if (biBitCount == 32)
            {
                Marshal.Copy(srcRowPtr, rowBuffer, 0, dstRowBytes);
                rowBuffer.AsSpan().CopyTo(dstSpan.Slice(dstOffset, dstRowBytes));
            }
            else
            {
                for (var x = 0; x < biWidth; x++)
                {
                    dstSpan[dstOffset + (x * 4)] = Marshal.ReadByte(srcRowPtr, x * 3);
                    dstSpan[dstOffset + (x * 4) + 1] = Marshal.ReadByte(srcRowPtr, (x * 3) + 1);
                    dstSpan[dstOffset + (x * 4) + 2] = Marshal.ReadByte(srcRowPtr, (x * 3) + 2);
                    dstSpan[dstOffset + (x * 4) + 3] = 255;
                }
            }
        }

        return SKImage.FromBitmap(bmp);
    }

    public IReadOnlyList<string>? ReadFiles()
    {
        if (!Win32Helper.IsClipboardFormatAvailable(Win32Helper.CF_HDROP)) return null;
        if (!Win32Helper.OpenClipboard(nint.Zero)) return null;

        try
        {
            var hDrop = Win32Helper.GetClipboardData(Win32Helper.CF_HDROP);
            if (hDrop == nint.Zero) return null;

            var count = Win32Helper.DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
            if (count == 0) return null;

            var list = new List<string>((int)count);

            for (uint i = 0; i < count; i++)
            {
                var len = Win32Helper.DragQueryFile(hDrop, i, null, 0);
                if (len == 0) continue;

                var buf = new char[len + 1];
                if (Win32Helper.DragQueryFile(hDrop, i, buf, (uint)buf.Length) == 0) continue;

                list.Add(new string(buf, 0, (int)len));
            }

            return list;
        }
        finally { Win32Helper.CloseClipboard(); }
    }

    public void WriteText(string text)
    {
        if (text is null) return;

        var bytes = (text.Length + 1) * 2;
        var hMem = Win32Helper.GlobalAlloc(Win32Helper.GMEM_MOVEABLE, (UIntPtr)bytes);
        if (hMem == nint.Zero) return;

        var ptr = Win32Helper.GlobalLock(hMem);
        if (ptr == nint.Zero) { Win32Helper.GlobalFree(hMem); return; }

        try
        {
            var chars = text.ToCharArray();
            Marshal.Copy(chars, 0, ptr, chars.Length);
        }
        finally { Win32Helper.GlobalUnlock(hMem); }

        if (!Win32Helper.OpenClipboard(nint.Zero))
        {
            Win32Helper.GlobalFree(hMem);
            return;
        }

        try
        {
            Win32Helper.EmptyClipboard();
            if (Win32Helper.SetClipboardData(Win32Helper.CF_UNICODETEXT, hMem) == nint.Zero)
                Win32Helper.GlobalFree(hMem);
        }
        finally { Win32Helper.CloseClipboard(); }
    }

    public void WriteImage(SKImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using var bmp = SKBitmap.FromImage(image);
        var width = bmp.Width;
        var height = bmp.Height;

        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var bgra = new SKBitmap(info);
        if (!bmp.CopyTo(bgra, SKColorType.Bgra8888)) return;

        var pixels = bgra.Bytes;
        var rowBytes = width * 4;
        var pixelBytes = rowBytes * height;
        var totalBytes = 40 + pixelBytes;

        var hMem = Win32Helper.GlobalAlloc(Win32Helper.GMEM_MOVEABLE, (UIntPtr)totalBytes);
        if (hMem == nint.Zero) return;

        var ptr = Win32Helper.GlobalLock(hMem);
        if (ptr == nint.Zero) { Win32Helper.GlobalFree(hMem); return; }

        try
        {
            var bmi = new Win32Helper.BITMAPINFOHEADER
            {
                biSize = 40,
                biWidth = width,
                biHeight = height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0,
                biSizeImage = (uint)pixelBytes,
            };

            Marshal.StructureToPtr(bmi, ptr, false);

            var dst = ptr + 40;
            for (var y = 0; y < height; y++)
            {
                var srcRow = (height - 1 - y) * rowBytes;
                Marshal.Copy(pixels, srcRow, dst + (y * rowBytes), rowBytes);
            }
        }
        finally { Win32Helper.GlobalUnlock(hMem); }

        if (!Win32Helper.OpenClipboard(nint.Zero))
        {
            Win32Helper.GlobalFree(hMem);
            return;
        }

        try
        {
            Win32Helper.EmptyClipboard();
            if (Win32Helper.SetClipboardData(Win32Helper.CF_DIB, hMem) == nint.Zero)
                Win32Helper.GlobalFree(hMem);
        }
        finally { Win32Helper.CloseClipboard(); }
    }

    public void WriteFiles(IReadOnlyList<string> files)
    {
        if (files is null || files.Count == 0) return;

        const int dropFilesSize = 20;

        var pathsSize = 0;
        foreach (var f in files) pathsSize += (f.Length + 1) * 2;
        pathsSize += 2;

        var totalBytes = dropFilesSize + pathsSize;
        var hMem = Win32Helper.GlobalAlloc(Win32Helper.GMEM_MOVEABLE, (UIntPtr)totalBytes);
        if (hMem == nint.Zero) return;

        var ptr = Win32Helper.GlobalLock(hMem);
        if (ptr == nint.Zero) { Win32Helper.GlobalFree(hMem); return; }

        try
        {
            var drop = new Win32Helper.DROPFILES
            {
                pFiles = dropFilesSize,
                pt = new Win32Helper.POINT { X = 0, Y = 0 },
                fNC = 0,
                fWide = 1
            };

            Marshal.StructureToPtr(drop, ptr, false);

            var offset = dropFilesSize;
            foreach (var file in files)
            {
                var chars = (file + "\0").ToCharArray();
                Marshal.Copy(chars, 0, ptr + offset, chars.Length);
                offset += chars.Length * 2;
            }
        }
        finally { Win32Helper.GlobalUnlock(hMem); }

        if (!Win32Helper.OpenClipboard(nint.Zero))
        {
            Win32Helper.GlobalFree(hMem);
            return;
        }

        try
        {
            Win32Helper.EmptyClipboard();
            if (Win32Helper.SetClipboardData(Win32Helper.CF_HDROP, hMem) == nint.Zero)
                Win32Helper.GlobalFree(hMem);
        }
        finally { Win32Helper.CloseClipboard(); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try { StopMonitoring(); } catch { }

        _monitorReady.Dispose();
    }
}