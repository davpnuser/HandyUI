using HandyUI.Core.Abstractions.Base;
using SkiaSharp;

public class ToggleSwitch : BaseControl<ToggleSwitch>
{
    public bool Checked { get; private set; }
    public event Action<bool>? WhenToggled;

    public override bool DoesIntersect(SKPoint point)
    {
        throw new NotImplementedException();
    }

    public override void OnDraw(SKCanvas canvas)
    {

    }
}