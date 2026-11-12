# Controls

All controls derive from [`BaseControl<TSelf>`](../base-control.md) and live
in the `HandyUI.Controls` package, under `HandyUI.Controls.Controls`.
Controls-specific enums are in `HandyUI.Controls.Enums`.

```bash
dotnet add package HandyUI.Core
dotnet add package HandyUI.Controls
```

## Conventions

- **Autosizing** - each control runs `ResizeToContent()` in its constructor
  and whenever a relevant property changes. Leave `Width` and `Height` at
  their defaults (usually `0`) to auto-size.
- **Theming** - controls read colors from
  [`ThemeManager`](../theme-manager.md) at draw time, so changing the OS
  theme recolors everything live.
- **Events** - every callback event is named `WhenXxx`; each has a fluent
  `WithWhenXxx(...)` helper (e.g. `WithWhenClicked`, `WithWhenToggled`).
- **Animation** - hover / ripple / focus / checkmark animations use
  `TweenManager` via `UpdateContext.TweenManager` (in `OnUpdate`) or
  `MouseEventContext.TweenManager` (in `OnMouse`). If the tween manager is
  missing (a null `Tween`), values snap to their target.
- **Focus** - text inputs and toggles call `RequestFocus()` and rely on
  `Focusable = true`. Most display controls leave `Focusable = false`.
- **Disposal** - every control disposes its paints and typeface in
  `OnDispose()`.

## Enums

- [`BorderDirection`](enums.md#borderdirection)
- [`StretchMode`](enums.md#stretchmode)