using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextButton : UIControlBase
{
    private string _text = "Button";

    public string Text
    {
        get => _text;
        set { if (_text != value) { _text = value; RecalculateBounds(); Invalidate(); } }
    }

    public float TextSize
    {
        get => _font.Size;
        set { if (_font.Size != value) { _font.Size = value; RecalculateBounds(); Invalidate(); } }
    }

    public SKColor NormalColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#313244");

    public SKColor HoverColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#45475A");

    public SKColor PressedColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#585B70");

    public SKColor TextColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#CDD6F4");

    public SKColor BorderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#45475A");

    public float BorderWidth
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 1f;

    public float CornerRadius
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 6f;

    public float PaddingX
    {
        get;
        set { if (field != value) { field = value; RecalculateBounds(); Invalidate(); } }
    } = 16f;

    public float PaddingY
    {
        get;
        set { if (field != value) { field = value; RecalculateBounds(); Invalidate(); } }
    } = 8f;

    public Action? OnClick { get; set; }

    private SKColor _animatedColor;
    private bool _isPressed;

    private readonly SKPaint _backgroundPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public TextButton(string text = "Button")
    {
        _text = text;
        _animatedColor = NormalColor;
        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        var textWidth = _font.MeasureText(_text);
        var metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;

        Bounds = SKRect.Create(0, 0, textWidth + (PaddingX * 2), textHeight + (PaddingY * 2));
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        _backgroundPaint.Color = _animatedColor;
        canvas.DrawRoundRect(Bounds, CornerRadius, CornerRadius, _backgroundPaint);

        if (BorderWidth > 0 && BorderColor.Alpha > 0)
        {
            var halfStroke = BorderWidth / 2f;
            var strokeRect = Bounds;
            strokeRect.Inflate(-halfStroke, -halfStroke);

            _borderPaint.Color = BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(strokeRect, Math.Max(0, CornerRadius - halfStroke), Math.Max(0, CornerRadius - halfStroke), _borderPaint);
        }

        _textPaint.Color = TextColor;
        var metrics = _font.Metrics;
        var textY = (Bounds.Height / 2f) - ((metrics.Ascent + metrics.Descent) / 2f);

        canvas.DrawText(_text, Bounds.Width / 2f, textY, SKTextAlign.Center, _font, _textPaint);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        var targetColor = _isPressed ? PressedColor : IsHovered ? HoverColor : NormalColor;
        var oldColor = _animatedColor;

        _animatedColor = LerpColor(_animatedColor, targetColor, deltaTime * 12f);

        if (_animatedColor != oldColor)
        {
            Invalidate();
        }
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
        if (mouseContext.Type == MouseEventType.MouseDown && IsHovered && IsEnabled)
        {
            _isPressed = true;
            Invalidate();
            return true;
        }

        if (mouseContext.Type == MouseEventType.MouseUp)
        {
            if (_isPressed && IsHovered && IsEnabled)
            {
                OnClick?.Invoke();
            }
            _isPressed = false;
            Invalidate();
        }

        return base.OnMouse(mouseContext);
    }

    protected override void OnDispose()
    {
        _backgroundPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}