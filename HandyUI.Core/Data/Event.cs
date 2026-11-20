using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Helpers;

namespace HandyUI.Core.Data;

public enum EventType
{
    None = 0,
    Mouse = 1,
    Keyboard = 2,
    Controller = 3,
    Render = 4,
    Update = 5,
}

public readonly record struct EventData(
    IHostWindow HostWindow,
    TweenManager TweenManager,
    double TotalTime
);