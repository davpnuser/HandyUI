namespace HandyUI.Core.Data;

public enum DeviceType
{
    Generic = 0,
    Desktop = 1,
    Mobile = 2,
    Console = 3,
    Embedded = 4,
}

public enum DisplayOrientation
{
    Unknown = 0,
    Landscape = 1,
    Portrait = 2
}

public readonly record struct DeviceInfo(
    DeviceType DeviceType,
    DisplayOrientation DisplayOrientation,
    float DPI
);