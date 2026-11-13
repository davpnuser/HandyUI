# `IControl`

`HandyUI.Core.Abstractions.Interface.IControl`

Contract for every drawable / interactive control.

```csharp
public interface IControl : IDisposable
```

## Properties

### Identity

```csharp
string?   Name   { get; set; }
IControl? Parent { get; set; }
IReadOnlyList<IControl> Children { get; }
```

`Name` is purely for debugging - the renderer never reads it. `Parent` is set
through the fluent builder or by assigning directly; assigning reassigns
child lists on both sides.

### Layout

```csharp
SKPoint Location { get; set; }   // offset from parent
SKRect  Bounds   { get; set; }   // local rectangle
int     ZIndex   { get; set; }   // higher = later + hit first
```

### Behaviour flags

```csharp
bool Enabled   { get; set; }   // default: true
bool Visible   { get; set; }   // default: true
bool ClipChildren { get; set; } // clips subtree + blocks input outside Bounds
bool Focusable { get; set; }    // default: false - needed for keyboard focus
```

!!! tip "Focusable"
    Setting `Focusable = true` is required before `RequestFocus()` or Tab
    navigation will target the control. Clicking a `Focusable` control sets
    focus automatically; clicking a non-focusable one clears it.

### State (managed by the renderer)

```csharp
bool IsDisposed { get; }
bool IsHovered  { get; }
bool IsMouseDown { get; }
bool IsFocused  { get; internal set; }
```

`IsHovered` / `IsMouseDown` are updated in `ProcessMouseEvent`. `IsFocused`
is set by `UIRenderer` when the control gains or loses focus.

### Rendering

```csharp
float   Opacity   { get; set; }   // clamped to [0, 1]
SKPaint AlphaPaint { get; }        // pre-built White * Opacity paint
```

## Events

```csharp
event Action? WhenInvalidated;
event Action<IControl?, bool>? WhenFocusRequested;
```

`WhenFocusRequested` fires with `(target, fromKeyboard)`. `fromKeyboard`
is `true` when the change was driven by `FocusNext()` / `FocusPrevious()`
(Tab navigation) and `false` when driven by mouse / `RequestFocus()`. The
renderer uses it to draw a focus rectangle only for keyboard focus.

## Overrideable members

```csharp
abstract bool DoesIntersect(SKPoint point);     // hit test in local coords
abstract void OnDraw(SKCanvas canvas);     // canvas already translated
void OnUpdate(UpdateContext updateContext);     // not forced anymore
```

## Methods

| Method | Description |
| --- | --- |
| `Invalidate()` | Bubble `WhenInvalidated` up the tree. No-op when hidden / disposed. |
| `RequestFocus()` | Raises `WhenFocusRequested(this, false)`. Guarded by `Enabled`, `Visible`, `Focusable`. |
| `ProcessMouseEvent(MouseEventContext)` | Updates `IsHovered`/`IsMouseDown`; calls `OnMouse`. |
| `ProcessKeyEvent(KeyEventContext)` | Calls `OnKey` if `Visible && Enabled && IsFocused`. |
| `GetGlobalLocation()` | Absolute `SKPoint` by summing ancestors' `Location`. |
| `GetLocalPosition(SKPoint)` | Converts a global point into this control's local space. |