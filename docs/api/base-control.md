# `BaseControl`

`HandyUI.Core.Base.BaseControl`

The base class for every drawable control. Provides transformation, geometry,
visual state, interaction state, and a parent/children tree.

```csharp
public abstract class BaseControl : IControl
```

!!! info "Nullable mouse position"
    `DoesIntersect` and `OnUpdate` receive `SKPoint?`. **`null`** means *"the
    cursor is not over this subtree"* (clipped out). Custom controls should
    treat `null` as not hovered.

## Properties

### `Children`

```csharp
public IReadOnlyList<IControl> Children { get; }
```

Read-only. Attach/detach via `Parent`.

### `Parent`

```csharp
public BaseControl? Parent { get; set; }
```

Setting this detaches from the old parent, attaches to the new one, and
invalidates both.

### `Location` / `Bounds`

```csharp
public SKPoint Location { get; set; }
public SKRect  Bounds   { get; set; }
```

`Location` is the offset from the parent (applied only when
`ParentChildPositioningEnabled` is `true`). `Bounds` is the rectangle in
this control's local space.

### `ZIndex`

```csharp
public int ZIndex { get; set; }
```

Higher = drawn later (on top) and hit-tested first. Children are sorted
lazily; `InvalidateChildrenOrder()` is rarely needed.

### `ParentChildPositioningEnabled` / `ScissoringEnabled`

```csharp
public bool ParentChildPositioningEnabled { get; set; }   // default: true
public bool ScissoringEnabled { get; set; }               // default: true
```

- `ParentChildPositioningEnabled` - whether `Location` is applied.
- `ScissoringEnabled` - whether the subtree is clipped to `Bounds` and skips
  input that falls outside.

### `AlphaPaint` / `Opacity`

```csharp
public SKPaint AlphaPaint { get; }
public float   Opacity   { get; set; }   // clamped to [0, 1]
```

`AlphaPaint` is a pre-built paint with `Color = White * Opacity` - useful for
`canvas.SaveLayer(AlphaPaint)`.

### Interaction state

```csharp
public bool IsHovered  { get; private set; }
public bool IsMouseDown { get; private set; }
public bool IsFocused  { get; set; }
```

`IsHovered` / `IsMouseDown` are updated by the renderer. `IsFocused` is
managed by the renderer but exposed as settable.

### `IsVisible` / `IsEnabled`

```csharp
public bool IsVisible { get; set; }   // default: true
public bool IsEnabled { get; set; }   // default: true
```

- `IsVisible == false` → not drawn, not hit.
- `IsEnabled == false` → not hit, not keyboard-interactive, cannot focus.

## Events

### `WhenInvalidated`

```csharp
public event Action? WhenInvalidated;
```

Raised when the control's visual state changes. `UIRenderer` subscribes to
this to keep its dirty flag accurate.

### `WhenFocusRequested`

```csharp
public event Action<IControl>? WhenFocusRequested;
```

Raised by `RequestFocus()`. The renderer listens and sets focus.

## Methods

### `Invalidate()`

```csharp
public void Invalidate();
```

Raises `WhenInvalidated` and bubbles up to the parent. Call after mutating
state outside of the built-in properties.

### `RequestFocus()`

```csharp
public void RequestFocus();
```

Asks the renderer to focus this control. No-op when disabled or invisible.

### `ProcessMouseEvent(MouseEventContext)`

```csharp
public bool ProcessMouseEvent(MouseEventContext mouseContext);
```

Updates `IsHovered`/`IsMouseDown`, then calls `OnMouse`. Normally only the
renderer calls this.

### `ProcessKeyEvent(KeyEventContext)`

```csharp
public bool ProcessKeyEvent(KeyEventContext keyContext);
```

Returns `OnKey(keyContext)` if the control is visible, enabled, and focused;
otherwise `false`.

### Abstract methods

```csharp
public abstract bool DoesIntersect(SKPoint? clientPoint);
public abstract void OnUpdate(float deltaTime, SKPoint? clientMousePosition);
public abstract void OnDraw(SKCanvas canvas);
```

- `DoesIntersect` - hit-test in local space (`null` → return `false`).
- `OnUpdate` - runs before `OnDraw`; `clientMousePosition` is local or `null`.
- `OnDraw` - canvas is already translated to the control's origin.

## Virtual hooks

```csharp
protected virtual bool OnMouse(MouseEventContext mouseContext) => false;
protected virtual bool OnKey(KeyEventContext keyContext) => false;
protected virtual void OnChildAdded(IControl control) { }
protected virtual void OnChildRemoved(IControl control) { }
protected virtual void OnDispose() { }
```

## Fluent API: `BaseControl<TSelf>`

```csharp
public abstract class BaseControl<TSelf> : BaseControl
    where TSelf : BaseControl<TSelf>
```

| Method | Sets |
| --- | --- |
| `WithLocation(SKPoint)` / `WithLocation(float, float)` | `Location` |
| `WithBounds(SKRect)` / `WithBounds(SKSize)` / `WithBounds(float x, float y, float w, float h)` | `Bounds` |
| `WithParent(BaseControl)` | `Parent` |
| `WithOpacity(float)` | `Opacity` |
| `WithIsVisible(bool)` | `IsVisible` |
| `WithIsEnabled(bool)` | `IsEnabled` |
| `WithParentChildPositioningEnabled(bool)` | `ParentChildPositioningEnabled` |
| `WithZIndex(int)` | `ZIndex` |
| `WithAddToRenderer(UIRenderer)` | `renderer.AddRootControl(this)` |

!!! tip "`WithBounds(x, y, w, h)` takes position + size"
    `WithBounds(10, 20, 100, 40)` creates an `SKRect` from `(10, 20)` to
    `(110, 60)`.

## Example

```csharp
public sealed class ClickBox : BaseControl<ClickBox>
{
    public event Action? Clicked;

    public override bool DoesIntersect(SKPoint? p)
        => p is { } value && Bounds.Contains(value);

    public override void OnUpdate(float dt, SKPoint? mouse) { }

    public override void OnDraw(SKCanvas canvas)
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

new ClickBox().WithBounds(0, 0, 160, 48).WithAddToRenderer(renderer);
```