using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextBox : UIControlBase
{
    private float _height = 36f;
    private float _width = 200f;
    private string _placeholder = "Type here...";

    public float Height
    {
        get => _height;
        set { if (_height != value) { _height = value; RecalculateBounds(); Invalidate(); } }
    }

    public float Width
    {
        get => _width;
        set { if (_width != value) { _width = value; RecalculateBounds(); Invalidate(); } }
    }

    public string Text
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _cursorPosition = Math.Clamp(_cursorPosition, 0, field.Length);
                OnTextChanged?.Invoke(field);
                Invalidate();
            }
        }
    } = string.Empty;

    public string Placeholder
    {
        get => _placeholder;
        set { if (_placeholder != value) { _placeholder = value; Invalidate(); } }
    }

    public float TextSize
    {
        get => _font.Size;
        set { if (_font.Size != value) { _font.Size = value; Invalidate(); } }
    }

    public float CornerRadius
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 6f;

    public float BorderWidth
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = 1.5f;

    public SKColor BackgroundColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#1E1E2E");

    public SKColor BorderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#313244");

    public SKColor FocusedBorderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#CBA6F7");

    public SKColor TextColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#CDD6F4");

    public SKColor PlaceholderColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#6C7086");

    public SKColor CursorColor
    {
        get;
        set { if (field != value) { field = value; Invalidate(); } }
    } = SKColor.Parse("#F5E0DC");

    public Action<string>? OnTextChanged { get; set; }

    private bool _isFocused;
    private int _cursorPosition;
    private float _cursorBlinkTimer;
    private bool _cursorVisible = true;

    private readonly SKPaint _bgPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _cursorPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
    private readonly SKFont _font = new(SKTypeface.Default, 14f) { Subpixel = true };

    public TextBox(float width = 200f, float height = 36f, string placeholder = "Type here...")
    {
        _width = width;
        _height = height;
        _placeholder = placeholder;
        RecalculateBounds();
    }

    private void RecalculateBounds()
    {
        Bounds = SKRect.Create(0, 0, _width, _height);
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, _width, _height);

        _bgPaint.Color = BackgroundColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _bgPaint);

        if (BorderWidth > 0)
        {
            var halfStroke = BorderWidth / 2f;
            var strokeRect = rect;
            strokeRect.Inflate(-halfStroke, -halfStroke);

            _borderPaint.Color = _isFocused ? FocusedBorderColor : BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRoundRect(strokeRect, Math.Max(0, CornerRadius - halfStroke), Math.Max(0, CornerRadius - halfStroke), _borderPaint);
        }

        var metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        var paddingX = 10f;
        var textY = ((_height + textHeight) / 2f) - metrics.Descent;

        if (string.IsNullOrEmpty(Text))
        {
            _textPaint.Color = PlaceholderColor;
            canvas.DrawText(Placeholder, paddingX, textY, SKTextAlign.Left, _font, _textPaint);
        }
        else
        {
            _textPaint.Color = TextColor;
            canvas.DrawText(Text, paddingX, textY, SKTextAlign.Left, _font, _textPaint);
        }

        if (_isFocused && _cursorVisible)
        {
            var textBeforeCursor = Text[..Math.Min(_cursorPosition, Text.Length)];
            var cursorX = paddingX + _font.MeasureText(textBeforeCursor);
            var cursorY1 = (_height - textHeight) / 2f;
            var cursorY2 = cursorY1 + textHeight;

            _cursorPaint.Color = CursorColor;
            canvas.DrawLine(cursorX, cursorY1, cursorX, cursorY2, _cursorPaint);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (_isFocused)
        {
            _cursorBlinkTimer += deltaTime;
            if (_cursorBlinkTimer >= 0.53f)
            {
                _cursorBlinkTimer = 0f;
                _cursorVisible = !_cursorVisible;
                Invalidate();
            }
        }
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseDown)
        {
            var wasFocused = _isFocused;
            _isFocused = IsHovered && IsEnabled;
            if (_isFocused != wasFocused)
            {
                _cursorVisible = true;
                _cursorBlinkTimer = 0f;
                Invalidate();
            }
            return _isFocused;
        }

        return base.OnMouse(mouseContext);
    }

    public bool OnTextInput(string text)
    {
        if (!_isFocused || !IsEnabled) return false;

        Text = Text.Insert(_cursorPosition, text);
        _cursorPosition += text.Length;
        _cursorVisible = true;
        _cursorBlinkTimer = 0f;
        return true;
    }

    public bool OnKeyDown(int key)
    {
        if (!_isFocused || !IsEnabled) return false;

        if (key == 8 && _cursorPosition > 0) // Backspace
        {
            Text = Text.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
            _cursorVisible = true;
            _cursorBlinkTimer = 0f;
            return true;
        }

        if (key == 37 && _cursorPosition > 0) // Left Arrow
        {
            _cursorPosition--;
            _cursorVisible = true;
            _cursorBlinkTimer = 0f;
            Invalidate();
            return true;
        }

        if (key == 39 && _cursorPosition < Text.Length) // Right Arrow
        {
            _cursorPosition++;
            _cursorVisible = true;
            _cursorBlinkTimer = 0f;
            Invalidate();
            return true;
        }

        return false;
    }

    protected override void OnDispose()
    {
        _bgPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _cursorPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}