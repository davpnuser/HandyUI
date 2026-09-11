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

var backgroundFrame = new Frame(width: 820, height: 695)
{
    NormalColor = SKColor.Parse("#111117"),
    BorderColor = new(0, 0, 0, 0),
    CornerRadius = 0,
    ZIndex = -1
};
renderer.AddControl(backgroundFrame);

#endregion

#region Setting up the controls showcase

var titleLabel = new TextLabel("HandyUI Control Showcase")
{
    Location = new SKPoint(20, 20),
    TextSize = 22f,
    TextColor = SKColor.Parse("#CBA6F7"),
    ZIndex = 1
};
renderer.AddControl(titleLabel);

var statusFrame = new Frame(width: 360f, height: 36f)
{
    Location = new SKPoint(440, 15),
    CornerRadius = 8f,
    NormalColor = SKColor.Parse("#181825"),
    BorderColor = SKColor.Parse("#313244"),
    ZIndex = 0
};
renderer.AddControl(statusFrame);

var statusLabel = new TextLabel("Ready")
{
    Parent = statusFrame,
    Location = new SKPoint(12, 10),
    TextSize = 13f,
    TextColor = SKColor.Parse("#A6ADC8"),
    ZIndex = 1
};
renderer.AddControl(statusLabel);

var card1 = new Frame(width: 380f, height: 300f)
{
    Location = new SKPoint(20, 65),
    CornerRadius = 10f,
    NormalColor = SKColor.Parse("#181825"),
    BorderColor = SKColor.Parse("#313244"),
    ZIndex = 0
};
renderer.AddControl(card1);

var card1Title = new TextLabel("Buttons, Switches & CheckBoxes")
{
    Parent = card1,
    Location = new SKPoint(15, 15),
    TextSize = 15f,
    TextColor = SKColor.Parse("#89B4FA"),
    ZIndex = 1
};
renderer.AddControl(card1Title);

var textBtn1 = new TextButton("Standard Button", width: 165f, height: 36f)
{
    Parent = card1,
    Location = new SKPoint(15, 45),
    CornerRadius = 6f,
    ZIndex = 1,
    OnClick = () => statusLabel.Text = "TextButton 1 clicked"
};
renderer.AddControl(textBtn1);

var textBtn2 = new TextButton("Custom Color", width: 165f, height: 36f)
{
    Parent = card1,
    Location = new SKPoint(195, 45),
    CornerRadius = 18f,
    NormalColor = SKColor.Parse("#313244"),
    HoverColor = SKColor.Parse("#45475A"),
    PressedColor = SKColor.Parse("#585B70"),
    BorderColor = SKColor.Parse("#F38BA8"),
    TextColor = SKColor.Parse("#F38BA8"),
    ZIndex = 1,
    OnClick = () => statusLabel.Text = "Custom TextButton clicked"
};
renderer.AddControl(textBtn2);

var toggleBtn1 = new ToggleButton("Toggle Off", isChecked: false, width: 165f, height: 36f)
{
    Parent = card1,
    Location = new SKPoint(15, 95),
    CornerRadius = 6f,
    ZIndex = 1
};
toggleBtn1.OnToggled = (chk) =>
{
    toggleBtn1.Text = chk ? "Toggle On" : "Toggle Off";
    statusLabel.Text = $"ToggleButton = {chk}";
};
renderer.AddControl(toggleBtn1);

var toggleBtn2 = new ToggleButton("Custom Toggle", isChecked: true, width: 165f, height: 36f)
{
    Parent = card1,
    Location = new SKPoint(195, 95),
    CornerRadius = 8f,
    OnColor = SKColor.Parse("#A6E3A1"),
    OffColor = SKColor.Parse("#313244"),
    TextOnColor = SKColor.Parse("#11111B"),
    BorderColor = SKColor.Parse("#A6E3A1"),
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"Custom Toggle = {chk}"
};
renderer.AddControl(toggleBtn2);

var switch1 = new ToggleSwitch(isChecked: true, width: 52f, height: 26f)
{
    Parent = card1,
    Location = new SKPoint(15, 150),
    CornerRadius = 13f,
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"Switch 1 = {chk}"
};
renderer.AddControl(switch1);

