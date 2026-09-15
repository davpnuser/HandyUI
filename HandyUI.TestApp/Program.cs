// the bounds testing code

using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.Core.Interfaces;
using HandyUI.WinForms.Classes.Helper;
using HandyUI.WinForms.Components;
using SkiaSharp;
using System.Runtime.CompilerServices;

static async void HotReloadControl(IUIControl control, UIRenderer renderer, int wait = 1000)
{
    var objectType = control.GetType();
    var state = control.Serialize();

    if (control.Parent == null)
    {
        renderer.RemoveRootControl(control);
    }
    else if (control.Parent != null)
    {
        control.Parent = null;
    }

    control.Dispose();

    await Task.Delay(wait);

    var newObject = (IUIControl)RuntimeHelpers.GetUninitializedObject(objectType);
    renderer.AddRootControl(newObject);
    newObject.Deserialize(state);
}

#region Setting up the window

ResourceManagerHelper.InitializeResourceManager();

using var MainForm = new HandyForm();
MainForm.AutoScaleMode = AutoScaleMode.None;
MainForm.MaximizeBox = false;
MainForm.Text = "HandyUI Control Showcase";
MainForm.ClientSize = new Size(820, 695);
MainForm.MinimumClientSize = new Size(820, 695);
MainForm.FormBorderStyle = FormBorderStyle.FixedSingle;
MainForm.StartPosition = FormStartPosition.CenterScreen;

if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallIcon.ico", out var iconStream, false))
{
    MainForm.Icon = new Icon(iconStream!);
}

#endregion

#region Setting up the renderer

var skiaPanel = new HandyUIGLControl
{
    Dock = DockStyle.Fill,
    VSync = true
};
var renderer = RendererHelper.Attach(skiaPanel);

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

var bg2 = new Frame(300, 300)
{
    Parent = backgroundFrame,
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

var btn = new TextButton("controls")
{
    ZIndex = 1,
    Location = new(25, 25),
    OnClick = async () =>
    {
        HotReloadControl(backgroundFrame, renderer);
        HotReloadControl(bg2, renderer);
        HotReloadControl(childButton, renderer);
    }
};
renderer.AddRootControl(btn);

#endregion

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);