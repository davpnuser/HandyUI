using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.TestApp;

public class FpsLabelControl : UIControlBase
{
    private readonly SKPaint _paint;
    private readonly SKFont _font;

    private int _frameCount;
    private float _elapsedTime;
    private string _fpsText = "fps: 0";

    public SKColor Color
    {
        get => _paint.Color;
        set => _paint.Color = value;
    }

    public float TextSize
    {
        get => _font.Size;
        set
        {
            if (_font.Size != value)
            {
                _font.Size = value;
                UpdateBounds();
            }
        }
    }

    public FpsLabelControl()
    {
        _paint = new SKPaint
        {
            Color = SKColors.LimeGreen,
            IsAntialias = true
        };

        _font = new SKFont(SKTypeface.Default, 20);

        UpdateBounds();
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

            UpdateBounds();
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawText(_fpsText, 0, -_font.Metrics.Ascent, SKTextAlign.Left, _font, _paint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    private void UpdateBounds()
    {
        var width = _font.MeasureText(_fpsText);
        var height = _font.Metrics.Descent - _font.Metrics.Ascent;

        Bounds = SKRect.Create(0, 0, width, height);
    }

    protected override void OnDispose()
    {
        _paint.Dispose();
        _font.Dispose();
    }
}