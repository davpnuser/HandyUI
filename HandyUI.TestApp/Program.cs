using HandyUI.Core.Classes.Controls;
using HandyUI.WinForms.Classes.Helper;
using HandyUI.WinForms.Components;
using SkiaSharp;

#region Setting up the window

using var MainForm = new HandyForm();
MainForm.AutoScaleMode = AutoScaleMode.None;
MainForm.MaximizeBox = false;
MainForm.Text = "HandyUI Testing App";
MainForm.ClientSize = new Size(306, 306);
MainForm.MinimumClientSize = new Size(306, 306);
MainForm.StartPosition = FormStartPosition.CenterScreen;

if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallIcon.ico", out var resourceStream, false))
    MainForm.Icon = new Icon(resourceStream!);

#endregion

#region Setting up the rendering surface
var skiaPanel = new HandyUIGLControl
{
    Dock = DockStyle.Fill,
    VSync = true
};

var renderer = RendererHelper.Attach(skiaPanel);
#endregion

#region Setting up the controls

var background = new Frame()
{
    CornerRadius = 0,
    NormalColor = new(0, 0, 0),
    BorderColor = new(0, 0, 0, 0),
    Width = MainForm.Width,
    Height = MainForm.Height
};
renderer.AddControl(background);

static void toggled(bool state)
{
    MessageBox.Show($"Toggled to: {state}");
}

static void clicked()
{
    MessageBox.Show($"Toggled to: clicked");
}

if (ResourceManager.GetResourceByPath("Resources/HandyUI-Logo-T.png", out resourceStream))
{
    var image = SKImage.FromEncodedData(resourceStream!);
    var imageControl = new ImageButton(image)
    {
        ZIndex = 1,
        Location = new(25, 25),
        Height = 256,
        Width = 256,
        ImageHeight = 256,
        ImageWidth = 256,
        OnClicked = clicked
    };
    renderer.AddControl(imageControl);
}
else
{
    var textControl = new ToggleButton("Could not load image.")
    {
        Location = new SKPoint(25, 25),
        TextSize = 18,
        OnToggled = toggled
    };
    renderer.AddControl(textControl);
}

#endregion

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);