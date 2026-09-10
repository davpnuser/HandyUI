using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextLabel : UIControlBase
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 16f) { Subpixel = true };

    public string Text
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = string.Empty;

    public SKColor TextColor { get; set; } = SKColors.White;

    public float TextSize
    {
        get; set
        {
            field = value;
            _font.Size = value;
            RecalculateBounds();
        }
    } = 16f;

    public SKTypeface Typeface
    {
        get; set
        {
            field = value;
            _font.Typeface = value;
            RecalculateBounds();
        }
    } = SKTypeface.Default;

    public TextLabel(string text = "")
    {
        Text = text;
    }

    private void RecalculateBounds()
    {
        var width = _font.MeasureText(Text);
        var metrics = _font.Metrics;
        var height = metrics.Descent - metrics.Ascent;

        Bounds = SKRect.Create(Location.X, Location.Y, width, height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible || string.IsNullOrEmpty(Text)) return;

        _paint.Color = TextColor;

        var baselineY = Location.Y - _font.Metrics.Ascent;
        canvas.DrawText(Text, Location.X, baselineY, SKTextAlign.Left, _font, _paint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (Bounds.Left != Location.X || Bounds.Top != Location.Y)
        {
            RecalculateBounds();
        }
    }

    protected override void OnDispose()
    {
        _paint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}