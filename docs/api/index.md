# API Reference

The public API of HandyUI is split across three packages:

- **`HandyUI.Core`** - renderer, controls base class, modules, helpers,
  theming, animation.
- **`HandyUI.Controls`** - a set of ready-to-use controls built on
  `HandyUI.Core`.
- **`HandyUI.SilkNet`** - a windowing adapter that hosts a `UIRenderer`
  inside a GLFW-backed window.

## Core

| Type | Summary |
| --- | --- |
| [`UIRenderer`](ui-renderer.md) | Render loop, input dispatcher, control/module registry, animation host. |
| [`ResourceManager`](resource-manager.md) | Loads resources from disk or embedded assembly resources. |
| [`FontManager`](font-manager.md) | Loads, caches, and exposes `SKTypeface` fonts. |
| [`BaseControl`](base-control.md) | Base class for all drawable controls. |
| [`BaseModule`](base-module.md) | Base class for logic-only modules. |

## Interfaces

| Type | Summary |
| --- | --- |
| [`IControl`](icontrol.md) | Contract implemented by every control. |
| [`IModule`](imodule.md) | Contract implemented by every module. |

## Animation

| Type | Summary |
| --- | --- |
| [`Tweener`](tweener.md) | Frame-driven float interpolation with easing. |
| [`TweenHandle`](tweener.md#tweenhandle) | Handle for pausing, reversing, and cancelling a tween. |
| [`EaseType`](tweener.md#easetype) | Built-in easing curves. |

## Theming

| Type | Summary |
| --- | --- |
| [`Theme`](theme.md) | Plain palette record with public `SKColor` fields. |
| [`ThemeManager`](theme-manager.md) | Singleton theme manager and OKLCH tone generator. |
| [`ThemeMode`](theme-manager.md#thememode) | `Dark`, `Light`, or `Unknown`. |
| [`Appearance`](theme-helper.md#appearance) | OS accent color + theme mode. |

## Data

| Type | Summary |
| --- | --- |
| [`MouseEventContext`](contexts.md#mouseeventcontext) | Describes a mouse input event. |
| [`KeyEventContext`](contexts.md#keyeventcontext) | Describes a keyboard input event. |

## Enums

| Type | Summary |
| --- | --- |
| [`BorderDirection`](border-direction.md) | Where a stroke is drawn relative to its shape. |
| [`StretchMode`](stretch-mode.md) | How `Picture` and `PictureButton` fit their image. |

## Helpers

| Type | Summary |
| --- | --- |
| [`SKColorHelper`](sk-color-helper.md) | Hue/brightness/transparency/lerp helpers for `SKColor`. |
| [`SKRectHelper`](sk-rect-helper.md) | Conversions and division helpers for SkiaSharp geometry. |

## Controls

`HandyUI.Controls` - ready-made controls, all deriving from
[`BaseControl<TSelf>`](base-control.md).

| Type | Summary |
| --- | --- |
| [`Button`](controls/button.md) | Filled text button with hover and ripple. |
| [`CheckBox`](controls/check-box.md) | Checkbox with text and animated checkmark. |
| [`Frame`](controls/frame.md) | Rounded rectangle with elevation, border, shadow. |
| [`ItemBox`](controls/item-box.md) | Selectable list item with exclusive selection. |
| [`Label`](controls/label.md) | Non-interactive text label. |
| [`ListBox`](controls/list-box.md) | Scrollable stack of child controls. |
| [`MultilineTextInput`](controls/multiline-text-input.md) | Multi-line text editor with selection. |
| [`Picture`](controls/picture.md) | Non-interactive image with stretch modes. |
| [`PictureButton`](controls/picture-button.md) | Clickable image with hover and ripple. |
| [`ProgressBar`](controls/progress-bar.md) | Determinate or indeterminate progress. |
| [`ScrollFrame`](controls/scroll-frame.md) | Scrollable viewport with auto-hiding scrollbar. |
| [`Slider`](controls/slider.md) | Draggable value slider. |
| [`TextInput`](controls/text-input.md) | Single-line text editor with selection. |
| [`Toggle`](controls/toggle.md) | On/off switch with animated thumb. |

## Silk.NET Adapter

`HandyUI.SilkNet` - the windowing/hosting layer.

| Type | Summary |
| --- | --- |
| [`HandyWindow`](handy-window.md) | GLFW-backed window with a pump thread. |
| [`WindowConfiguration`](window-configuration.md) | Options record for `HandyWindow`. |
| [`ThemeHelper`](theme-helper.md) | Reads OS accent color and dark/light mode. |
| [`RendererHelper`](silknet-helpers.md#rendererhelper) | Attaches a `UIRenderer` to a `HandyWindow`. |
| [`WindowHelper`](silknet-helpers.md#windowhelper) | Window/renderer factories and platform tweaks. |
| [`Win32Helper`](silknet-helpers.md#win32helper) | Native Win32 window tweaks. |
| [`VectorExtensions`](vector-extensions.md) | Conversions between SkiaSharp and Silk.NET types. |