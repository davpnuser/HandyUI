# API Reference

This section documents the public API surface of HandyUI.

## Core

| Type | Summary |
| --- | --- |
| [`UIRenderer`](ui-renderer.md) | Render loop, input dispatcher, control/module registry. |
| [`ResourceManager`](resource-manager.md) | Loads resources from disk or embedded assembly resources. |
| [`UIControlBase`](ui-control-base.md) | Base class for all drawable controls. |
| [`BaseModule`](base-module.md) | Base class for logic‑only modules. |

## Interfaces

| Type | Summary |
| --- | --- |
| [`IUIControl`](iui-control.md) | Contract implemented by every control. |
| [`IModule`](imodule.md) | Contract implemented by every module. |

## Records & Contexts

| Type | Summary |
| --- | --- |
| [`MouseEventContext`](contexts.md#mouseeventcontext) | Describes a mouse input event. |
| [`KeyEventContext`](contexts.md#keyeventcontext) | Describes a keyboard input event. |
| [`ThemeRecord`](theme-record.md) | Immutable color palette. |

## Helpers

| Type | Summary |
| --- | --- |
| [`SKColorHelper`](sk-color-helper.md) | Hue/brightness/transparency helpers for `SKColor`. |
| [`SKRectHelper`](sk-rect-helper.md) | Conversions between `SKPoint`, `SKSize`, and `SKRect`. |

## Themes & Enums

| Type | Summary |
| --- | --- |
| [`DefaultTheme`](default-theme.md) | The built‑in light theme. |
| [`BorderDirection`](border-direction.md) | Where a stroke is drawn relative to its shape. |

## Silk.NET Adapter

| Type | Summary |
| --- | --- |
| [`HandyWindow`](handy-window.md) | An `IWindow` wrapper with a pump thread and window state. |
| [`WindowConfiguration`](window-configuration.md) | Options record used to create a `HandyWindow`. |
| [`RendererHelper`](silknet-helpers.md#rendererhelper) | Attaches a `UIRenderer` to a `HandyWindow`. |
| [`WindowHelper`](silknet-helpers.md#windowhelper) | Window/renderer factories and platform tweaks. |
| [`Win32Helper`](silknet-helpers.md#win32helper) | Native Win32 tweaks (clip children, taskbar, …). |
| [`VectorExtensions`](vector-extensions.md) | Conversions between `SKPoint`/`SKSize` and `Vector2D<int>`. |