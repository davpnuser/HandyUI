using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Helpers;

namespace HandyUI.Core.Data;

public readonly record struct UpdateContext(
    TweenManager TweenManager,
    IHostWindow Host,
    float DeltaTime
);