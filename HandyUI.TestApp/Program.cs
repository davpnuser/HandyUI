using HandyUI.Core.Classes.Controls;
using HandyUI.Core.Components;
using HandyUI.WinForms.Classes.Helper;
using HandyUI.WinForms.Components;
using SkiaSharp;
using CheckBox = HandyUI.Core.Classes.Controls.CheckBox;
using ProgressBar = HandyUI.Core.Classes.Controls.ProgressBar;
using TextBox = HandyUI.Core.Classes.Controls.TextBox;

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

var backgroundFrame = new Frame
{
    Width = 820,
    Height = 695,
    NormalColor = SKColor.Parse("#d7d7e0"),
    BorderColor = new(0, 0, 0, 0),
    ZIndex = -1
};
renderer.AddRootControl(backgroundFrame);

#endregion

#region Setting up the controls showcase

var titleLabel = new TextLabel
{
    Text = "HandyUI Control Showcase",
    Location = new SKPoint(20, 20),
    TextSize = 22f,
    ZIndex = 1
};
renderer.AddRootControl(titleLabel);

var statusFrame = new Frame
{
    Width = 360f,
    Height = 36f,
    Location = new SKPoint(440, 15),
    ZIndex = 0
};
renderer.AddRootControl(statusFrame);

var statusLabel = new TextLabel
{
    Parent = statusFrame,
    Text = "Ready",
    Location = new SKPoint(12, 10),
    TextSize = 13f,
    ZIndex = 1
};

var card1 = new Frame
{
    Width = 380f,
    Height = 300f,
    Location = new SKPoint(20, 65),
    ZIndex = 0
};
renderer.AddRootControl(card1);

var card1Title = new TextLabel
{
    Parent = card1,
    Text = "Buttons, Switches & CheckBoxes",
    Location = new SKPoint(15, 15),
    TextSize = 15f,
    ZIndex = 1
};

var textBtn1 = new TextButton
{
    Parent = card1,
    Text = "Standard Button",
    Width = 165f,
    Height = 36f,
    Location = new SKPoint(15, 45),
    ZIndex = 1,
    OnClick = () => statusLabel.Text = "TextButton 1 clicked"
};

var textBtn2 = new TextButton
{
    Parent = card1,
    Text = "Custom Color",
    Width = 165f,
    Height = 36f,
    Location = new SKPoint(195, 45),
    NormalColor = SKColor.Parse("#313244"),
    HoverColor = SKColor.Parse("#45475A"),
    PressedColor = SKColor.Parse("#585B70"),
    BorderColor = SKColor.Parse("#F38BA8"),
    TextColor = SKColor.Parse("#F38BA8"),
    ZIndex = 1,
    OnClick = () => statusLabel.Text = "Custom TextButton clicked"
};

var toggleBtn1 = new ToggleButton
{
    Parent = card1,
    Text = "Toggle Off",
    IsChecked = false,
    Width = 165f,
    Height = 36f,
    Location = new SKPoint(15, 95),
    ZIndex = 1
};
toggleBtn1.OnToggled = (chk) =>
{
    toggleBtn1.Text = chk ? "Toggle On" : "Toggle Off";
    statusLabel.Text = $"ToggleButton = {chk}";
};

var toggleBtn2 = new ToggleButton
{
    Parent = card1,
    Text = "Custom Toggle",
    IsChecked = true,
    Width = 165f,
    Height = 36f,
    Location = new SKPoint(195, 95),
    OnColor = SKColor.Parse("#A6E3A1"),
    OffColor = SKColor.Parse("#313244"),
    TextOnColor = SKColor.Parse("#11111B"),
    BorderColor = SKColor.Parse("#A6E3A1"),
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"Custom Toggle = {chk}"
};

var switch1 = new ToggleSwitch
{
    Parent = card1,
    IsChecked = true,
    Width = 52f,
    Height = 26f,
    Location = new SKPoint(15, 150),
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"Switch 1 = {chk}"
};

var switch1Label = new TextLabel
{
    Parent = card1,
    Text = "Default Switch",
    Location = new SKPoint(75, 155),
    TextSize = 13f,
    ZIndex = 1
};

