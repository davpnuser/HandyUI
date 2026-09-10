using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ToggleSwitch : UIControlBase
{
    private float _width;
    private float _height;

    public float Width
    {
        get => _width;
        set { _width = value; RecalculateBounds(); }
    }

    public float Height
    {
        get => _height;
        set { _height = value; RecalculateBounds(); }
    }

    public bool IsChecked { get; set; }
    public float CornerRadius { get; set; } = 13f;

    public SKColor TrackOffColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor TrackOnColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor TrackHoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor KnobColor { get; set; } = SKColor.Parse("#CDD6F4");
    public SKColor KnobOnColor { get; set; } = SKColor.Parse("#11111B");

    public Action<bool>? OnToggled { get; set; }

    private float _animProgress;
    private SKColor _animatedTrackColor;
    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _knobPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    public ToggleSwitch(bool isChecked = false, float width = 50f, float height = 26f)
    {
        IsChecked = isChecked;
        _width = width;
        _height = height;
        _animProgress = isChecked ? 1f : 0f;
        _animatedTrackColor = isChecked ? TrackOnColor : TrackOffColor;
        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(Location.X, Location.Y, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(Location.X, Location.Y, _width, _height);

        _trackPaint.Color = _animatedTrackColor;
        _borderPaint.Color = BorderColor;
        _knobPaint.Color = IsChecked ? KnobOnColor : KnobColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _trackPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var padding = 4f;
        var knobRadius = (rect.Height - (padding * 2f)) / 2f;
        var knobY = rect.Top + padding + knobRadius;

        var startX = rect.Left + padding + knobRadius;
        var endX = rect.Right - padding - knobRadius;
        var knobX = startX + ((endX - startX) * _animProgress);

        canvas.DrawCircle(knobX, knobY, knobRadius, _knobPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        if (RetainedModePositioning)
        {
            var localRect = SKRect.Create(0, 0, _width, _height);
            return localRect.Contains(clientPoint);
        }

        var rect = SKRect.Create(Location.X, Location.Y, _width, _height);
        return rect.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (Bounds.Left != Location.X || Bounds.Top != Location.Y)
        {
            RecalculateBounds();
        }

        var targetProgress = IsChecked ? 1f : 0f;
        _animProgress += (targetProgress - _animProgress) * deltaTime * 14f;

        SKColor targetTrack = IsChecked ? TrackOnColor : IsHovered ? TrackHoverColor : TrackOffColor;
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