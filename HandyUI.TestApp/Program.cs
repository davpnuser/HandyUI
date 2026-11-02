using HandyUI.Controls.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Components;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using Silk.NET.Windowing;
using SkiaSharp;

#region Setup

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var logoStream))
    logoImage = SKImage.FromEncodedData(logoStream);

var config = WindowConfiguration.Default with
{
    Title = "Test Showcase",
    Icon = logoImage,
    IsTitleBarVisible = false,
    MinSize = new(500, 500)
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);

var cb = new Button()
    .WithZIndex(2)
    .WithText("Close")
    .WithWhenClicked(window.Close)
    .WithAddToRenderer(renderer);

var mb = new Button()
    .WithZIndex(2)
    .WithText("Minimize")
    .WithLocation(cb.Bounds.Width, 0)
    .WithWhenClicked(() => { window.State = WindowState.Minimized; })
    .WithAddToRenderer(renderer);

var mxb = new Button()
    .WithZIndex(2)
    .WithText("Maximize")
    .WithLocation(mb.Bounds.Width + cb.Bounds.Width, 0)
    .WithAddToRenderer(renderer);

var titleBar = new Frame()
    .WithZIndex(1)
    .WithHasShadow(false)
    .WithHasBorder(false)
    .WithSize(3500, cb.Bounds.Height)
    .WithLocation(0, 0)
    //.WithWhenMouseDown(window.BeginDrag)
    //.WithWhenMouseUp(() => { window.IsDragging = false; })
    .WithAddToRenderer(renderer);

var winSize = window.Size ?? new SKSize(800, 600);
var left = mxb.Location.X + mxb.Bounds.Width;
window.DragRegions.Add(new SKRect(left, 0, winSize.Width, mxb.Height));

var state = false;

mxb.WhenClicked += () =>
{
    if (state == false)
    {
        mxb.Text = "Restore";
        window.State = WindowState.Maximized;
        state = true;
    }
    else if (state == true)
    {
        mxb.Text = "Maximize";
        window.State = WindowState.Normal;
        state = false;
    }

    titleBar.IsEnabled = !state;
};

var container = new Frame()
    .WithSize(3500, 3500)
    .WithLocation(0, cb.Bounds.Height)
    .WithHasBackground(false)
    .WithHasShadow(false)
    .WithHasBorder(false)
    .WithAddToRenderer(renderer);

#endregion

new Label()
    .WithText("Hello, World!")
    .WithFontSize(24)
    .WithLocation(15, 15)
    .WithParent(container);

new Label()
    .WithText("Right-click the tray icon to hide/show this window.")
    .WithFontSize(14)
    .WithLocation(15, 55)
    .WithParent(container);

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