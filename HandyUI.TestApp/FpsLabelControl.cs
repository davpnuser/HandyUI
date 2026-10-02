using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.TestApp;

public class FpsLabelControl : UIControlBase
{
    private readonly SKPaint _paint;
    private readonly SKFont _font;

    private int _frameCount;
    private float _elapsedTime;
    private string _fpsText = "no data";

    public SKColor Color
    {
        get => _paint.Color;
        set => _paint.Color = value;
    }

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public FpsLabelControl()
    {
        Bounds = new SKRect(0, 0, 500, 500);

        _paint = new SKPaint
        {
            Color = SKColors.LimeGreen,
            IsAntialias = true
        };

        _font = new SKFont(SKTypeface.Default, 20);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        _frameCount++;
        _elapsedTime += deltaTime;

        if (_elapsedTime >= 0.5f)
        {
            var currentFps = (int)MathF.Round(_frameCount / _elapsedTime);
            _fpsText = $"fps: {currentFps}";

            _frameCount = 0;
            _elapsedTime = 0f;
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        var textY = Location.Y + _font.Size;
        canvas.DrawText(_fpsText, Location.X, textY, SKTextAlign.Left, _font, _paint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    protected override void OnDispose()
    {
        _paint.Dispose();
        _font.Dispose();
    }
}