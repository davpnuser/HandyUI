using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using HandyUI.Core.Helpers.Extensions;
using SkiaSharp;

namespace HandyUI.TestApp.Controls;

public class MouseDisplayControl : BaseControl<MouseDisplayControl>
{
    private SKSize _lastWindowSize;

    private int _lastMousePosX = 0;
    private int _lastMousePosY = 0;

    private readonly SKPaint _paint = new() { Color = ThemeManager.Instance.Accent, IsAntialias = true, Style = SKPaintStyle.Fill };

    public override bool DoesIntersect(SKPoint point) => true;
    public override void OnDraw(SKCanvas canvas) => canvas.DrawCircle(_lastMousePosX, _lastMousePosY, 15, _paint);

    public override void OnUpdate(UpdateContext updateContext)
    {
        if (_lastWindowSize != updateContext.Host.Size)
        {
            _lastWindowSize = updateContext.Host.Size;
            Bounds = updateContext.Host.Size.ToSKRect();
        }
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (eventContext.MousePosition is null)
            return false;

        if (eventContext.Type == MouseEventType.MoveEvent)
        {
            _lastMousePosX = (int)eventContext.MousePosition.Value.X;
            _lastMousePosY = (int)eventContext.MousePosition.Value.Y;

            //Invalidate();
        }

        return true;
    }

    protected override void OnDispose()
        => _paint.Dispose();
}