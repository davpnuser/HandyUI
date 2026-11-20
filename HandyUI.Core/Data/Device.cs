namespace HandyUI.Core.Data;

public enum DeviceType
{
    Generic = 0,
    Desktop = 1,
    Mobile = 2,
    Console = 3,
    Embedded = 4,
}

public readonly record struct DeviceInfo(
    DeviceType DeviceType,
    float DPI
);