using HandyUI.Controls.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using SkiaSharp;

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var logoStream))
    logoImage = SKImage.FromEncodedData(logoStream);

var config = WindowConfiguration.Default with
{
    Title = "Test Showcase",
    Size = new(1050, 1000),
    Icon = logoImage,
    //WindowBorder = WindowBorder.Resizable,
    //WindowBorder = WindowBorder.Hidden,
    //IsTitleBarVisible = false,
    //IsResizable = false
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);

new Frame()
    .WithSize(150, 150)
    .WithLocation(50, 50)
    .WithWhenMouseDown(() => { window.IsDragging = true; })
    .WithWhenMouseUp(() => { window.IsDragging = false; })
    .WithAddToRenderer(renderer);

new Label()
    .WithText("Hello, World!")
    .WithFontSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

new Label()
    .WithText("Right-click the tray icon to hide/show this window.")
    .WithFontSize(14)
    .WithLocation(15, 55)
    .WithAddToRenderer(renderer);

var nb = new Button()
    .WithText("test test test")
    .WithAddToRenderer(renderer);

//nb.WhenClicked += () =>
//{
//    window.IsDragging = true;
//};

window.TrayIcon.Icon = logoImage;
window.TrayIcon.ToolTip = "Test Showcase";
window.TrayIcon.Visible = true;

window.TrayIcon.MenuItems.Add(new TrayMenuItem("Show", () => window.Show()) { IsDefault = true });
window.TrayIcon.MenuItems.Add(new TrayMenuItem("Hide", () => window.Hide()));
window.TrayIcon.MenuItems.Add(TrayMenuItem.Separator());
window.TrayIcon.MenuItems.Add(new TrayMenuItem("Exit", () => window.Close()));

window.TrayIcon.RightClick += () =>
{
    if (!window.TrayIcon.ShowContextMenuOnRightClick)
        Console.WriteLine("User right-clicked the tray icon");
};

await window.RunAsync();