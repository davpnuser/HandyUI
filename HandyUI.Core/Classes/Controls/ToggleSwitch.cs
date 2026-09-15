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

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["Width"] = Width,
            ["Height"] = Height,
            ["IsChecked"] = IsChecked,
            ["CornerRadius"] = CornerRadius,
            ["TrackOffColor"] = TrackOffColor.ToString(),
            ["TrackOnColor"] = TrackOnColor.ToString(),
            ["TrackHoverColor"] = TrackHoverColor.ToString(),
            ["BorderColor"] = BorderColor.ToString(),
            ["KnobColor"] = KnobColor.ToString(),
            ["KnobOnColor"] = KnobOnColor.ToString()
        };
    }

    public override void Deserialize(Dictionary<string, object> values)
    {
        if (values.TryGetValue("Width", out var w) && float.TryParse(w.ToString(), out var wVal)) Width = wVal;
        if (values.TryGetValue("Height", out var h) && float.TryParse(h.ToString(), out var hVal)) Height = hVal;
        if (values.TryGetValue("IsChecked", out var chk) && bool.TryParse(chk.ToString(), out var chkVal)) IsChecked = chkVal;
        if (values.TryGetValue("CornerRadius", out var cr) && float.TryParse(cr.ToString(), out var crVal)) CornerRadius = crVal;

        if (values.TryGetValue("TrackOffColor", out var toc) && toc != null) TrackOffColor = SKColor.Parse(toc.ToString());
        if (values.TryGetValue("TrackOnColor", out var tonc) && tonc != null) TrackOnColor = SKColor.Parse(tonc.ToString());
        if (values.TryGetValue("TrackHoverColor", out var thc) && thc != null) TrackHoverColor = SKColor.Parse(thc.ToString());
        if (values.TryGetValue("BorderColor", out var bc) && bc != null) BorderColor = SKColor.Parse(bc.ToString());
        if (values.TryGetValue("KnobColor", out var kc) && kc != null) KnobColor = SKColor.Parse(kc.ToString());
        if (values.TryGetValue("KnobOnColor", out var konc) && konc != null) KnobOnColor = SKColor.Parse(konc.ToString());
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, _width, _height);

        _trackPaint.Color = _animatedTrackColor;
        _borderPaint.Color = BorderColor;
        _knobPaint.Color = IsChecked ? KnobOnColor : KnobColor;

        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _trackPaint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var padding = 4f;
        var knobRadius = (rect.Height - (padding * 2f)) / 2f;
        var knobY = padding + knobRadius;

        var startX = padding + knobRadius;
        var endX = _width - padding - knobRadius;
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