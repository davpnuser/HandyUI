using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.WinForms.Classes.Helper;
using HandyUI.WinForms.Components;
using SkiaSharp;
using TextBox = HandyUI.Core.Classes.Controls.TextBox;

#region Setting up the window

ResourceManagerHelper.InitializeResourceManager();

using var MainForm = new HandyForm();
MainForm.AutoScaleMode = AutoScaleMode.None;
MainForm.MaximizeBox = false;
MainForm.Text = "handyui showcase app";
MainForm.ClientSize = new Size(780, 580);
MainForm.MinimumClientSize = new Size(780, 580);
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

#region Setting up the controls

var tb = new TextBox(placeholder: "hekko")
{
    Location = new SKPoint(20, 20),
};
renderer.AddControl(tb);

var interactiveSlider = new Slider(305f, 0f, 100f, 50f)
{
    Location = new SKPoint(20, 120),
    ZIndex = 1,
};
renderer.AddControl(interactiveSlider);

#endregion

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);