using SkiaSharp.Views.Desktop;

namespace HandyUI.WinForms.Components;

public class HandyUIGLControl : SKGLControl
{
    protected override bool IsInputKey(Keys keyData) => true;
}