var switch2 = new ToggleSwitch
{
    Parent = card1,
    IsChecked = false,
    Width = 52f,
    Height = 26f,
    Location = new SKPoint(195, 150),
    TrackOnColor = SKColor.Parse("#FAB387"),
    TrackOffColor = SKColor.Parse("#1E1E2E"),
    BorderColor = SKColor.Parse("#FAB387"),
    KnobColor = SKColor.Parse("#BAC2DE"),
    KnobOnColor = SKColor.Parse("#11111B"),
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"Custom Switch = {chk}"
};

var switch2Label = new TextLabel
{
    Parent = card1,
    Text = "Custom Switch",
    Location = new SKPoint(255, 155),
    TextSize = 13f,
    ZIndex = 1
};

var chk1 = new CheckBox
{
    Parent = card1,
    Text = "Enable standard setting",
    IsChecked = true,
    Location = new SKPoint(15, 195),
    BoxSize = 20f,
    Spacing = 10f,
    ZIndex = 1,
    OnCheckChanged = (chk) => statusLabel.Text = $"CheckBox 1 = {chk}"
};

var chk2 = new CheckBox
{
    Parent = card1,
    Text = "Custom theme checkbox",
    IsChecked = false,
    Location = new SKPoint(15, 235),
    BoxSize = 22f,
    Spacing = 10f,
    BoxOnColor = SKColor.Parse("#F9E2AF"),
    BoxOffColor = SKColor.Parse("#181825"),
    CheckMarkColor = SKColor.Parse("#11111B"),
    BorderColor = SKColor.Parse("#F9E2AF"),
    TextColor = SKColor.Parse("#F9E2AF"),
    ZIndex = 1,
    OnCheckChanged = (chk) => statusLabel.Text = $"CheckBox 2 = {chk}"
};

var card2 = new Frame
{
    Width = 380f,
    Height = 279f,
    Location = new SKPoint(420, 65),
    ZIndex = 0
};
renderer.AddRootControl(card2);

var card2Title = new TextLabel
{
    Parent = card2,
    Text = "TextBox & Manual Image Controls",
    Location = new SKPoint(15, 15),
    TextSize = 15f,
    ZIndex = 1
};

var tb1 = new TextBox
{
    Parent = card2,
    Width = 350f,
    Height = 36f,
    PlaceholderText = "Type something here...",
    Location = new SKPoint(15, 45),
    PaddingX = 12f,
    TextSize = 13f,
    ZIndex = 1,
    OnTextChanged = (txt) => statusLabel.Text = $"Input 1 = \"{txt}\"",
    OnSubmit = (txt) => statusLabel.Text = $"Submitted = \"{txt}\""
};

var tb2 = new TextBox
{
    Parent = card2,
    Width = 350f,
    Height = 36f,
    Text = "Prefilled text value",
    PlaceholderText = "Search...",
    Location = new SKPoint(15, 95),
    PaddingX = 14f,
    BackgroundColor = SKColor.Parse("#313244"),
    BorderColor = SKColor.Parse("#45475A"),
    FocusBorderColor = SKColor.Parse("#89B4FA"),
    TextColor = SKColor.Parse("#89B4FA"),
    CaretColor = SKColor.Parse("#89B4FA"),
    ZIndex = 1,
    OnTextChanged = (txt) => statusLabel.Text = $"Input 2 = \"{txt}\""
};

var imgLabelTitle = new TextLabel
{
    Parent = card2,
    Text = "ImageLabel (Manual Sizing):",
    Location = new SKPoint(15, 150),
    TextSize = 13f,
    ZIndex = 1
};

var imgLabel = new ImageLabel
{
    Parent = card2,
    Image = logoImage,
    AutoSize = false,
    Location = new SKPoint(200, 142),
    Width = 32f,
    Height = 32f,
    ZIndex = 1
};

var imgBtn = new ImageButton
{
    Parent = card2,
    Image = logoImage,
    Text = "Image Btn",
    Width = 165f,
    Height = 38f,
    Location = new SKPoint(15, 190),
    AutoSizeImage = false,
    ImageWidth = 22f,
    ImageHeight = 22f,
    Spacing = 8f,
    ZIndex = 1,
    OnClick = () => statusLabel.Text = "ImageButton clicked"
};

