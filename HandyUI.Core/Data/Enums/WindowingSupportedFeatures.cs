namespace HandyUI.Core.Data.Enums;

[Flags]
public enum WindowingSupportedFeatures : ulong
{
    None = 0,
    Title = 1UL << 0,
    Resize = 1UL << 1,
    Icon = 1UL << 2,
    Cursor = 1UL << 3,
    MemoryCleansing = 1UL << 4,
    FullySupported = ulong.MaxValue
}