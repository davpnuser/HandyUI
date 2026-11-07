using HandyUI.Core.v1.Components;
using HandyUI.Core.v1.Enums;
using SkiaSharp;

namespace HandyUI.Core.v1.Data;

public record MouseEventContext(
    SKPoint Position,
    MouseEventType Type,
    MouseButton Button = MouseButton.None,
    int WheelDelta = 0,
    Tweener Tweener = null!);