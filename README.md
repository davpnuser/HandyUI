# HandyUI

HandyUI is a Platform-agnostic Code-Only C# UI framework powered by SkiaSharp.

To test this UI framework for yourself, You can check out the "TestApp" project in the HandyUI solution.

#### Example cross-platform app:

```csharp
using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

var options = WindowOptions.Default;
options.Size = new Vector2D<int>(820, 695);
options.Title = "HandyUI Example Menu";
options.WindowBorder = WindowBorder.Fixed;
options.VSync = true;
options.FramesPerSecond = 0;

using var window = Window.Create(options);

var renderer = SilkRendererHelper.Attach(window);

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
{
    logoImage = SKImage.FromEncodedData(imageStream!);
}

var button = new TextButton("click me!")
{
    Location = new(15, 15)
};
renderer.AddRootControl(button);

window.Run();
```
