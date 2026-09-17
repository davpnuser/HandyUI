using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class NonFocusableFrame : Frame
{
    public override bool Intersects(SKPoint clientPoint)
        => false;
}