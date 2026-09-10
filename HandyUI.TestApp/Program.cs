using HandyUI.Core.Classes.Controls;
using HandyUI.WinForms.Classes.Helper;
using HandyUI.WinForms.Components;

using var MainForm = new Form();
MainForm.Text = "Example HandyUI App";
MainForm.MinimumSize = new Size(427, 294);
MainForm.Size = new Size(427, 294);
MainForm.StartPosition = FormStartPosition.CenterScreen;

if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallIcon-Tt.ico", out var resourceStream, false))
    MainForm.Icon = new Icon(resourceStream!);

var skiaPanel = new HandyUIGLControl
{
    Dock = DockStyle.Fill,
    VSync = true
};

var renderer = RendererHelper.Attach(skiaPanel);

var label1 = new TextLabel()
{
    Text = "Hi! This is a example HandyUI app.",
    TextSize = 24f,
    Location = new(25, 25),
    TextColor = new(0, 0, 0)
};

var label2 = new TextLabel()
{
    Text = "+0",
    TextSize = 24f,
    Location = new(25, 60),
    TextColor = new(0, 0, 0, 155)
};

var i = 0;
void Add()
{
    i++;
    Change();
}
void Remove()
{
    i--;
    Change();
}

void AddMore()
{
    i += 10;
    Change();
}
void RemoveMore()
{
    i -= 10;
    Change();
}

void Change()
{
    var sign = i < 0 ? '-' : '+';

    label2.Text = $"{sign}{Math.Abs(i)}";
}

var button1 = new TextButton("+1")
{
    TextSize = 16f,
    Location = new(25, 104),
    OnClick = Add
};

var button2 = new TextButton("-1")
{
    TextSize = 16f,
    Location = new(155, 104),
    OnClick = Remove
};

var button3 = new TextButton("+10")
{
    TextSize = 16f,
    Location = new(25, 150),
    OnClick = AddMore
};

var button4 = new TextButton("-10")
{
    TextSize = 16f,
    Location = new(155, 150),
    OnClick = RemoveMore
};

var label3 = new TextLabel()
{
    Text = "Im toggled off!",
    TextSize = 24f,
    Location = new(90, 206),
    TextColor = new(0, 0, 0)
};

void toggle(bool state)
{
    var stateText = state == true ? "on" : "off";
    label3.Text = $"Im toggled {stateText}!";
}

var toggleSwitch = new ToggleSwitch()
{
    Location = new(25, 211),
    OnToggled = toggle
};

renderer.AddControl(label1);
renderer.AddControl(label2);
renderer.AddControl(button1);
renderer.AddControl(button2);
renderer.AddControl(button3);
renderer.AddControl(button4);
renderer.AddControl(label3);
renderer.AddControl(toggleSwitch);

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);