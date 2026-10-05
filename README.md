![Animated Header](https://readme-svg-wave-divider-generator.vercel.app/wave?type=smooth&width=854&height=70&amplitude=30&frequency=0&layers=3&color_top=f4e9d3&color_bottom=b5ac9a&opacity=1&flip=true&gradient=true&mirror=true&animate=true&speed=12)

# HandyUI

## About HandyUI
HandyUI is a platform-agnostic at its core code-only C# UI framework powered by SkiaSharp.
To test this UI framework for yourself, You can check out the "TestApp" project in the HandyUI solution.

<!--- Repository badges --->
***Repository badges***

![Last Commit](https://img.shields.io/github/last-commit/davpnuser/HandyUI?style=flat-square&color=F4E9D3&label=Last%20Commit&cacheSeconds=1800)
![License](https://img.shields.io/github/license/davpnuser/HandyUI?style=flat-square&color=F4E9D3&label=License&cacheSeconds=3600)
![Repository Size](https://img.shields.io/github/repo-size/davpnuser/HandyUI?style=flat-square&color=F4E9D3&label=Repository%20Size&cacheSeconds=3600)

<!--- Badges for HandyUI.Core --->
***Badges for HandyUI.Core***

[![HandyUI.Core Version](https://img.shields.io/nuget/v/HandyUI.Core?style=flat-square&color=F4E9D3&label=HandyUI.Core%20Version)](https://www.nuget.org/packages/HandyUI.Core)
[![HandyUI.Core Downloads](https://img.shields.io/nuget/dt/HandyUI.Core?style=flat-square&color=F4E9D3&label=HandyUI.Core%20Downloads)](https://www.nuget.org/packages/HandyUI.Core)

<!--- Badges for HandyUI.SilkNet --->
***Badges for HandyUI.SilkNet***

[![HandyUI.SilkNet Version](https://img.shields.io/nuget/v/HandyUI.SilkNet?style=flat-square&color=F4E9D3&label=HandyUI.SilkNet%20Version)](https://www.nuget.org/packages/HandyUI.SilkNet)
[![HandyUI.SilkNet Downloads](https://img.shields.io/nuget/dt/HandyUI.SilkNet?style=flat-square&color=F4E9D3&label=HandyUI.SilkNet%20Downloads)](https://www.nuget.org/packages/HandyUI.SilkNet)

<!--- Badges for HandyUI.WinForms --->
***HandyUI.WinForms***

[![HandyUI.WinForms Version](https://img.shields.io/nuget/v/HandyUI.WinForms?style=flat-square&color=F4E9D3&label=HandyUI.WinForms%20Version)](https://www.nuget.org/packages/HandyUI.WinForms)
[![HandyUI.WinForms Downloads](https://img.shields.io/nuget/dt/HandyUI.WinForms?style=flat-square&color=F4E9D3&label=HandyUI.WinForms%20Downloads)](https://www.nuget.org/packages/HandyUI.WinForms)

<!--- Compatibility Table --->
## HandyUI Compatibility

| OS | Support state for HandyUI.Core | Support state for HandyUI.SilkNet | Support state for HandyUI.WinForms (deprecated) |
|---|---|---|---|
| Windows | ✔ Main target / Fully works. | ✔ Main Target / All features work. | ✔ Main Target / All features work. |
| Linux | ✔ Secondary target / Fully works.  | ⚠ Secondary Target / Needs adapter fixes.  | ❌ Incompatible. |
| Other(s) | ℹ️ Third target / Not tested. | ❌ Third target / Not implemented at all. | ❌ Incompatible. |

## C# Code examples using the HandyUI library.

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

```csharp
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
