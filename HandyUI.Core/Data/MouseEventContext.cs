using HandyUI.Core.Enums;
using SkiaSharp;

namespace HandyUI.Core.Data;

public record MouseEventContext(
    SKPoint? ClientPosition,
    MouseEventType Type,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0
);