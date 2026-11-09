using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using HandyUI.Core.v1.Interface;
using SkiaSharp;

namespace HandyUI.Core.Data;

public record UpdateContext(
    TweenManager Tweener,
    IWindowHost Host,
    float DeltaTime,
    SKPoint MousePosition,
    DisplayOrientation Orientation = DisplayOrientation.Unknown,
    float DpiScale = 1.0f,
    float DisplayDist = 1.0f
);