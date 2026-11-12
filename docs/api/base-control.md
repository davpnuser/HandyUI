# `BaseControl`

`HandyUI.Core.Abstractions.Base.BaseControl`

Base class for every drawable control. Implements all of `IControl` plus a
children tree, focus rect painting, and disposal.

```csharp
public abstract class BaseControl : IControl
```

## Properties

### Standard (from `IControl`)

All of `IControl.Name`, `Parent`, `Children`, `Location`, `Bounds`,
`ZIndex`, `Enabled`, `Visible`, `ClipChildren`, `Focusable`,
`Opacity`, `AlphaPaint`, `IsHovered`, `IsMouseDown`, `IsFocused`,
`IsDisposed` are implemented.

!!! info "Changed from previous versions"
    `Enabled` replaces `IsEnabled`; `Visible` replaces `IsVisible`;
    `ClipChildren` replaces `ScissoringEnabled`. `Parent` is typed as
    `IControl?`.

### `Children`

```csharp
public IReadOnlyList<IControl> Children { get; }
```

Backed by a `List<IControl>`. Do not mutate directly - set a child's `Parent`
instead.

### `Opacity` / `AlphaPaint`

```csharp
public float   Opacity   { get; set; }   // clamped [0, 1]
public SKPaint AlphaPaint { get; }        // Color = White × Opacity
```

Setting `Opacity` updates `AlphaPaint.Color` and invalidates.

## Events

```csharp
public event Action? WhenInvalidated;
public event Action<IControl?, bool>? WhenFocusRequested;
```

## Overrideable members

```csharp
public abstract bool DoesIntersect(SKPoint point);
public abstract void OnDraw(SKCanvas canvas);
public abstract void OnUpdate(UpdateContext updateContext);

protected virtual bool OnMouse(MouseEventContext eventContext) => false;
protected virtual bool OnKey(KeyEventContext eventContext) => false;

protected virtual void OnChildAdded(IControl child) { }
protected virtual void OnChildRemoved(IControl child) { }
protected virtual void OnDrawFocusRect(SKCanvas canvas);   // default: dashed-less accent stroke
protected virtual void OnDispose() { }
```

## Methods

| Method | Description |
| --- | --- |
| `Invalidate()` | Raises `WhenInvalidated` and bubbles to `Parent.Invalidate()`. No-op if disposed or `!Visible`. |
| `RequestFocus()` | Raises `WhenFocusRequested(this, false)` if `Enabled && Visible && Focusable`. |
| `InvalidateChildrenOrder()` | Marks child list dirty (re-sorts by `ZIndex` next frame). Rare. |
| `DrawFocusRect(SKCanvas)` | Draws the keyboard focus rectangle if `IsFocused`. Called by the renderer. |
| `GetGlobalLocation()` | Sum of `Location` across ancestors. |
| `GetLocalPosition(SKPoint)` | Global → local. |
| `ProcessMouseEvent(...)` | Standard `IControl` behavior; also updates `IsHovered`/`IsMouseDown`. |
| `ProcessKeyEvent(...)` | Standard `IControl` behavior. |

## Fluent builder: `BaseControl<TSelf>`

```csharp
public abstract class BaseControl<TSelf> : BaseControl where TSelf : BaseControl<TSelf>
```

### `DoAndReturn(Action<TSelf>)`

```csharp
public TSelf DoAndReturn(Action<TSelf> action);
```

Invokes the action with `this` and returns `this` for chaining. Every
`WithXxx` helper is a one-liner built on top of this.

```csharp
button.DoAndReturn(b => b.Text = "Save");
```

### Fluent property setters

| Method | Sets |
| --- | --- |
| `WithName(string)` | `Name` |
| `WithParent(IControl)` / `WithParent(BaseControl)` | `Parent` |
| `WithLocation(SKPoint)` / `WithLocation(float, float)` | `Location` |
| `WithBounds(SKRect)` / `WithBounds(SKSize)` / `WithBounds(float, float, float, float)` | `Bounds` |
| `WithOpacity(float)` | `Opacity` |
| `WithVisible(bool)` | `Visible` |
| `WithEnabled(bool)` | `Enabled` |
| `WithZIndex(int)` | `ZIndex` |
| `WithFocusable(bool)` | `Focusable` |
| `WithAddToRenderer(UIRenderer)` | `renderer.AddControl(this)` |

## Example

```csharp
public sealed class ClickBox : BaseControl<ClickBox>
{
    public ClickBox() { Focusable = true; }

    public event Action? WhenClicked;

    public override bool DoesIntersect(SKPoint point) => Bounds.Contains(point);

    public override void OnUpdate(UpdateContext updateContext) { }

    public override void OnDraw(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = IsHovered ? SKColors.Orange : SKColors.CornflowerBlue
        };
        canvas.DrawRect(Bounds, paint);
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (eventContext.Type != MouseEventType.ClickEvent) return false;
        WhenClicked?.Invoke();
        return true;
    }
}
```