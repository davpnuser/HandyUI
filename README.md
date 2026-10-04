# HandyUI

HandyUI is a platform-agnostic at its core code-only C# UI framework powered by SkiaSharp.
To test this UI framework for yourself, You can check out the "TestApp" project in the HandyUI solution.

## HandyUI Compatibility

| OS | Support state for HandyUI.Core | Support state for HandyUI.SilkNet | Support state for HandyUI.WinForms (deprecated) |
|---|---|---|---|
| Windows | ✔ Main target / Fully works. | ✔ Main Target / All features work. | ✔ Main Target / All features work. |
| Linux | ✔ Secondary target / Fully works.  | ⚠ Secondary Target / Needs adapter fixes.  | ❌ Incompatible. |
| Other(s) | ℹ️ Third target / Not tested. | ❌ Third target / Not implemented at all. | ❌ Incompatible. |

## Examples using the HandyUI adapter for Silk.NET library.

#### Example app only spawning a single window: _(uses HandyUI.SilkNet)_

```csharp
using HandyUI.Core.Classes.Controls;
using HandyUI.SilkNet.Classes.Helper;
using SkiaSharp;

var (window, renderer) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window", new SKSize(800, 600));

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

await window.RunAsync();
```
#### Example app spawning multiple windows: _(uses HandyUI.SilkNet)_

```csharp
using HandyUI.Core.Classes.Controls;
using HandyUI.SilkNet.Classes.Helper;
using SkiaSharp;

var (window1, renderer1) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window 1", new SKSize(800, 600));
var (window2, renderer2) = SilkWindowHelper.CreateWindowAndGetRenderer("Example Window 2", new SKSize(800, 600));

new TextLabel()
    .WithText("Hello, World!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer1);

new TextLabel()
    .WithText("Hello World, Once Again!")
    .WithTextSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer2);

await Task.WhenAll(window1.RunAsync(), window2.RunAsync());
```

#### Example app only spawning a single window: _(uses HandyUI.WinForms)_

```csaharp
using HandyUI.Core.Classes.Controls;
using HandyUI.WinForms.Classes.Helper;
using HandyUI.WinForms.Components;
using SkiaSharp;

using var MainForm = new HandyForm();
MainForm.Text = "Example window";
MainForm.ClientSize = new Size(820, 695);

var skiaPanel = new HandyUIGLControl
{
    Dock = DockStyle.Fill,
    VSync = true
};

var renderer = RendererHelper.Attach(skiaPanel);

var label = new TextLabel("Hello, World!")
{
    TextSize = 24,
    Location = new SKPoint(15, 15),
    TextColor = SKColors.Black
};

renderer.AddRootControl(label);

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);
```
