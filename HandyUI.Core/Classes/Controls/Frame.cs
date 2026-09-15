using HandyUI.Core.Classes.Base;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class Frame : UIControlBase
{
    private float _height;
    private float _width;

    public float Height
    {
        get => _height;
        set { _height = value; RecalculateBounds(); }
    }

    public float Width
    {
        get => _width;
        set { _width = value; RecalculateBounds(); }
    }

    public float CornerRadius { get; set; } = 6f;

    public SKColor NormalColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#CBA6F7");

    private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };

    public Frame(float width = 150f, float height = 150f)
    {
        _height = height;
        _width = width;
        RecalculateBounds();
    }

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["Width"] = Width,
            ["Height"] = Height,
            ["CornerRadius"] = CornerRadius,
            ["NormalColor"] = NormalColor.ToString(),
            ["BorderColor"] = BorderColor.ToString()
        };
    }

    public override void Deserialize(Dictionary<string, object> values)
    {
        if (values.TryGetValue("Width", out var w) && float.TryParse(w.ToString(), out var wVal)) Width = wVal;
        if (values.TryGetValue("Height", out var h) && float.TryParse(h.ToString(), out var hVal)) Height = hVal;
        if (values.TryGetValue("CornerRadius", out var cr) && float.TryParse(cr.ToString(), out var crVal)) CornerRadius = crVal;

        if (values.TryGetValue("NormalColor", out var nc) && nc != null) NormalColor = SKColor.Parse(nc.ToString());
        if (values.TryGetValue("BorderColor", out var bc) && bc != null) BorderColor = SKColor.Parse(bc.ToString());
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, _width, _height);

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
        ;
    }

    protected override void OnDispose()
    {
        _fillPaint.Dispose();
        _borderPaint.Dispose();
        base.OnDispose();
    }
}