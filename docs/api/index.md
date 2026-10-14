# API Reference

This section documents the public API surface of HandyUI. Core primitives
live in `HandyUI.Core`; built-in controls ship in a separate
`HandyUI.Controls` package.

## Core

| Type | Summary |
| --- | --- |
| [`UIRenderer`](ui-renderer.md) | Render loop, input dispatcher, control/module registry. |
| [`ResourceManager`](resource-manager.md) | Loads resources from disk or embedded assembly resources. |
| [`BaseControl`](base-control.md) | Base class for all drawable controls. |
| [`BaseModule`](base-module.md) | Base class for logic-only modules. |

## Interfaces

| Type | Summary |
| --- | --- |
| [`IControl`](icontrol.md) | Contract implemented by every control. |
| [`IModule`](imodule.md) | Contract implemented by every module. |

## Records & Contexts

| Type | Summary |
| --- | --- |
| [`MouseEventContext`](contexts.md#mouseeventcontext) | Describes a mouse input event. |
| [`KeyEventContext`](contexts.md#keyeventcontext) | Describes a keyboard input event. |
| [`Theme`](theme.md) | Palette of colors used by every control. |

## Helpers

| Type | Summary |
| --- | --- |
| [`SKColorHelper`](sk-color-helper.md) | Hue/brightness/transparency helpers for `SKColor`. |
| [`SKRectHelper`](sk-rect-helper.md) | Conversions between `SKPoint`, `SKSize`, and `SKRect`. |

## Enums

| Type | Summary |
| --- | --- |
| [`BorderDirection`](border-direction.md) | Where a stroke is drawn relative to its shape. |

## Controls

`HandyUI.Controls` - built-in controls that ship on top of `HandyUI.Core`.

| Type | Summary |
| --- | --- |
| [`CheckBox`](controls/check-box.md) | Two-state toggle with a box and optional fill. |
| [`Frame`](controls/frame.md) | Rectangle with a background fill and border. |
| [`ImageButton`](controls/image-button.md) | Clickable button with an image and/or text. |
| [`ImageLabel`](controls/image-label.md) | Static image with optional auto-sizing. |
| [`TextBox`](controls/text-box.md) | Single-line editable text field with caret. |
| [`TextButton`](controls/text-button.md) | Clickable button that renders a text label. |
| [`TextLabel`](controls/text-label.md) | Static, non-interactive text label. |

## Silk.NET Adapter

| Type | Summary |
| --- | --- |
| [`HandyWindow`](handy-window.md) | An `IWindow` wrapper with a pump thread and window state. |
| [`WindowConfiguration`](window-configuration.md) | Options record used to create a `HandyWindow`. |
| [`RendererHelper`](silknet-helpers.md#rendererhelper) | Attaches a `UIRenderer` to a `HandyWindow`. |
| [`WindowHelper`](silknet-helpers.md#windowhelper) | Window/renderer factories and platform tweaks. |
| [`Win32Helper`](silknet-helpers.md#win32helper) | Native Win32 tweaks (clip children, taskbar, …). |
| [`VectorExtensions`](vector-extensions.md) | Conversions between `SKPoint`/`SKSize` and `Vector2D<int>`. |