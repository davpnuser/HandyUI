# Enums

All shared enums live in `HandyUI.Core.Data.Enums`. Controls-specific enums
live in `HandyUI.Controls.Enums` (see [Controls → Enums](controls/enums.md)).

## `MouseEventType`

```csharp
public enum MouseEventType
{
    None = 0,
    ClickEvent = 1,
    MouseDownEvent = 2,
    MouseUpEvent = 3,
    ScrollEvent = 4,
    MoveEvent = 5
}
```

## `MouseButton`

```csharp
public enum MouseButton
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 3,
    Wheel = 4,
    Generic = 5,
}
```

## `KeyEventType`

```csharp
public enum KeyEventType
{
    None = 0,
    KeyDown = 1,
    KeyUp = 2,
    CharInput = 3
}
```

## `CursorType`

```csharp
public enum CursorType
{
    Default = 0, Arrow = 1, Hand = 2, IBeam = 3,
    Crosshair = 4, BusyArrow = 5, Busy = 6,
    HResize = 7, VResize = 8, NwseResize = 9,
    NeswResize = 10, ResizeAll = 11, NotAllowed = 12
}
```

Set on [`IHostWindow.Cursor`](ihostwindow.md) - the adapter maps it to the
native cursor.

## `DeviceType`

```csharp
public enum DeviceType
{
    Generic = 0, Desktop = 1, Mobile = 2, BigScreen = 3, Embedded = 4
}
```

## `DisplayOrientation`

```csharp
public enum DisplayOrientation
{
    Unknown = 0, Landscape = 1, Portrait = 2
}
```

`HandyWindow` sets this based on `Size.Width` vs `Size.Height` and raises
`WhenOrientationChanged` whenever it changes.

## `ThemeMode`

```csharp
public enum ThemeMode { Dark, Light, Unknown }
```

`Unknown` is used when the OS appearance can't be read (unsupported
platform). `ThemeManager` treats it as `Light`.

## `ProfilerAnchor`

```csharp
public enum ProfilerAnchor { None, TopLeft, TopRight, BottomLeft, BottomRight }
```

Reserved for future profiler positioning.

## `EaseType`

See [`TweenManager → EaseType`](tween-manager.md#easetype).

## `TweenStatus`

See [`Tween`](tween.md#tweenstatus).