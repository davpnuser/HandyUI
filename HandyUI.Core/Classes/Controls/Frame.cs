using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Frame : UIControlBase
{
    public float Height
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 150f;

    public float Width
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 150f;

    public float CornerRadius { get; set; } = 0f;

    public SKColor NormalColor { get; set; } = SKColor.Parse("#fbfbfe");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#1d72eb");

    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, Width, Height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var rect = SKRect.Create(0, 0, Width, Height);

        _fillPaint.Color = NormalColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _fillPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
    }

    protected override void OnDispose()
    {
        _fillPaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }
}