# Controls

Built-in controls, all deriving from [`BaseControl<TSelf>`](../base-control.md).
They live in `HandyUI.Controls`, under the `HandyUI.Controls.Controls`
namespace (and `HandyUI.Controls.Enums` for related enums).

| Control | Purpose |
| --- | --- |
| [`Button`](button.md) | Filled text button with hover and ripple. |
| [`CheckBox`](check-box.md) | Checkbox with text and animated checkmark. |
| [`Frame`](frame.md) | Rounded rectangle with elevation, border, shadow. |
| [`ItemBox`](item-box.md) | Selectable list item with exclusive selection. |
| [`Label`](label.md) | Non-interactive text label. |
| [`ListBox`](list-box.md) | Scrollable stack of child controls. |
| [`MultilineTextInput`](multiline-text-input.md) | Multi-line text editor. |
| [`Picture`](picture.md) | Non-interactive image. |
| [`PictureButton`](picture-button.md) | Clickable image. |
| [`ProgressBar`](progress-bar.md) | Determinate or indeterminate progress. |
| [`ScrollFrame`](scroll-frame.md) | Scrollable viewport. |
| [`Slider`](slider.md) | Draggable value slider. |
| [`TextInput`](text-input.md) | Single-line text editor. |
| [`Toggle`](toggle.md) | On/off switch. |

## Installation

```bash
dotnet add package HandyUI.Core
dotnet add package HandyUI.Controls
```

## Common conventions

- **Autosizing** - every control runs `ResizeToContent()` in its constructor
  and whenever relevant properties change. Leave `Width` and `Height` at
  their defaults (usually `0`) to auto-size to content.
- **Theming** - controls read colors from
  [`ThemeManager`](../theme-manager.md) at draw time, so changing the OS
  theme updates every control live. Controls that also expose a `Theme`
  property use it for per-control overrides.
- **Events** - callback events follow the `WhenXxx` naming convention
  (`WhenClicked`, `WhenToggled`, `WhenTextChanged`, …). Fluent helpers
  attach to them directly (`OnClicked(...)`, `OnToggled(...)`).
- **Animations** - hover, ripple, focus, and checkmark animations use
  [`Tweener`](../tweener.md). They degrade gracefully if no tweener is
  available (values snap to target).
- **Disposal** - every control disposes its internal `SKPaint` and
  `SKTypeface` instances in `OnDispose`.