using HandyUI.Core.Data.Input;
using SkiaSharp;

namespace HandyUI.Core.Data;

public readonly record struct UpdateContext(
    EventData EventData,
    ModifierKeys ModifierKeys,
    SKPoint MousePosition,
    float DeltaTime
);