var switch1Label = new TextLabel("Default Switch")
{
    Parent = card1,
    Location = new SKPoint(75, 155),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(switch1Label);

var switch2 = new ToggleSwitch(isChecked: false, width: 52f, height: 26f)
{
    Parent = card1,
    Location = new SKPoint(195, 150),
    CornerRadius = 4f,
    TrackOnColor = SKColor.Parse("#FAB387"),
    TrackOffColor = SKColor.Parse("#1E1E2E"),
    BorderColor = SKColor.Parse("#FAB387"),
    KnobColor = SKColor.Parse("#BAC2DE"),
    KnobOnColor = SKColor.Parse("#11111B"),
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"Custom Switch = {chk}"
};
renderer.AddControl(switch2);

var switch2Label = new TextLabel("Custom Switch")
{
    Parent = card1,
    Location = new SKPoint(255, 155),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(switch2Label);

var chk1 = new CheckBox("Enable standard setting", isChecked: true)
{
    Parent = card1,
    Location = new SKPoint(15, 195),
    BoxSize = 20f,
    Spacing = 10f,
    CornerRadius = 4f,
    ZIndex = 1,
    OnCheckChanged = (chk) => statusLabel.Text = $"CheckBox 1 = {chk}"
};
renderer.AddControl(chk1);

var chk2 = new CheckBox("Custom theme checkbox", isChecked: false)
{
    Parent = card1,
    Location = new SKPoint(15, 235),
    BoxSize = 22f,
    Spacing = 10f,
    CornerRadius = 11f,
    BoxOnColor = SKColor.Parse("#F9E2AF"),
    BoxOffColor = SKColor.Parse("#181825"),
    CheckMarkColor = SKColor.Parse("#11111B"),
    BorderColor = SKColor.Parse("#F9E2AF"),
    TextColor = SKColor.Parse("#F9E2AF"),
    ZIndex = 1,
    OnCheckChanged = (chk) => statusLabel.Text = $"CheckBox 2 = {chk}"
};
renderer.AddControl(chk2);

var card2 = new Frame(width: 380f, height: 279f)
{
    Location = new SKPoint(420, 65),
    CornerRadius = 10f,
    NormalColor = SKColor.Parse("#181825"),
    BorderColor = SKColor.Parse("#313244"),
    ZIndex = 0
};
renderer.AddControl(card2);

var card2Title = new TextLabel("TextBox & Manual Image Controls")
{
    Parent = card2,
    Location = new SKPoint(15, 15),
    TextSize = 15f,
    TextColor = SKColor.Parse("#89B4FA"),
    ZIndex = 1
};
renderer.AddControl(card2Title);

var tb1 = new TextBox(width: 350f, height: 36f, placeholder: "Type something here...")
{
    Parent = card2,
    Location = new SKPoint(15, 45),
    CornerRadius = 6f,
    PaddingX = 12f,
    TextSize = 13f,
    FocusBorderColor = SKColor.Parse("#CBA6F7"),
    CaretColor = SKColor.Parse("#CBA6F7"),
    ZIndex = 1,
    OnTextChanged = (txt) => statusLabel.Text = $"Input 1 = \"{txt}\"",
    OnSubmit = (txt) => statusLabel.Text = $"Submitted = \"{txt}\""
};
renderer.AddControl(tb1);

var tb2 = new TextBox(width: 350f, height: 36f, text: "Prefilled text value", placeholder: "Search...")
{
    Parent = card2,
    Location = new SKPoint(15, 95),
    CornerRadius = 18f,
    PaddingX = 14f,
    BackgroundColor = SKColor.Parse("#313244"),
    BorderColor = SKColor.Parse("#45475A"),
    FocusBorderColor = SKColor.Parse("#89B4FA"),
    TextColor = SKColor.Parse("#89B4FA"),
    CaretColor = SKColor.Parse("#89B4FA"),
    ZIndex = 1,
    OnTextChanged = (txt) => statusLabel.Text = $"Input 2 = \"{txt}\""
};
renderer.AddControl(tb2);

var imgLabelTitle = new TextLabel("ImageLabel (Manual Sizing):")
{
    Parent = card2,
    Location = new SKPoint(15, 150),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(imgLabelTitle);

var imgLabel = new ImageLabel(logoImage, autoSize: false)
{
    Parent = card2,
    Location = new SKPoint(200, 142),
    AutoSize = false,
    Width = 32f,
    Height = 32f,
    ZIndex = 1
};
renderer.AddControl(imgLabel);

var imgBtn = new ImageButton(logoImage, "Image Btn", width: 165f, height: 38f)
{
    Parent = card2,
    Location = new SKPoint(15, 190),
    AutoSizeImage = false,
    ImageWidth = 22f,
    ImageHeight = 22f,
    CornerRadius = 6f,
    Spacing = 8f,
    ZIndex = 1,
    OnClicked = () => statusLabel.Text = "ImageButton clicked"
};
renderer.AddControl(imgBtn);

var imgToggleBtn = new ImageToggleButton(logoImage, logoImage, "Image Toggle", isChecked: false, width: 170f, height: 38f)
{
    Parent = card2,
    Location = new SKPoint(195, 190),
    AutoSizeImage = false,
    ImageWidth = 22f,
    ImageHeight = 22f,
    CornerRadius = 6f,
    Spacing = 8f,
    OnColor = SKColor.Parse("#CBA6F7"),
    OffColor = SKColor.Parse("#1E1E2E"),
    TextOnColor = SKColor.Parse("#11111B"),
    TextOffColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1,
    OnToggled = (chk) => statusLabel.Text = $"ImageToggleButton = {chk}"
};
renderer.AddControl(imgToggleBtn);

var imageNoteLabel = new TextLabel("HandyUI is a free UI library for C#.")
{
    Parent = card2,
    Location = new SKPoint(15, 248),
    TextSize = 11f,
    TextColor = SKColor.Parse("#6C7086"),
    ZIndex = 1
};
renderer.AddControl(imageNoteLabel);

var card3 = new Frame(width: 780f, height: 290f)
{
    Location = new SKPoint(20, 385),
    CornerRadius = 10f,
    NormalColor = SKColor.Parse("#181825"),
    BorderColor = SKColor.Parse("#313244"),
    ZIndex = 0
};
renderer.AddControl(card3);

var card3Title = new TextLabel("ProgressBar & Slider Configurations")
{
    Parent = card3,
    Location = new SKPoint(15, 15),
    TextSize = 15f,
    TextColor = SKColor.Parse("#89B4FA"),
    ZIndex = 1
};
renderer.AddControl(card3Title);

var progressLabel1 = new TextLabel("Standard ProgressBar with percentage:")
{
    Parent = card3,
    Location = new SKPoint(15, 45),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(progressLabel1);

var progressBar1 = new ProgressBar(width: 750f, height: 24f, min: 0f, max: 100f, value: 65f)
{
    Parent = card3,
    Location = new SKPoint(15, 70),
    ShowPercentage = true,
    CornerRadius = 6f,
    ProgressColor = SKColor.Parse("#CBA6F7"),
    TrackColor = SKColor.Parse("#313244"),
    ZIndex = 1
};
renderer.AddControl(progressBar1);

var progressLabel2 = new TextLabel("Custom color rounded ProgressBar (No text):")
{
    Parent = card3,
    Location = new SKPoint(15, 105),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(progressLabel2);

var progressBar2 = new ProgressBar(width: 750f, height: 16f, min: 0f, max: 100f, value: 40f)
{
    Parent = card3,
    Location = new SKPoint(15, 130),
    ShowPercentage = false,
    CornerRadius = 8f,
    ProgressColor = SKColor.Parse("#A6E3A1"),
    TrackColor = SKColor.Parse("#1E1E2E"),
    BorderColor = SKColor.Parse("#A6E3A1"),
    ZIndex = 1
};
renderer.AddControl(progressBar2);

var sliderLabel1 = new TextLabel("Interactive Slider (Drives ProgressBar 1 & 2):")
{
    Parent = card3,
    Location = new SKPoint(15, 160),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(sliderLabel1);

var interactiveSlider1 = new Slider(width: 750f, min: 0f, max: 100f, value: 65f)
{
    Parent = card3,
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
renderer.AddControl(interactiveSlider1);

var sliderLabel2 = new TextLabel("Custom Thick Slider:")
{
    Parent = card3,
    Location = new SKPoint(15, 220),
    TextSize = 13f,
    TextColor = SKColor.Parse("#CDD6F4"),
    ZIndex = 1
};
renderer.AddControl(sliderLabel2);

var interactiveSlider2 = new Slider(width: 750f, min: 0f, max: 200f, value: 120f)
{
    Parent = card3,
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
renderer.AddControl(interactiveSlider2);

#endregion

MainForm.Controls.Add(skiaPanel);
Application.Run(MainForm);