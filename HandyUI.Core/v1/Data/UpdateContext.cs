using HandyUI.Core.v1.Components;
using HandyUI.Core.v1.Enums;
using HandyUI.Core.v1.Interface;
using SkiaSharp;

namespace HandyUI.Core.v1.Data;

public record UpdateContext(
    IWindowHost Host,
    double DeltaTime,
    double TotalTime,
    Tweener Tweener,
    SKPoint MousePosition,
    float DpiScale,
    int DisplayDist,
    DisplayOrientation Orientation);