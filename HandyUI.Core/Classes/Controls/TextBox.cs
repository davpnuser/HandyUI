using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextBox : UIControlBase
{
    private string _text = string.Empty;
    private string _placeholderText = "Type here...";
    private int _caretIndex;
    private float _blinkTimer;
    private bool _showCaret = true;

    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _caretIndex = Math.Clamp(_caretIndex, 0, _text.Length);
            OnTextChanged?.Invoke(_text);
        }
    }

    public string PlaceholderText
    {
        get => _placeholderText;
        set => _placeholderText = value ?? string.Empty;
    }

    public float CornerRadius { get; set; } = 6f;
    public float PaddingX { get; set; } = 10f;

    public float TextSize
    {
        get => _font.Size;
        set => _font.Size = value;
    }

    public SKColor BackgroundColor { get; set; } = SKColor.Parse("#1E1E2E");
    public SKColor BorderColor { get; set; } = SKColor.Parse("#45475A");
    public SKColor FocusBorderColor { get; set; } = SKColor.Parse("#CBA6F7");
    public SKColor TextColor { get; set; } = SKColor.Parse("#CDD6F4");
    public SKColor PlaceholderColor { get; set; } = SKColor.Parse("#6C7086");
    public SKColor CaretColor { get; set; } = SKColor.Parse("#CBA6F7");

    public Action<string>? OnTextChanged { get; set; }
    public Action<string>? OnSubmit { get; set; }

    private readonly SKPaint _bgPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _caretPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKFont _font = new(SKTypeface.Default, 13f) { Subpixel = true };

    public TextBox(float width = 200f, float height = 36f, string text = "", string placeholder = "Type here...")
    {
        _text = text ?? string.Empty;
        _placeholderText = placeholder ?? string.Empty;
        _caretIndex = _text.Length;
        Bounds = SKRect.Create(0, 0, width, height);
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (IsFocused)
        {
            _blinkTimer += deltaTime;
            if (_blinkTimer >= 0.5f)
            {
                _showCaret = !_showCaret;
                _blinkTimer = 0f;
            }
        }
        else
        {
            _showCaret = false;
            _blinkTimer = 0f;
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var rect = SKRect.Create(0, 0, Bounds.Width, Bounds.Height);

        _bgPaint.Color = BackgroundColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _bgPaint);

        _borderPaint.Color = IsFocused ? FocusBorderColor : BorderColor;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _borderPaint);

        var contentRect = SKRect.Create(PaddingX, 0, Math.Max(1f, Bounds.Width - (PaddingX * 2f)), Bounds.Height);
        canvas.Save();
        canvas.ClipRect(contentRect, SKClipOperation.Intersect, true);

        var metrics = _font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        var textY = ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

        if (!string.IsNullOrEmpty(_text))
        {
            _textPaint.Color = TextColor;
            canvas.DrawText(_text, PaddingX, textY, SKTextAlign.Left, _font, _textPaint);
        }
        else if (!string.IsNullOrEmpty(_placeholderText) && !IsFocused)
        {
            _textPaint.Color = PlaceholderColor;
            canvas.DrawText(_placeholderText, PaddingX, textY, SKTextAlign.Left, _font, _textPaint);
        }

        if (IsFocused && _showCaret)
        {
            var safeCaret = Math.Clamp(_caretIndex, 0, _text.Length);
            var textUpToCaret = _text[..safeCaret];
            var caretX = PaddingX + _font.MeasureText(textUpToCaret);
            var topY = (Bounds.Height - textHeight) / 2f;
            var bottomY = topY + textHeight;

            _caretPaint.Color = CaretColor;
            canvas.DrawLine(caretX, topY, caretX, bottomY, _caretPaint);
        }

        canvas.Restore();
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseDown && mouseContext.Button == MouseButton.Left)
        {
            if (IsHovered && IsEnabled)
            {
                IsFocused = true;
                ResetCaretBlink();

                var relativeClickX = mouseContext.ClientPosition.X - PaddingX;
                _caretIndex = GetCaretIndexFromX(relativeClickX);
                return true;
            }
            else
            {
                IsFocused = false;
            }
        }

        return base.OnMouse(mouseContext);
    }

    protected override bool OnKey(KeyEventContext keyContext)
    {
        if (!IsFocused || !IsEnabled) return base.OnKey(keyContext);

        if (keyContext.Type == KeyEventType.CharInput)
        {
            var ch = keyContext.Character;
            if (!char.IsControl(ch) && ch != '\0')
            {
                var safeCaret = Math.Clamp(_caretIndex, 0, _text.Length);
                _text = _text.Insert(safeCaret, ch.ToString());
                _caretIndex = safeCaret + 1;
                OnTextChanged?.Invoke(_text);
                ResetCaretBlink();
                return true;
            }
        }
        else if (keyContext.Type == KeyEventType.KeyDown)
        {
            switch (keyContext.KeyCode)
            {
                case 8:
                    if (_caretIndex > 0 && _text.Length > 0)
                    {
                        var safeCaret = Math.Clamp(_caretIndex, 1, _text.Length);
                        _text = _text.Remove(safeCaret - 1, 1);
                        _caretIndex = safeCaret - 1;
                        OnTextChanged?.Invoke(_text);
                        ResetCaretBlink();
                    }
                    return true;

                case 46:
                    if (_caretIndex < _text.Length && _text.Length > 0)
                    {
                        var safeCaret = Math.Clamp(_caretIndex, 0, _text.Length - 1);
                        _text = _text.Remove(safeCaret, 1);
                        _caretIndex = safeCaret;
                        OnTextChanged?.Invoke(_text);
                        ResetCaretBlink();
                    }
                    return true;

                case 37:
                    _caretIndex = Math.Max(0, _caretIndex - 1);
                    ResetCaretBlink();
                    return true;

                case 39:
                    _caretIndex = Math.Min(_text.Length, _caretIndex + 1);
                    ResetCaretBlink();
                    return true;

                case 36:
                    _caretIndex = 0;
                    ResetCaretBlink();
                    return true;

                case 35:
                    _caretIndex = _text.Length;
                    ResetCaretBlink();
                    return true;

                case 13:
                    OnSubmit?.Invoke(_text);
                    return true;
            }
        }

        return base.OnKey(keyContext);
    }

    private int GetCaretIndexFromX(float relativeX)
    {
        if (relativeX <= 0f || string.IsNullOrEmpty(_text)) return 0;

        var bestDistance = float.MaxValue;
        var bestIndex = 0;

        for (var i = 0; i <= _text.Length; i++)
        {
            var sub = _text[..i];
            var width = _font.MeasureText(sub);
            var dist = Math.Abs(width - relativeX);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void ResetCaretBlink()
    {
        _showCaret = true;
        _blinkTimer = 0f;
    }

    protected override void OnDispose()
    {
        _bgPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _caretPaint.Dispose();
        _font.Dispose();
        base.OnDispose();
    }
}