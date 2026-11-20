using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data.Input;
using HandyUI.Core.Helpers;
using SkiaSharp;

public class ToggleSwitch : BaseControl<ToggleSwitch>
{
    public bool IsToggled { get; private set; }
    public event Action<bool>? WhenToggled;

    private readonly SKPaint _toggledPaint = new() { Color = ThemeManager.Instance.SurfacePrimary };
    private readonly SKPaint _untoggledPaint = new() { Color = ThemeManager.Instance.DisabledSurface };

    public override bool DoesIntersect(SKPoint point)
    {
        throw new NotImplementedException();
    }

    public override void OnDraw(SKCanvas canvas)
    {
        // Check the control state and branch depending on the current state.
        if (IsToggled)
        {
            // Draw the toggled state
            canvas.DrawRect(Bounds, _toggledPaint);
        }
        else
        {
            // Draw the untoggled state
            canvas.DrawRect(Bounds, _untoggledPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        // Handle clicks
        if (eventContext.Type == MouseEventType.ClickEvent && eventContext.Button == MouseButton.Left)
        {
            IsToggled = !IsToggled;
            WhenToggled?.Invoke(IsToggled);
            return true;
        }

        return false;
    }

    protected override void OnDispose()
    {
        _toggledPaint.Dispose();
        _untoggledPaint.Dispose();
    }
}