### **HandyUI.Core**

* **UIControlBase.cs**:
* Reorganized class layout using #region blocks (Properties, Internal Fields, Internal Functions, Event Methods, Abstract/Virtual Methods, Fluent APIs, Disposal).
* Added WithBounds(SKRect) and WithBounds(float, float, float, float) fluent methods.
* Added WithAddToRenderer(UIRenderer) fluent method.
* Moved WithLocation methods to the Fluent APIs section.
* Changed visibility of Dispose(bool) from protected virtual to protected.
* **TestControl.cs**: Removed from HandyUI.Core/Classes/Controls/.
* **TextButton.cs**:
* Added support for LightTextColor theme color property and backing field _customLightTextSet.
* Updated background/border/text color mapping in UpdateBrushes() for active/hovered/normal states.
* Added WithLightTextColor(SKColor) fluent API method.
* **SKColorHelper.cs**: Added new helper class providing FromHue, WithBrightness, WithBrightnessHsl, and WithTransparency methods.
* **SKRectHelper.cs**: Added new helper class with conversion extensions between SKRect, SKPoint, SKSize.
* **ContextClasses.cs**: Wrapped mouse and keyboard context records and enums into #region blocks.
* **ThemeRecord.cs**: Added LightTextColor property to ThemeRecord.
* **DefaultTheme.cs**: Updated default light/dark color definitions in GetTheme().
* **UIRenderer.cs**:
* Added primary constructor parameter bool useDirtyRendering = true.
* Updated default BackgroundColor to DefaultTheme.GetTheme().DarkerBackgroundColor.
* Reorganized code into #region blocks.
* Changed default canvas clear color in RenderControls from white to BackgroundColor.
* **IUIControl.cs**:
* Added #region block organization.
* Added WithBounds and WithAddToRenderer method signatures to interface.
* **HandyUI.Core.csproj**: Updated assembly version to 1.3.36.26277 and removed file excludes for helpers and TestControl.cs.

### **HandyUI.SilkNet**

* **SilkWindowHelper.cs**:
* Removed old implementation file under Classes/Extensions/.
* Re-created SilkWindowHelper.cs under Classes/Helper/ with CreateWindowAndGetRenderer returning (HandyWindow, UIRenderer).
* **SilkRendererHelper.cs**:
* Refactored Attach() to accept HandyWindow instead of IWindow.
* Delegated lifecycle and rendering setup to SilkWindowRendererAdapter.
* **HandyWindow.cs**: Added new wrapper component around Silk.NET IWindow with multi-threading pump loop, modal state support, parent-child window handling, size limits, and asynchronous run features.
* **SilkWindowRendererAdapter.cs**: Added internal adapter class handling OpenGL context creation, SkiaSharp surfaces, rendering loops, and input event bindings.
* **VectorExtensions.cs**: Added vector extension methods converting between SkiaSharp objects (SKPoint, SKSize) and Silk.NET Vector2D<int>.
* **GlfwTool.cs**: Added internal helper class ensuring thread-safe GLFW initialization.
* **HandyWindowHelper.cs**: Added Win32 native helper to apply WS_CLIPCHILDREN style to parent windows.
* **PlatformTools.cs**: Added platform restriction check restricting Silk.NET adapter usage to Windows.

### **HandyUI.TestApp**

* **Program.cs**: Replaced multi-window test loop with standard single-window example rendering a TextLabel ("Hello, World!").
* **FpsLabelControl.cs**: Refactored text height calculations and bounds measurement in UpdateBounds().
* **HueShiftingControl.cs**: Added new test control demonstrating dynamic hue shifts using SKColorHelper.
* **TestControl.cs**: Added local TestControl implementation inside HandyUI.TestApp project.
