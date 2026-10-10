using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class TextBox : UIControlBase<TextBox>
{
    private SKTypeface? _cachedTypeface;
    private int _caretIndex;
    private float _blinkTimer;
    private bool _showCaret = true;
    private bool _colorsInitialized;

    private bool _customBgSet;
    private bool _customBorderSet;
    private bool _customFocusBorderSet;
    private bool _customTextSet;
    private bool _customPlaceholderSet;
    private bool _customCaretSet;

    private readonly SKPaint _bgPaint = new() { Style = SKPaintStyle.Fill, IsAntialias = true };
    private readonly SKPaint _borderPaint = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _placeholderPaint = new() { IsAntialias = true };
    private readonly SKPaint _caretPaint = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };

    public TextBox(float width = 200f, float height = 36f, string text = "", string placeholder = "Type here...")
    {
        Text = text;
        PlaceholderText = placeholder;
        _caretIndex = Text.Length;
        Bounds = SKRect.Create(0, 0, width, height);
        UpdateBrushes();
    }

    public new bool IsEnabled
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = true;

    public ThemeRecord Theme
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            if (!_customBgSet) BackgroundColor = value.BackgroundColor;
            if (!_customBorderSet) BorderColor = value.BorderColor;
            if (!_customFocusBorderSet) FocusBorderColor = value.ActiveColor;
            if (!_customTextSet) TextColor = value.TextColor;
            if (!_customPlaceholderSet) PlaceholderColor = value.DisabledTextColor;
            if (!_customCaretSet) CaretColor = value.ActiveColor;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme();

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
        set
        {
            if (field == value) return;
            field = value;
            _customBgSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BackgroundColor;

    public SKColor BorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customBorderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().BorderColor;

    public SKColor FocusBorderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customFocusBorderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().ActiveColor;

    public SKColor TextColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customTextSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().TextColor;

    public SKColor PlaceholderColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customPlaceholderSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().DisabledTextColor;

    public SKColor CaretColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _customCaretSet = true;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme().ActiveColor;

    public Action<string>? OnTextChanged { get; set; }
    public Action<string>? OnSubmit { get; set; }

    public event Action<string>? TextChanged;
    public event Action<string>? Submitted;

    private SKTypeface GetOrCreateTypeface()
    {
        return _cachedTypeface ??= SKTypeface.FromFamilyName(FontFamily, FontWeight, FontWidth, FontSlant);
    }

    private void InvalidateTypeface()
    {
        _cachedTypeface?.Dispose();
        _cachedTypeface = null;
    }

    private void UpdateBrushes()
    {
        _colorsInitialized = true;

        if (IsEnabled)
        {
            _bgPaint.Color = BackgroundColor;
            _borderPaint.Color = IsFocused ? FocusBorderColor : BorderColor;
            _textPaint.Color = TextColor;
            _placeholderPaint.Color = PlaceholderColor;
            _caretPaint.Color = CaretColor;
        }
        else
        {
            _bgPaint.Color = Theme.DarkerBackgroundColor;
            _borderPaint.Color = Theme.DisabledColor;
            _textPaint.Color = Theme.DisabledTextColor;
            _placeholderPaint.Color = Theme.DisabledTextColor;
            _caretPaint.Color = SKColors.Transparent;
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint.X, clientPoint.Y);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!_colorsInitialized)
            UpdateBrushes();

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

        UpdateBrushes();

        var localRect = SKRect.Create(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height);

        canvas.DrawRoundRect(localRect, CornerRadius, CornerRadius, _bgPaint);
        canvas.DrawRoundRect(localRect, CornerRadius, CornerRadius, _borderPaint);

        var contentRect = SKRect.Create(Bounds.Left + PaddingX, Bounds.Top, Math.Max(1f, Bounds.Width - (PaddingX * 2f)), Bounds.Height);
        canvas.Save();
        canvas.ClipRect(contentRect, SKClipOperation.Intersect, true);

        using var font = new SKFont(GetOrCreateTypeface(), TextSize) { Subpixel = true };
        var metrics = font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        var textY = Bounds.Top + ((Bounds.Height + textHeight) / 2f) - metrics.Descent;

        if (!string.IsNullOrEmpty(Text))
        {
            canvas.DrawText(Text, Bounds.Left + PaddingX, textY, SKTextAlign.Left, font, _textPaint);
        }
        else if (!string.IsNullOrEmpty(PlaceholderText) && !IsFocused)
        {
            canvas.DrawText(PlaceholderText, Bounds.Left + PaddingX, textY, SKTextAlign.Left, font, _placeholderPaint);
        }

        if (IsFocused && _showCaret && IsEnabled)
        {
            var safeCaret = Math.Clamp(_caretIndex, 0, Text.Length);
            var textUpToCaret = Text[..safeCaret];
            var caretX = Bounds.Left + PaddingX + font.MeasureText(textUpToCaret);
            var topY = Bounds.Top + ((Bounds.Height - textHeight) / 2f);
            var bottomY = topY + textHeight;

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
                if (!IsFocused)
                {
                    IsFocused = true;
                    UpdateBrushes();
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
                UpdateBrushes();
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
                case 8:
                    if (_caretIndex > 0 && Text.Length > 0)
                    {
                        var safeCaret = Math.Clamp(_caretIndex, 1, Text.Length);
                        Text = Text.Remove(safeCaret - 1, 1);
                        _caretIndex = safeCaret - 1;
                        ResetCaretBlink();
                    }
                    return true;

                case 46:
                    if (_caretIndex < Text.Length && Text.Length > 0)
                    {
                        var safeCaret = Math.Clamp(_caretIndex, 0, Text.Length - 1);
                        Text = Text.Remove(safeCaret, 1);
                        _caretIndex = safeCaret;
                        ResetCaretBlink();
                    }
                    return true;

                case 37:
                    if (_caretIndex > 0)
                    {
                        _caretIndex--;
                        ResetCaretBlink();
                    }
                    return true;

                case 39:
                    if (_caretIndex < Text.Length)
                    {
                        _caretIndex++;
                        ResetCaretBlink();
                    }
                    return true;

                case 36:
                    if (_caretIndex != 0)
                    {
                        _caretIndex = 0;
                        ResetCaretBlink();
                    }
                    return true;

                case 35:
                    if (_caretIndex != Text.Length)
                    {
                        _caretIndex = Text.Length;
                        ResetCaretBlink();
                    }
                    return true;

                case 13:
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
        _bgPaint.Dispose();
        _borderPaint.Dispose();
        _textPaint.Dispose();
        _placeholderPaint.Dispose();
        _caretPaint.Dispose();
        base.OnDispose();
    }

    public TextBox WithTheme(ThemeRecord theme) { Theme = theme; return this; }
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
}