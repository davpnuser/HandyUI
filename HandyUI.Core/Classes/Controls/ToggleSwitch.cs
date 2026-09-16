using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ToggleSwitch : UIControlBase
{
    public float Width
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 50f;

    public float Height
    {
        get;
        set { field = value; RecalculateBounds(); }
    } = 26f;

    public bool IsChecked { get; set; }
    public float CornerRadius { get; set; } = 13f;

    public SKColor TrackOffColor { get; set; } = SKColor.Parse("#fbfbfe");
    public SKColor TrackOnColor { get; set; } = SKColor.Parse("#1d72eb");
    public SKColor TrackHoverColor { get; set; } = SKColor.Parse("#7abdff");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#3b82f6");
    public SKColor KnobColor { get; set; } = SKColor.Parse("#3b82f6");
    public SKColor KnobOnColor { get; set; } = SKColor.Parse("#fbfbfe");

    public Action<bool>? OnToggled { get; set; }

    private float _animProgress;
    private SKColor _animatedTrackColor = SKColor.Parse("#fbfbfe");
    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _knobPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, Width, Height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        RecalculateBounds();
        var rect = SKRect.Create(0, 0, Width, Height);

        _trackPaint.Color = _animatedTrackColor;
        _borderPaint.Color = BorderColor;
        _knobPaint.Color = IsChecked ? KnobOnColor : KnobColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _trackPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var padding = 4f;
        var knobRadius = (rect.Height - (padding * 2f)) / 2f;
        var knobY = padding + knobRadius;

        var startX = padding + knobRadius;
        var endX = Width - padding - knobRadius;
        var knobX = startX + ((endX - startX) * _animProgress);

        canvas.DrawCircle(knobX, knobY, knobRadius, _knobPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetProgress = IsChecked ? 1f : 0f;
        _animProgress += (targetProgress - _animProgress) * deltaTime * 14f;

        var targetTrack = IsChecked ? TrackOnColor : IsHovered ? TrackHoverColor : TrackOffColor;
        _animatedTrackColor = LerpColor(_animatedTrackColor, targetTrack, deltaTime * 12f);
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
            OnToggled?.Invoke(IsChecked);
            return true;
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _trackPaint.Dispose();
        _borderPaint.Dispose();
        _knobPaint.Dispose();
        base.OnDispose();
    }
}