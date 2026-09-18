using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextBox : UIControlBase
{
    private SKTypeface? _cachedTypeface;
    private int _caretIndex;
    private float _blinkTimer;
    private bool _showCaret = true;

    public float Width
    {
        get => Bounds.Width;
        set
        {
            if (Math.Abs(Bounds.Width - value) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, value, Bounds.Height);
            Invalidate();
        }
    }

    public float Height
    {
        get => Bounds.Height;
        set
        {
            if (Math.Abs(Bounds.Height - value) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, Bounds.Width, value);
            Invalidate();
        }
    }

    public SKSize Size
    {
        get => new(Bounds.Width, Bounds.Height);
        set
        {
            if (Math.Abs(Bounds.Width - value.Width) < 0.001f && Math.Abs(Bounds.Height - value.Height) < 0.001f) return;
            Bounds = SKRect.Create(Location.X, Location.Y, value.Width, value.Height);
            Invalidate();
        }
    }

    public string Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            _caretIndex = Math.Clamp(_caretIndex, 0, field.Length);
            TextChanged?.Invoke(field);
            OnTextChanged?.Invoke(field);
            Invalidate();
        }
    } = string.Empty;

    public string PlaceholderText
    {
        get;
        set { if (field == value) return; field = value ?? string.Empty; Invalidate(); }
    } = "Type here...";

    public float CornerRadius
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 6f;

    public float PaddingX
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 10f;

    public float TextSize
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 13f;

    public string FontFamily
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = "Segoe UI";

    public SKFontStyleWeight FontWeight
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = SKFontStyleWeight.Normal;

    public SKFontStyleWidth FontWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = SKFontStyleWidth.Normal;

    public SKFontStyleSlant FontSlant
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTypeface();
            Invalidate();
        }
    } = SKFontStyleSlant.Upright;

    public SKColor BackgroundColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = SKColor.Parse("#1E1E2E");

    public SKColor BorderColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = SKColor.Parse("#45475A");

    public SKColor FocusBorderColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = SKColor.Parse("#CBA6F7");

    public SKColor TextColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = SKColor.Parse("#CDD6F4");

    public SKColor PlaceholderColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = SKColor.Parse("#6C7086");

    public SKColor CaretColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = SKColor.Parse("#CBA6F7");

    public Action<string>? OnTextChanged { get; set; }
    public Action<string>? OnSubmit { get; set; }

    public event Action<string>? TextChanged;
    public event Action<string>? Submitted;

    public TextBox(float width = 200f, float height = 36f, string text = "", string placeholder = "Type here...")
    {
        Text = text;
        PlaceholderText = placeholder;
        _caretIndex = Text.Length;
        Bounds = SKRect.Create(0, 0, width, height);
    }

    private SKTypeface GetOrCreateTypeface() => _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);

    private void InvalidateTypeface()
    {
        _cachedTypeface?.Dispose();
        _cachedTypeface = null;
    }

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (IsFocused)
        {
            _blinkTimer += deltaTime;
            if (_blinkTimer >= 0.5f)
            {
                _showCaret = !_showCaret;
                _blinkTimer = 0f;
                Invalidate();
            }
        }
        else if (_showCaret)
        {
            _showCaret = false;
            _blinkTimer = 0f;
            Invalidate();
        }
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        var localRect = SKRect.Create(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height);

        using var bgPaint = new SKPaint { Color = BackgroundColor, Style = SKPaintStyle.Fill, IsAntialias = true };
        canvas.DrawRoundRect(localRect, CornerRadius, CornerRadius, bgPaint);

        using var borderPaint = new SKPaint
        {
            Color = IsFocused ? FocusBorderColor : BorderColor,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        canvas.DrawRoundRect(localRect, CornerRadius, CornerRadius, borderPaint);

        var contentRect = SKRect.Create(Bounds.Left + PaddingX, Bounds.Top, Math.Max(1f, Bounds.Width - (PaddingX * 2f)), Bounds.Height);
        canvas.Save();
        canvas.ClipRect(contentRect, SKClipOperation.Intersect, true);

        using var font = new SKFont(GetOrCreateTypeface(), TextSize) { Subpixel = true };
        var metrics = font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        var textY = Bounds.Top + ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

        if (!string.IsNullOrEmpty(Text))
        {
            using var textPaint = new SKPaint { Color = TextColor, IsAntialias = true };
            canvas.DrawText(Text, Bounds.Left + PaddingX, textY, SKTextAlign.Left, font, textPaint);
        }
        else if (!string.IsNullOrEmpty(PlaceholderText) && !IsFocused)
        {
            using var placeholderPaint = new SKPaint { Color = PlaceholderColor, IsAntialias = true };
            canvas.DrawText(PlaceholderText, Bounds.Left + PaddingX, textY, SKTextAlign.Left, font, placeholderPaint);
        }

        if (IsFocused && _showCaret)
        {
            var safeCaret = Math.Clamp(_caretIndex, 0, Text.Length);
            var textUpToCaret = Text[..safeCaret];
            var caretX = Bounds.Left + PaddingX + font.MeasureText(textUpToCaret);
            var topY = Bounds.Top + ((Bounds.Height - textHeight) / 2f);
            var bottomY = topY + textHeight;

            using var caretPaint = new SKPaint { Color = CaretColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
            canvas.DrawLine(caretX, topY, caretX, bottomY, caretPaint);
        }

        canvas.Restore();
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseDown && mouseContext.Button == MouseButton.Left)
        {
            if (IsHovered && IsEnabled)
            {
                if (!IsFocused)
                {
                    IsFocused = true;
                    Invalidate();
                }
                ResetCaretBlink();

                var relativeClickX = mouseContext.ClientPosition.X - Bounds.Left - PaddingX;
                var newIndex = GetCaretIndexFromX(relativeClickX);
                if (_caretIndex != newIndex)
                {
                    _caretIndex = newIndex;
                    Invalidate();
                }
                return true;
            }

            if (IsFocused)
            {
                IsFocused = false;
                Invalidate();
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
                var safeCaret = Math.Clamp(_caretIndex, 0, Text.Length);
                Text = Text.Insert(safeCaret, ch.ToString());
                _caretIndex = safeCaret + 1;
                ResetCaretBlink();
                return true;
            }
        }
        else if (keyContext.Type == KeyEventType.KeyDown)
        {
            switch (keyContext.KeyCode)
            {
                case 8: // Backspace
                    if (_caretIndex > 0 && Text.Length > 0)
                    {
                        var safeCaret = Math.Clamp(_caretIndex, 1, Text.Length);
                        Text = Text.Remove(safeCaret - 1, 1);
                        _caretIndex = safeCaret - 1;
                        ResetCaretBlink();
                    }
                    return true;

                case 46: // Delete
                    if (_caretIndex < Text.Length && Text.Length > 0)
                    {
                        var safeCaret = Math.Clamp(_caretIndex, 0, Text.Length - 1);
                        Text = Text.Remove(safeCaret, 1);
                        _caretIndex = safeCaret;
                        ResetCaretBlink();
                    }
                    return true;

                case 37: // Left
                    if (_caretIndex > 0)
                    {
                        _caretIndex--;
                        ResetCaretBlink();
                    }
                    return true;

                case 39: // Right
                    if (_caretIndex < Text.Length)
                    {
                        _caretIndex++;
                        ResetCaretBlink();
                    }
                    return true;

                case 36: // Home
                    if (_caretIndex != 0)
                    {
                        _caretIndex = 0;
                        ResetCaretBlink();
                    }
                    return true;

                case 35: // End
                    if (_caretIndex != Text.Length)
                    {
                        _caretIndex = Text.Length;
                        ResetCaretBlink();
                    }
                    return true;

                case 13: // Enter
                    Submitted?.Invoke(Text);
                    OnSubmit?.Invoke(Text);
                    return true;
            }
        }

        return base.OnKey(keyContext);
    }

    private int GetCaretIndexFromX(float relativeX)
    {
        if (relativeX <= 0f || string.IsNullOrEmpty(Text)) return 0;

        using var font = new SKFont(GetOrCreateTypeface(), TextSize);
        var bestDistance = float.MaxValue;
        var bestIndex = 0;

        for (var i = 0; i <= Text.Length; i++)
        {
            var sub = Text[..i];
            var width = font.MeasureText(sub);
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
        Invalidate();
    }

    protected override void OnDispose()
    {
        InvalidateTypeface();
        base.OnDispose();
    }

    public TextBox WithWidth(float width) { Width = width; return this; }
    public TextBox WithHeight(float height) { Height = height; return this; }
    public TextBox WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public TextBox WithText(string text) { Text = text; return this; }
    public TextBox WithPlaceholder(string placeholder) { PlaceholderText = placeholder; return this; }
    public TextBox WithCornerRadius(float radius) { CornerRadius = radius; return this; }
    public TextBox WithPaddingX(float paddingX) { PaddingX = paddingX; return this; }
    public TextBox WithTextSize(float size) { TextSize = size; return this; }
    public TextBox WithFont(string family, float size = 13f, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
    {
        FontFamily = family;
        TextSize = size;
        FontWeight = weight;
        FontSlant = slant;
        return this;
    }
    public TextBox WithColors(SKColor bg, SKColor border, SKColor focusBorder, SKColor text, SKColor placeholder, SKColor caret)
    {
        BackgroundColor = bg;
        BorderColor = border;
        FocusBorderColor = focusBorder;
        TextColor = text;
        PlaceholderColor = placeholder;
        CaretColor = caret;
        return this;
    }
    public TextBox WithOnTextChanged(Action<string> onTextChanged) { TextChanged += onTextChanged; OnTextChanged = onTextChanged; return this; }
    public TextBox WithOnSubmit(Action<string> onSubmit) { Submitted += onSubmit; OnSubmit = onSubmit; return this; }
    public TextBox WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public TextBox WithParent(UIControlBase? parent) { Parent = parent; return this; }
}