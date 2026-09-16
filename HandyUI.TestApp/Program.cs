using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Classes.Helper;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

#region Setting up the window

var options = WindowOptions.Default;
options.Size = new Vector2D<int>(820, 695);
options.Title = "HandyUI Control Showcase";
options.WindowBorder = WindowBorder.Fixed;
options.VSync = true;
options.FramesPerSecond = 0;

using var window = Window.Create(options);

#endregion

#region Setting up the renderer

var renderer = SilkRendererHelper.Attach(window);

#endregion

#region Setting up the assets

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo.png", out var imageStream))
{
    logoImage = SKImage.FromEncodedData(imageStream!);
}

#endregion

#region Setting up the background

var backgroundFrame = new Frame(width: 820, height: 695)
{
    NormalColor = SKColor.Parse("#111117"),
    BorderColor = new(0, 0, 0, 0),
    CornerRadius = 0,
    ZIndex = -1
};
renderer.AddRootControl(backgroundFrame);

#endregion

#region Setting up the controls showcase

var bg = new Frame(200, 500)
{
    Location = new(25, 25),
    BorderColor = new(0, 0, 0, 0),
    NormalColor = new(0, 255, 0),
    CornerRadius = 0f,
};
renderer.AddRootControl(bg);

var bg2 = new Frame(300, 300)
{
    Parent = bg,
    Location = new(50, 25),
    BorderColor = new(0, 0, 0, 0),
    CornerRadius = 0f,
    NormalColor = new(255, 0, 0)
};

var childButton = new TextButton("click me!")
{
    Parent = bg2,
    Location = new(90, -18)
};

#endregion

window.Run();