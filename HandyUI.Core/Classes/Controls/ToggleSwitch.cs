using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ToggleSwitch : UIControlBase
{
    public bool IsChecked { get; set; }
    public float CornerRadius { get; set; } = 13f;

    public SKColor TrackOffColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor TrackOnColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor TrackHoverColor { get; set; } = SKColor.Parse("#313244");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#585B70");
    public SKColor KnobColor { get; set; } = SKColor.Parse("#CDD6F4");
    public SKColor KnobOnColor { get; set; } = SKColor.Parse("#11111B");

    public Action<bool>? OnToggled { get; set; }

    private readonly SKPaint _trackPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _knobPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    public ToggleSwitch(bool isChecked = false, float width = 50f, float height = 26f)
    {
        IsChecked = isChecked;
        Bounds = SKRect.Create(Location.X, Location.Y, width, height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(Location.X, Location.Y, Bounds.Width, Bounds.Height);

        _trackPaint.Color = IsChecked ? TrackOnColor : IsHovered ? TrackHoverColor : TrackOffColor;

        _borderPaint.Color = BorderColor;
        _knobPaint.Color = IsChecked ? KnobOnColor : KnobColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _trackPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var padding = 4f;
        var knobRadius = (rect.Height - (padding * 2f)) / 2f;
        var knobY = rect.Top + padding + knobRadius;

        var knobX = IsChecked
            ? rect.Right - padding - knobRadius
            : rect.Left + padding + knobRadius;

        canvas.DrawCircle(knobX, knobY, knobRadius, _knobPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        var rect = SKRect.Create(Location.X, Location.Y, Bounds.Width, Bounds.Height);
        return rect.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (Bounds.Left != Location.X || Bounds.Top != Location.Y)
        {
            Bounds = SKRect.Create(Location.X, Location.Y, Bounds.Width, Bounds.Height);
        }
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