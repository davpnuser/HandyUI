# `UIControlBase`

`HandyUI.Core.Classes.Base.UIControlBase`

The base class for every drawable control. Provides transformation, geometry,
visual state, interaction state, a parent/children tree, and invalidation
events.

```csharp
public abstract class UIControlBase : IUIControl
```

## Properties

### `Children`

```csharp
public IReadOnlyList<IUIControl> Children { get; }
```

Do **not** mutate directly. Set a child's `Parent` instead.

### `Parent`

```csharp
public UIControlBase? Parent { get; set; }
```

Setting this detaches from the old parent, attaches to the new one, and
invalidates both sides.

### `Location`

```csharp
public SKPoint Location { get; set; }
```

Offset relative to the parent, applied only when
`ParentChildPositioningEnabled` is `true`.

### `Bounds`

```csharp
public SKRect Bounds { get; set; }
```

The control's rectangle in **its own** coordinate space.

### `ZIndex`

```csharp
public int ZIndex { get; set; }
```

Higher `ZIndex` = drawn later (on top) and hit-tested first.

!!! tip "Sorting is lazy"
    Children are sorted on demand after a change. Calling
    `InvalidateChildrenOrder()` is rarely necessary.

### `ParentChildPositioningEnabled`

```csharp
public bool ParentChildPositioningEnabled { get; set; }   // default: true
```

When `false`, the control ignores its `Location` when resolving absolute
positions.

### `ScissoringEnabled`

```csharp
public bool ScissoringEnabled { get; set; }   // default: true
```

When `true`, the renderer clips the subtree to `Bounds` and skips dispatching
events to out-of-bounds children.

### `AlphaPaint`

```csharp
public SKPaint AlphaPaint { get; }
```

A pre-built paint with `Color = White * Opacity`. Handy for tinting layers:

```csharp
canvas.SaveLayer(AlphaPaint);
// draw children...
canvas.Restore();
```

### `Opacity`

```csharp
public float Opacity { get; set; }   // clamped to [0, 1]
```

Setting this updates `AlphaPaint.Color` and invalidates.

### Interaction state

```csharp
public bool IsHovered  { get; private set; }
public bool IsMouseDown { get; private set; }
public bool IsFocused  { get; set; }
```

`IsHovered` and `IsMouseDown` are managed by the renderer. `IsFocused` is
managed by the renderer but exposed as settable so custom controls can
reflect it.

### `IsVisible` / `IsEnabled`

```csharp
public bool IsVisible { get; set; }   // default: true
public bool IsEnabled { get; set; }   // default: true
```

- `IsVisible == false` → the control and its children are not drawn or hit.
- `IsEnabled == false` → the control is not hit, not keyboard-interactive,
  and cannot request focus.

## Events

### `Invalidated`

```csharp
public event Action? Invalidated;
```

Raised when the control's visual state changes. The renderer subscribes to
keep its dirty flag accurate.

### `FocusRequested`

```csharp
public event Action<IUIControl>? FocusRequested;
```

Raised by `RequestFocus()`. The renderer listens and sets focus accordingly.

## Methods

### `Invalidate()`

```csharp
public void Invalidate()
```

Raises `Invalidated` and bubbles up to the parent. Call after mutating state
outside of the built-in properties.

### `RequestFocus()`

```csharp
public void RequestFocus()
```

Asks the renderer to focus this control. No-op when the control is disabled
or invisible.

### `ProcessMouseEvent(MouseEventContext)`

```csharp
public bool ProcessMouseEvent(MouseEventContext mouseContext)
```

Updates `IsHovered` and `IsMouseDown`, then calls `OnMouse`. You generally
don't call this yourself.

### `ProcessKeyEvent(KeyEventContext)`

```csharp
public bool ProcessKeyEvent(KeyEventContext keyContext)
```

Returns `OnKey(keyContext)` if the control is visible, enabled, and focused;
otherwise `false`.

### Abstract methods

```csharp
public abstract bool Intersects(SKPoint clientPoint);
public abstract void Update(float deltaTime, SKPoint clientMousePosition);
public abstract void Draw(SKCanvas canvas);
```

- `Intersects` receives a point in the control's **local** space.
- `Update` runs once per frame **before** `Draw`. `clientMousePosition` is
  local (or `(-99999, -99999)` when clipped).
- `Draw` runs after `Update`; the canvas is already translated to the control
  origin when `ParentChildPositioningEnabled` is `true`.

## Virtual hooks

```csharp
protected virtual bool OnMouse(MouseEventContext mouseContext) => false;
protected virtual bool OnKey(KeyEventContext keyContext) => false;
protected virtual void OnChildAdded(IUIControl control) { }
protected virtual void OnChildRemoved(IUIControl control) { }
protected virtual void OnDispose() { }
```

## Fluent API: `UIControlBase<TSelf>`

```csharp
public abstract class UIControlBase<TSelf> : UIControlBase
    where TSelf : UIControlBase<TSelf>
```

Every `With...` method returns `TSelf`:

```csharp
new MyBox()
    .WithLocation(10, 10)
    .WithBounds(0, 0, 200, 100)
    .WithOpacity(0.85f)
    .WithZIndex(5)
    .WithAddToRenderer(renderer);
```

| Method | Sets |
| --- | --- |
| `WithLocation(SKPoint)` / `WithLocation(float, float)` | `Location` |
| `WithBounds(SKRect)` / `WithBounds(SKSize)` / `WithBounds(float, float, float, float)` | `Bounds` |
| `WithParent(UIControlBase)` | `Parent` |
| `WithOpacity(float)` | `Opacity` |
| `WithIsVisible(bool)` | `IsVisible` |
| `WithIsEnabled(bool)` | `IsEnabled` |
| `WithParentChildPositioningEnabled(bool)` | `ParentChildPositioningEnabled` |
| `WithZIndex(int)` | `ZIndex` |
| `WithAddToRenderer(UIRenderer)` | `renderer.AddRootControl(this)` |

## Example

```csharp
public sealed class ClickBox : UIControlBase<ClickBox>
{
    public event Action? Clicked;

    public override bool Intersects(SKPoint p) => Bounds.Contains(p);

    public override void Update(float dt, SKPoint mouse) { }

    public override void Draw(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = IsHovered ? SKColors.Orange : SKColors.CornflowerBlue
        };
        canvas.DrawRect(Bounds, paint);
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.Click)
        {
            Clicked?.Invoke();
            return true;
        }
        return false;
    }
}

new ClickBox()
    .WithBounds(0, 0, 160, 48)
    .WithAddToRenderer(renderer);
```