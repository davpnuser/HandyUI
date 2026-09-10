using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;
using System.Diagnostics;

namespace HandyUI.Core.Classes.Controls;

public class DebugControl : UIControlBase
{
    private readonly SKPaint _paint1;
    private readonly SKPaint _paint2;
    private readonly SKPaint _paint3;
    private readonly List<SKPoint> _lastClicks = [];
    private readonly List<SKPoint> _lastKeys = [];
    private SKPoint _mousePos = SKPoint.Empty;
    private MouseEventContext? _lastMouseContext;
    private KeyEventContext? _lastKeyContext;
    private float _latestDelta = 0f;
    private float _latestFps = 0f;
    private long _drawRequestId = 0;

    public DebugControl()
    {
        _paint1 = new SKPaint
        {
            Color = new SKColor(0, 0, 0),
            Style = SKPaintStyle.Stroke
        };

        _paint2 = new SKPaint
        {
            Color = new SKColor(255, 0, 0),
            Style = SKPaintStyle.Fill
        };

        _paint3 = new SKPaint
        {
            Color = new SKColor(0, 255, 0),
            Style = SKPaintStyle.Fill
        };
    }

    public override bool Intersects(SKPoint clientPoint)
        => true;

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        _latestDelta = deltaTime;
        _latestFps = 1.0f / deltaTime;

        _mousePos = clientMousePosition;
    }

    public override void Draw(SKCanvas canvas)
    {
        _drawRequestId++;

        foreach (var point in _lastKeys)
            canvas.DrawRect(point.X - 1, point.Y - 1, 3, 3, _paint3);

        foreach (var point in _lastClicks)
            canvas.DrawRect(point.X - 1, point.Y - 1, 3, 3, _paint2);

        canvas.DrawRect(_mousePos.X - 1, _mousePos.Y - 1, 3, 3, _paint1);
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.Move)
            return false;

        Debugger.Break();
        _lastClicks.Add(mouseContext.ClientPosition);
        _lastMouseContext = mouseContext;
        return true;
    }

    protected override bool OnKey(KeyEventContext keyContext)
    {
        Debugger.Break();
        _lastKeys.Add(_mousePos);
        _lastKeyContext = keyContext;
        return true;
    }

    protected override void OnDispose()
    {
        _paint1.Dispose();
        _paint2.Dispose();
        _paint3.Dispose();
    }
}