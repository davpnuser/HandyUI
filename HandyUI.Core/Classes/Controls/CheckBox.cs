using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class CheckBox : UIControlBase
{
    public string Text
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = "CheckBox";

    public bool IsChecked { get; set; }

    public float BoxSize
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 20f;

    public float Spacing
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 8f;

    public float CornerRadius { get; set; } = 0f;

    public float TextSize
    {
        get => _font.Size;
        set { _font.Size = value; RecalculateBounds(); }
    }

    public SKColor BoxOffColor { get; set; } = SKColor.Parse("#fbfbfe");
    public SKColor BoxOnColor { get; set; } = SKColor.Parse("#1d72eb");
    public SKColor BoxHoverColor { get; set; } = SKColor.Parse("#7abdff");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#3b82f6");
    public SKColor CheckMarkColor { get; set; } = SKColor.Parse("#fbfbfe");
    public SKColor TextColor { get; set; } = SKColor.Parse("#040316");

    public Action<bool>? OnCheckChanged { get; set; }

    private float _animProgress;
    private SKColor _animatedBoxColor = SKColor.Parse("#1E1E2E");

    private readonly SKPaint _boxPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _checkPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    private void RecalculateBounds()
    {
        var textWidth = string.IsNullOrEmpty(Text) ? 0f : _font.MeasureText(Text);
        var metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        var totalWidth = BoxSize + (string.IsNullOrEmpty(Text) ? 0f : Spacing + textWidth);
        var totalHeight = Math.Max(BoxSize, textHeight);

        Bounds = SKRect.Create(0, 0, totalWidth, totalHeight);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var boxY = (Bounds.Height - BoxSize) / 2f;
        var boxRect = SKRect.Create(0, boxY, BoxSize, BoxSize);

        _boxPaint.Color = _animatedBoxColor;
        _borderPaint.Color = BorderColor;

        canvas.DrawRoundRect(boxRect, CornerRadius, CornerRadius, _boxPaint);
        canvas.DrawRoundRect(boxRect, CornerRadius, CornerRadius, _borderPaint);

        if (_animProgress > 0.01f)
        {
            _checkPaint.Color = CheckMarkColor.WithAlpha((byte)(CheckMarkColor.Alpha * _animProgress));

            var p1 = new SKPoint(boxRect.Left + (boxRect.Width * 0.25f), boxRect.Top + (boxRect.Height * 0.5f));
            var p2 = new SKPoint(boxRect.Left + (boxRect.Width * 0.45f), boxRect.Top + (boxRect.Height * 0.7f));
            var p3 = new SKPoint(boxRect.Left + (boxRect.Width * 0.75f), boxRect.Top + (boxRect.Height * 0.3f));

            using var builder = new SKPathBuilder();
            builder.MoveTo(p1);
            builder.LineTo(p2);
            builder.LineTo(p3);
            using var path = builder.Detach();

            canvas.DrawPath(path, _checkPaint);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = TextColor;
            var metrics = _font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var textX = BoxSize + Spacing;
            var textY = ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

            canvas.DrawText(Text, textX, textY, SKTextAlign.Left, _font, _textPaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetProgress = IsChecked ? 1f : 0f;
        _animProgress += (targetProgress - _animProgress) * deltaTime * 14f;

        var targetBoxColor = IsChecked ? BoxOnColor : IsHovered ? BoxHoverColor : BoxOffColor;
        _animatedBoxColor = LerpColor(_animatedBoxColor, targetBoxColor, deltaTime * 12f);
    }

    private static SKColor LerpColor(SKColor from, SKColor to, float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        var r = (byte)(from.Red + ((to.Red - from.Red) * progress));
        var g = (byte)(from.Green + ((to.Green - from.Green) * progress));
        var b = (byte)(from.Blue + ((to.Blue - from.Blue) * progress));
        var a = (byte)(from.Alpha + ((to.Alpha - from.Alpha) * progress));
        return new SKColor(r, g, b, a);
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            IsChecked = !IsChecked;
            OnCheckChanged?.Invoke(IsChecked);
            return true;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _boxPaint.Dispose();
        _borderPaint.Dispose();
        _checkPaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}