var imgToggleBtn = new ImageToggleButton
{
    Parent = card2,
    OffImage = logoImage,
    OnImage = logoImage,
    Text = "Image Toggle",
    IsChecked = false,
    Width = 170f,
    Height = 38f,
    Location = new SKPoint(195, 190),
    AutoSizeImage = false,
    ImageWidth = 22f,
    ImageHeight = 22f,
    Spacing = 8f,
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"ImageToggleButton = {chk}"
};

var imageNoteLabel = new TextLabel
{
    Parent = card2,
    Text = "HandyUI is a free UI library for C#.",
    Location = new SKPoint(15, 248),
    TextSize = 11f,
    ZIndex = 1
};

var card3 = new Frame
{
    Width = 780f,
    Height = 290f,
    Location = new SKPoint(20, 385),
    ZIndex = 0,
    Opacity = 0.5f
};
renderer.AddRootControl(card3);

var card3Title = new TextLabel
{
    Parent = card3,
    Text = "ProgressBar & Slider Configurations",
    Location = new SKPoint(15, 15),
    TextSize = 15f,
    ZIndex = 1
};

var progressLabel1 = new TextLabel
{
    Parent = card3,
    Text = "Standard ProgressBar with percentage:",
    Location = new SKPoint(15, 45),
    TextSize = 13f,
    ZIndex = 1
};

var progressBar1 = new ProgressBar
{
    Parent = card3,
    Width = 750f,
    Height = 24f,
    Minimum = 0f,
    Maximum = 100f,
    Value = 65f,
    Location = new SKPoint(15, 70),
    ShowPercentage = true,
    ZIndex = 1
};

var progressLabel2 = new TextLabel
{
    Parent = card3,
    Text = "Custom color rounded ProgressBar (No text):",
    Location = new SKPoint(15, 105),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};

var progressBar2 = new ProgressBar
{
    Parent = card3,
    Width = 750f,
    Height = 16f,
    Minimum = 0f,
    Maximum = 100f,
    Value = 40f,
    Location = new SKPoint(15, 130),
    ShowPercentage = false,
    ProgressColor = SKColor.Parse("#A6E3A1"),
    TrackColor = SKColor.Parse("#1E1E2E"),
    BorderColor = SKColor.Parse("#A6E3A1"),
    ZIndex = 1
};

var sliderLabel1 = new TextLabel
{
    Parent = card3,
    Text = "Interactive Slider (Drives ProgressBar 1 & 2):",
    Location = new SKPoint(15, 160),
    TextSize = 13f,
    ZIndex = 1
};

var interactiveSlider1 = new Slider
{
    Parent = card3,
    Width = 750f,
    Minimum = 0f,
    Maximum = 100f,
    Value = 65f,
    Location = new SKPoint(15, 185),
    TrackHeight = 6f,
    ThumbRadius = 10f,
    ProgressColor = SKColor.Parse("#CBA6F7"),
    ThumbColor = SKColor.Parse("#B4BEFE"),
    ThumbHoverColor = SKColor.Parse("#89B4FA"),
    ZIndex = 1,
    OnValueChanged = (val) =>
    {
        progressBar1.Value = val;
        progressBar2.Value = val;
        statusLabel.Text = $"Slider 1 = {Math.Round(val, 1)}%";
    }
};

var sliderLabel2 = new TextLabel
{
    Parent = card3,
    Text = "Custom Thick Slider:",
    Location = new SKPoint(15, 220),
    TextSize = 13f,
    ZIndex = 1
};

var interactiveSlider2 = new Slider
{
    Parent = card3,
    Width = 750f,
    Minimum = 0f,
    Maximum = 200f,
    Value = 120f,
    Location = new SKPoint(15, 245),
    TrackHeight = 10f,
    ThumbRadius = 14f,
    ProgressColor = SKColor.Parse("#F9E2AF"),
    TrackColor = SKColor.Parse("#313244"),
    ThumbColor = SKColor.Parse("#FAB387"),
    ThumbHoverColor = SKColor.Parse("#F38BA8"),
    ZIndex = 1,
    OnValueChanged = (val) => statusLabel.Text = $"Thick Slider = {Math.Round(val, 1)} / 200"
};

#endregion

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);