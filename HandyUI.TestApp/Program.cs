using HandyUI.Controls.Controls;
using HandyUI.Core.Components;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;
using SkiaSharp;

var config = WindowConfiguration.Default with
{
    Title = "Control Showcase",
    Size = new(1050, 1000),
    Icon = ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var iconStream)
        ? SKImage.FromEncodedData(iconStream)
        : null
};

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(config);

SKImage? logoImage = null;
if (ResourceManager.GetResourceByPath("Resources/HandyUI-SmallLogo.png", out var logoStream))
    logoImage = SKImage.FromEncodedData(logoStream);

#region Left Column

new Label().WithText("BUTTONS").WithIsBold(true).WithLocation(30, 20).WithAddToRenderer(renderer);

new Button()
    .WithText("Click Me")
    .WithLocation(30, 55)
    .WithAddToRenderer(renderer);

new Button()
    .WithText("Compact")
    .WithPadding(8, 4)
    .WithLocation(160, 55)
    .WithAddToRenderer(renderer);

new Button()
    .WithText("Large")
    .WithPadding(24, 12)
    .WithElevation(4)
    .WithIsBold(true)
    .WithLocation(290, 55)
    .WithAddToRenderer(renderer);

new Label().WithText("LABELS").WithIsBold(true).WithLocation(30, 120).WithAddToRenderer(renderer);

new Label()
    .WithText("Regular label text")
    .WithLocation(30, 155)
    .WithAddToRenderer(renderer);

new Label()
    .WithText("Bold label")
    .WithIsBold(true)
    .WithLocation(30, 185)
    .WithAddToRenderer(renderer);

new Label()
    .WithText("Large label")
    .WithFontSize(20)
    .WithLocation(30, 215)
    .WithAddToRenderer(renderer);

new Label().WithText("ITEM BOXES").WithIsBold(true).WithLocation(30, 270).WithAddToRenderer(renderer);

new ItemBox()
    .WithText("Unselected item")
    .WithLocation(30, 305)
    .WithWidth(470)
    .WithHeight(36)
    .WithAddToRenderer(renderer);

new ItemBox()
    .WithText("Selected item")
    .WithIsSelected(true)
    .WithLocation(30, 350)
    .WithWidth(470)
    .WithHeight(36)
    .WithAddToRenderer(renderer);

new ItemBox()
    .WithText("Another item")
    .WithLocation(30, 395)
    .WithWidth(470)
    .WithHeight(36)
    .WithAddToRenderer(renderer);

new Label().WithText("CHECKBOXES").WithIsBold(true).WithLocation(30, 460).WithAddToRenderer(renderer);

new CheckBox()
    .WithText("Unchecked option")
    .WithLocation(30, 495)
    .WithAddToRenderer(renderer);

new CheckBox()
    .WithText("Checked option")
    .WithIsChecked(true)
    .WithLocation(30, 535)
    .WithAddToRenderer(renderer);

new Label().WithText("TOGGLES").WithIsBold(true).WithLocation(30, 595).WithAddToRenderer(renderer);

new Toggle()
    .WithText("Toggle off")
    .WithLocation(30, 630)
    .WithAddToRenderer(renderer);

new Toggle()
    .WithText("Toggle on")
    .WithIsOn(true)
    .WithLocation(30, 675)
    .WithAddToRenderer(renderer);

new Label().WithText("LIST BOX").WithIsBold(true).WithLocation(30, 740).WithAddToRenderer(renderer);

var listBox = new ListBox()
    .WithLocation(30, 775)
    .WithWidth(470)
    .WithHeight(150)
    .WithItemSpacing(0)
    .WithAddToRenderer(renderer);

new ItemBox().WithText("List item 1").WithWidth(470).WithHeight(36).WithParent(listBox);
new ItemBox().WithText("List item 2").WithWidth(470).WithHeight(36).WithParent(listBox);
new ItemBox().WithText("List item 3").WithWidth(470).WithHeight(36).WithParent(listBox);

#endregion

#region Right Column

new Label().WithText("PICTURES").WithIsBold(true).WithLocation(550, 20).WithAddToRenderer(renderer);

if (logoImage is not null)
{
    new Picture()
        .WithImage(logoImage)
        .WithLocation(550, 55)
        .WithWidth(100)
        .WithHeight(100)
        .WithAddToRenderer(renderer);

    new PictureButton()
        .WithImage(logoImage)
        .WithLocation(670, 55)
        .WithWidth(100)
        .WithHeight(100)
        .WithAddToRenderer(renderer);
}

new Label().WithText("PROGRESS BARS").WithIsBold(true).WithLocation(550, 180).WithAddToRenderer(renderer);

new ProgressBar()
    .WithValue(60)
    .WithBarHeight(6)
    .WithLocation(550, 215)
    .WithWidth(470)
    .WithHeight(20)
    .WithAddToRenderer(renderer);

new ProgressBar()
    .WithIsIndeterminate(true)
    .WithBarHeight(6)
    .WithLocation(550, 245)
    .WithWidth(470)
    .WithHeight(20)
    .WithAddToRenderer(renderer);

new Label().WithText("SLIDER").WithIsBold(true).WithLocation(550, 290).WithAddToRenderer(renderer);

new Slider()
    .WithMin(0)
    .WithMax(100)
    .WithValue(35)
    .WithLocation(550, 325)
    .WithWidth(470)
    .WithHeight(30)
    .WithAddToRenderer(renderer);

new Label().WithText("TEXT INPUT").WithIsBold(true).WithLocation(550, 380).WithAddToRenderer(renderer);

new TextInput()
    .WithPlaceholder("Type here...")
    .WithLocation(550, 415)
    .WithWidth(470)
    .WithHeight(40)
    .WithAddToRenderer(renderer);

new Label().WithText("MULTILINE TEXT INPUT").WithIsBold(true).WithLocation(550, 480).WithAddToRenderer(renderer);

new MultilineTextInput()
    .WithText("Line one\nLine two\nLine three\nLine four\nLine five")
    .WithLocation(550, 515)
    .WithWidth(470)
    .WithHeight(130)
    .WithAddToRenderer(renderer);

new Label().WithText("FRAME").WithIsBold(true).WithLocation(550, 660).WithAddToRenderer(renderer);

var frame = new Frame()
    .WithLocation(550, 695)
    .WithWidth(470)
    .WithHeight(100)
    .WithElevation(2)
    .WithAddToRenderer(renderer);

new Label()
    .WithText("Content inside a frame")
    .WithLocation(20, 40)
    .WithParent(frame);

new Label().WithText("SCROLL FRAME").WithIsBold(true).WithLocation(550, 820).WithAddToRenderer(renderer);

var scrollFrame = new ScrollFrame()
    .WithLocation(550, 855)
    .WithWidth(470)
    .WithHeight(110)
    .WithScrollSpeed(50)
    .WithAddToRenderer(renderer);

for (var i = 0; i < 6; i++)
{
    new Label()
        .WithText($"Scrollable item {i + 1}")
        .WithLocation(12, 10 + (i * 40))
        .WithParent(scrollFrame);
}

#endregion

await window.RunAsync();