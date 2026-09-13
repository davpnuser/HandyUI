Change notes for:
 - HandyUI.Core (v1.1.0.26256)
 - HandyUI.WinForms (v1.1.0.26256)

Control Interface Changes:
 - Added Children list exposure and changed Parent to type UIControlBase?.
 - Updated property flags to include InheritedPositioningEnabled, ScissoringEnabled, AlphaPaint, and Opacity.

Base Control Changes:
 - Added internal child tracking collections, sorting flags, and synchronized hooks for AddChildInternal and RemoveChildInternal that fire OnChildAdded and OnChildRemoved.
 - Updated the Parent property setter to automatically invoke child addition and removal methods on underlying bases.
 - Added child sorting helpers like EnsureChildrenSorted and InvalidateChildrenOrder.
 - Renamed RetainedModePositioning to InheritedPositioningEnabled.
 - Introduced an AlphaPaint object and a clamped Opacity property that automatically scales paint alpha transparency.

Renderer Changes:
 - Refactored root control tracking from _controls and AddControl to _rootControls and AddRootControl.
 - Built out recursive traversal methods for hit-testing, mouse dispatching, rendering, and clean disposal down the UI tree.
 - Added rendering support for layered opacity via SaveLayer and hardware scissoring/clipping rectangles.
