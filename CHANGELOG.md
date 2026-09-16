Release Log & Changelog

Version Overview:

* HandyUI.Core: v1.1.3.26259
* HandyUI.WinForms: v1.1.1.26259
* HandyUI.SilkNet: v1.1.0.26259 (New Package)

Key Highlights:

* New Silk.NET Backend Support: Introduced the HandyUI.SilkNet package, bringing cross-platform windowing, OpenGL rendering context creation, and comprehensive input mapping via Silk.NET.
* UI Hierarchy Management Overhaul: Enhanced parent-child tracking with synchronized hooks, automatic child lists, and robust ordering/sorting helpers.
* Advanced Rendering & Opacity: Added support for hardware scissoring/clipping rectangles and layered transparency via SaveLayer and clamped AlphaPaint scaling.

Detailed Changes:

Control Interface & Base Controls:

* Exposed a public Children list on control interfaces and changed the Parent property type to UIControlBase?.
* Updated the Parent property setter to automatically invoke underlying base addition and removal routines.
* Added internal child-tracking collections, sorting flags, and synchronized hooks for AddChildInternal and RemoveChildInternal that fire OnChildAdded and OnChildRemoved.
* Added child sorting helpers like EnsureChildrenSorted and InvalidateChildrenOrder.
* Renamed RetainedModePositioning to InheritedPositioningEnabled.
* Updated property flags to include InheritedPositioningEnabled, ScissoringEnabled, AlphaPaint, and Opacity.
* Introduced an AlphaPaint object and a clamped Opacity property that automatically scales paint alpha transparency.

Renderer & Graphics Engine:

* Refactored root control tracking from _controls and AddControl to _rootControls and AddRootControl.
* Built out recursive traversal methods for hit-testing, mouse dispatching, rendering, and clean disposal down the UI tree.
* Added rendering support for layered opacity via SaveLayer and hardware scissoring/clipping rectangles.

Platform & Packaging Updates:

* Added the new HandyUI.SilkNet project integrating Silk.NET.Input, Silk.NET.OpenGL, and Silk.NET.Windowing.
* Migrated HandyUI.TestApp from Windows Forms to a platform-agnostic Silk.NET window loop.
* Standardized NuGet readme file paths (NuGet-README.md), asset packaging, and third-party notice inclusions across packages.
