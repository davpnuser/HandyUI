# API Reference

HandyUI is split across three packages. Each layer builds on the one below.

| Package | Purpose |
| --- | --- |
| **`HandyUI.Core`** | Renderer, controls base, modules, theming, animation, helpers. |
| **`HandyUI.Controls`** | Ready-made controls built on Core. |
| **`HandyUI.SilkNet`** | GLFW-backed host window and Win32 integrations. |

The renderer no longer talks to GLFW directly - it targets the abstract
[`IHostWindow`](ihostwindow.md) interface. `HandyUI.SilkNet.HandyWindow` is the
reference implementation; any host that implements `IHostWindow` can drive a
`UIRenderer`.

## Core

| Type | Summary |
| --- | --- |
| [`IControl`](icontrol.md) | Drawable / focusable interface implemented by every control. |
| [`IModule`](imodule.md) | Logic-only interface (can process input before controls). |
| [`IHostWindow`](ihostwindow.md) | Hosting contract that a `UIRenderer` targets. |
| [`BaseControl`](base-control.md) | Base class for controls (fluent builder in `BaseControl<TSelf>`). |
| [`BaseModule`](base-module.md) | Base class for modules. |
| [`UIRenderer`](ui-renderer.md) | Render loop, input dispatcher, focus manager. |
| [`UpdateContext`](update-context.md) | Per-frame context (tweener, host, delta time). |
| [`MouseEventContext`](contexts.md#mouseeventcontext) | Mouse event with modifiers, click count, and the shared tweener. |
| [`KeyEventContext`](contexts.md#keyeventcontext) | Key event with modifiers. |
| [`Tween`](tween.md) | Handle for pausing / reversing / cancelling an animation. |
| [`TweenManager`](tween-manager.md) | Host for `Tween` jobs and `Easing` helpers. |
| [`ThemeManager`](theme-manager.md) | OKLCH palette generator for every built-in control. |
| [`FontManager`](font-manager.md) | Font loading + caching + default font. |
| [`ResourceManager`](resource-manager.md) | Disk / embedded resource loader. |
| [`SkiaSharp Extensions`](sk-extensions.md) | `SKColor`, `SKPoint`, `SKRect`, `SKSize` helpers. |
| [`Enums`](enums.md) | Shared enums (`EaseType`, `MouseButton`, `CursorType`, …). |
| [`ProfilerControl`](profiler-control.md) | Built-in debug overlay (`Ctrl+Shift+G`). |

## Controls

All live in `HandyUI.Controls.Controls` and derive from
[`BaseControl<TSelf>`](base-control.md).

| Control | Purpose |
| --- | --- |
| [`Button`](controls/button.md) | Filled text button with hover and ripple. |
| [`CheckBox`](controls/check-box.md) | Box + label with animated checkmark. |
| [`Frame`](controls/frame.md) | Rounded rectangle with border, elevation, shadow. |
| [`ItemBox`](controls/item-box.md) | Selectable list item with sibling exclusivity. |
| [`Label`](controls/label.md) | Static multi-line text. |
| [`ListBox`](controls/list-box.md) | Auto-laid-out scrolling stack. |
| [`MultilineTextInput`](controls/multiline-text-input.md) | Multi-line text editor. |
| [`Picture`](controls/picture.md) | Image with four stretch modes. |
| [`PictureButton`](controls/picture-button.md) | Clickable image with ripple. |
| [`ProgressBar`](controls/progress-bar.md) | Determinate / indeterminate progress. |
| [`ScrollFrame`](controls/scroll-frame.md) | Scroll viewport with fade-out scrollbar. |
| [`Slider`](controls/slider.md) | Draggable value slider. |
| [`TextInput`](controls/text-input.md) | Single-line text editor. |
| [`Toggle`](controls/toggle.md) | On / off switch. |

## Silk.NET Adapter

| Type | Summary |
| --- | --- |
| [`HandyWindow`](handy-window.md) | GLFW-backed `IHostWindow` implementation. |
| [`WindowConfiguration`](window-configuration.md) | Options record for `HandyWindow`. |
| [`TrayIcon`](tray-icon.md) | Windows notification-area icon. |
| [`TrayMenuItem`](tray-menu-item.md) | One entry in a tray context menu. |
| [`ThemeHelper`](theme-helper.md) | OS accent + dark/light detection. |
| [`RendererHelper`](silknet-helpers.md#rendererhelper) | Attaches a `UIRenderer` to a `HandyWindow`. |
| [`WindowHelper`](silknet-helpers.md#windowhelper) | Window/renderer factories. |
| [`Win32Helper`](silknet-helpers.md#win32helper) | Native Win32 access (styles, tray, DPI, drag, icons). |
| [`VectorExtensions`](vector-extensions.md) | Conversions between SkiaSharp and Silk.NET types. |