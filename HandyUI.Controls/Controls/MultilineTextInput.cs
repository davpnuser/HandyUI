using HandyUI.Core.Base;
using HandyUI.Core.Components;
using HandyUI.Core.Data;
using HandyUI.Core.Enums;
using HandyUI.Core.Theming;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class MultilineTextInput : BaseControl<MultilineTextInput>
{
    #region Properties

    public float Width
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public float Height
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    }

    public string Text
    {
        get;
        set
        {
            field = value ?? string.Empty;
            if (_cursorIndex > field.Length) _cursorIndex = field.Length;
            if (_selectionAnchor > field.Length) _selectionAnchor = field.Length;
            ResizeToContent();
            Invalidate();
        }
    } = string.Empty;

    public string Placeholder
    {
        get;
        set
        {
            if (field == value) return;
            field = value ?? string.Empty;
            ResizeToContent();
            Invalidate();
        }
    } = string.Empty;

    public float FontSize
    {
        get;
        set
        {
            var v = Math.Max(1f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            _font.Size = v;
            ResizeToContent();
            Invalidate();
        }
    } = 14f;

    public float LineSpacing
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 4f;

    public float CornerRadius
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            Invalidate();
        }
    } = 4f;

    public float Padding
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 12f;

    #endregion

    #region Internal Fields

    private readonly SKFont _font;
    private readonly SKPaint _bgPaint = new() { IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _placeholderPaint = new() { IsAntialias = true };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _cursorPaint = new() { IsAntialias = true };
    private readonly SKPaint _selectionPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    private int _cursorIndex;
    private int _selectionAnchor;
    private bool _isMouseSelecting;

    private float _focusAmount;
    private float _cursorBlink;
    private bool _lastFocused;
    private TweenHandle _focusTween;
    private TweenHandle _cursorTween;

    #endregion

    #region Events

    public event Action<string>? WhenTextChanged;

    #endregion

    #region Constructor

    public MultilineTextInput()
    {
        _font = new SKFont(FontManager.DefaultFont, FontSize)
        {
            Edging = SKFontEdging.Antialias,
            Subpixel = true
        };
        ResizeToContent();
    }

    #endregion

    #region Overrides

    public override bool DoesIntersect(SKPoint? clientPoint)
    {
        return clientPoint is { } p && Bounds.Contains(p);
    }

    public override void OnUpdate(float deltaTime, SKPoint? clientMousePosition)
    {
        if (_lastFocused != IsFocused)
        {
            _lastFocused = IsFocused;
            StartFocusAnimation(IsFocused ? 1f : 0f);
            if (IsFocused) StartCursorBlink();
            else StopCursorBlink();
        }
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var rect = new SKRect(0, 0, w, h);

        _bgPaint.Color = IsEnabled
            ? ThemeManager.Instance.SurfaceSecondary
            : ThemeManager.Instance.DisabledSurface;
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _bgPaint);

        if (_focusAmount > 0.001f)
        {
            _borderPaint.Color = ThemeManager.Instance.Accent.WithAlpha((byte)(255 * _focusAmount));
            _borderPaint.StrokeWidth = 1f + _focusAmount;
            var borderRect = new SKRect(
                _borderPaint.StrokeWidth * 0.5f,
                _borderPaint.StrokeWidth * 0.5f,
                w - (_borderPaint.StrokeWidth * 0.5f),
                h - (_borderPaint.StrokeWidth * 0.5f));
            canvas.DrawRoundRect(borderRect, CornerRadius, CornerRadius, _borderPaint);
        }

        _textPaint.Color = IsEnabled
            ? ThemeManager.Instance.TextPrimary
            : ThemeManager.Instance.TextDisabled;

        var fm = _font.Metrics;
        var lineHeight = -fm.Ascent + fm.Descent + LineSpacing;
        var firstBaseline = Padding - fm.Ascent;

        canvas.Save();
        canvas.ClipRect(new SKRect(Padding, Padding, w - Padding, h - Padding));

        if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder))
        {
            _placeholderPaint.Color = ThemeManager.Instance.TextTertiary;
            canvas.DrawText(Placeholder, Padding, firstBaseline, SKTextAlign.Left, _font, _placeholderPaint);
        }
        else
        {
            var lines = Text.Split('\n');

            if (HasSelection)
                DrawSelection(canvas, lines, firstBaseline, lineHeight);

            var y = firstBaseline;
            for (var i = 0; i < lines.Length; i++)
            {
                canvas.DrawText(lines[i], Padding, y, SKTextAlign.Left, _font, _textPaint);
                y += lineHeight;
            }

            if (IsFocused && _cursorBlink > 0.5f)
            {
                var (line, col) = GetLineCol(_cursorIndex, lines);
                var safeLine = Math.Min(line, lines.Length - 1);
                var beforeText = lines[safeLine][..Math.Min(col, lines[safeLine].Length)];
                var cursorX = Padding + _font.MeasureText(beforeText);
                var cursorY = firstBaseline + (safeLine * lineHeight);

                _cursorPaint.Color = ThemeManager.Instance.Accent;
                _cursorPaint.StrokeWidth = 1.5f;
                canvas.DrawLine(cursorX, cursorY - FontSize, cursorX, cursorY, _cursorPaint);
            }
        }

        canvas.Restore();
    }

    protected override bool OnMouse(MouseEventContext ctx)
    {
        if (ctx.Type == MouseEventType.MouseDown && IsHovered && ctx.ClientPosition is { } p)
        {
            RequestFocus();
            _isMouseSelecting = true;
            _cursorIndex = IndexFromPoint(p);
            _selectionAnchor = _cursorIndex;
            return true;
        }

        if (ctx.Type == MouseEventType.Move && _isMouseSelecting && ctx.ClientPosition is { } mp)
        {
            _cursorIndex = IndexFromPoint(mp);
            Invalidate();
            return true;
        }

        if (ctx.Type == MouseEventType.MouseUp && _isMouseSelecting)
        {
            _isMouseSelecting = false;
            return true;
        }

        if (ctx.Type == MouseEventType.Click && IsHovered)
        {
            RequestFocus();
            return true;
        }

        return false;
    }

    protected override bool OnKey(KeyEventContext ctx)
    {
        if (ctx.Type == KeyEventType.CharInput && ctx.Character != '\0' && !char.IsControl(ctx.Character))
        {
            if (HasSelection) DeleteSelection();
            Text = Text.Insert(_cursorIndex, ctx.Character.ToString());
            _cursorIndex++;
            _selectionAnchor = _cursorIndex;
            WhenTextChanged?.Invoke(Text);
            return true;
        }

        if (ctx.Type == KeyEventType.KeyDown)
        {
            switch (ctx.KeyCode)
            {
                case 13:
                    if (HasSelection) DeleteSelection();
                    Text = Text.Insert(_cursorIndex, "\n");
                    _cursorIndex++;
                    _selectionAnchor = _cursorIndex;
                    WhenTextChanged?.Invoke(Text);
                    return true;
                case 8:
                    HandleBackspace(ctx);
                    return true;
                case 46:
                    HandleDelete(ctx);
                    return true;
                case 37:
                    HandleLeft(ctx);
                    return true;
                case 39:
                    HandleRight(ctx);
                    return true;
                case 38:
                    HandleUp(ctx);
                    return true;
                case 40:
                    HandleDown(ctx);
                    return true;
                case 36:
                    HandleHome(ctx);
                    return true;
                case 35:
                    HandleEnd(ctx);
                    return true;
                case 65:
                    if (ctx.IsControlPressed)
                    {
                        _selectionAnchor = 0;
                        _cursorIndex = Text.Length;
                        return true;
                    }
                    break;
            }
        }

        return false;
    }

    #endregion

    #region Internal Helpers

    private bool HasSelection => _selectionAnchor != _cursorIndex;
    private int SelectionStart => Math.Min(_selectionAnchor, _cursorIndex);
    private int SelectionEnd => Math.Max(_selectionAnchor, _cursorIndex);

    private void DrawSelection(SKCanvas canvas, string[] lines, float firstBaseline, float lineHeight)
    {
        _selectionPaint.Color = ThemeManager.Instance.Accent.WithAlpha(90);
        var selStart = SelectionStart;
        var selEnd = SelectionEnd;

        var offset = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            var lineStart = offset;
            var lineEnd = offset + lines[i].Length;

            if (selStart > lineEnd) { offset = lineEnd + 1; continue; }
            if (selEnd <= lineStart) break;

            var startInLine = Math.Max(0, selStart - lineStart);
            var endInLine = Math.Min(lines[i].Length, selEnd - lineStart);

            var startX = Padding + _font.MeasureText(lines[i].AsSpan(0, startInLine));
            var endX = Padding + _font.MeasureText(lines[i].AsSpan(0, endInLine));

            if (endInLine == 0 && selStart < lineStart && selEnd > lineStart)
                endX = startX + 4f;

            var y = firstBaseline + (i * lineHeight);
            canvas.DrawRect(startX, y - FontSize, endX - startX, FontSize + 4f, _selectionPaint);

            offset = lineEnd + 1;
        }
    }

    private void DeleteSelection()
    {
        if (!HasSelection) return;
        var start = SelectionStart;
        var end = SelectionEnd;
        Text = Text.Remove(start, end - start);
        _cursorIndex = start;
        _selectionAnchor = start;
    }

    private void HandleBackspace(KeyEventContext ctx)
    {
        if (HasSelection)
        {
            DeleteSelection();
        }
        else if (ctx.IsControlPressed)
        {
            var prev = PrevWordBoundary(_cursorIndex);
            if (prev < _cursorIndex)
            {
                Text = Text.Remove(prev, _cursorIndex - prev);
                _cursorIndex = prev;
                _selectionAnchor = prev;
            }
        }
        else if (_cursorIndex > 0)
        {
            _cursorIndex--;
            Text = Text.Remove(_cursorIndex, 1);
            _selectionAnchor = _cursorIndex;
        }
        WhenTextChanged?.Invoke(Text);
    }

    private void HandleDelete(KeyEventContext ctx)
    {
        if (HasSelection)
        {
            DeleteSelection();
        }
        else if (ctx.IsControlPressed)
        {
            var next = NextWordBoundary(_cursorIndex);
            if (next > _cursorIndex)
                Text = Text.Remove(_cursorIndex, next - _cursorIndex);
        }
        else if (_cursorIndex < Text.Length)
        {
            Text = Text.Remove(_cursorIndex, 1);
        }
        _selectionAnchor = _cursorIndex;
        WhenTextChanged?.Invoke(Text);
    }

    private void HandleLeft(KeyEventContext ctx)
    {
        var newPos = ctx.IsControlPressed
            ? PrevWordBoundary(_cursorIndex)
            : Math.Max(0, _cursorIndex - 1);

        if (ctx.IsShiftPressed)
        {
            _cursorIndex = newPos;
        }
        else { _cursorIndex = HasSelection ? SelectionStart : newPos; _selectionAnchor = _cursorIndex; }
        Invalidate();
    }

    private void HandleRight(KeyEventContext ctx)
    {
        var newPos = ctx.IsControlPressed
            ? NextWordBoundary(_cursorIndex)
            : Math.Min(Text.Length, _cursorIndex + 1);

        if (ctx.IsShiftPressed)
        {
            _cursorIndex = newPos;
        }
        else { _cursorIndex = HasSelection ? SelectionEnd : newPos; _selectionAnchor = _cursorIndex; }
        Invalidate();
    }

    private void HandleUp(KeyEventContext ctx)
    {
        var lines = Text.Split('\n');
        var (line, col) = GetLineCol(_cursorIndex, lines);
        _cursorIndex = line > 0 ? LineColToIndex(Math.Max(0, line - 1), col, lines) : 0;

        if (!ctx.IsShiftPressed) _selectionAnchor = _cursorIndex;
        Invalidate();
    }

    private void HandleDown(KeyEventContext ctx)
    {
        var lines = Text.Split('\n');
        var (line, col) = GetLineCol(_cursorIndex, lines);
        _cursorIndex = line < lines.Length - 1 ? LineColToIndex(line + 1, col, lines) : Text.Length;

        if (!ctx.IsShiftPressed) _selectionAnchor = _cursorIndex;
        Invalidate();
    }

    private void HandleHome(KeyEventContext ctx)
    {
        var lines = Text.Split('\n');
        var target = ctx.IsControlPressed ? 0 : LineColToIndex(GetLineCol(_cursorIndex, lines).Line, 0, lines);
        if (ctx.IsShiftPressed)
        {
            _cursorIndex = target;
        }
        else { _cursorIndex = target; _selectionAnchor = target; }
        Invalidate();
    }

    private void HandleEnd(KeyEventContext ctx)
    {
        var lines = Text.Split('\n');
        var (line, _) = GetLineCol(_cursorIndex, lines);
        var target = ctx.IsControlPressed
            ? Text.Length
            : LineColToIndex(line, lines[line].Length, lines);
        if (ctx.IsShiftPressed)
        {
            _cursorIndex = target;
        }
        else { _cursorIndex = target; _selectionAnchor = target; }
        Invalidate();
    }

    private (int Line, int Col) GetLineCol(int index, string[] lines)
    {
        var offset = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            if (offset + lines[i].Length >= index)
                return (i, index - offset);
            offset += lines[i].Length + 1;
        }
        return (lines.Length - 1, lines[^1].Length);
    }

    private int LineColToIndex(int line, int col, string[] lines)
    {
        if (line < 0) line = 0;
        if (line >= lines.Length) line = lines.Length - 1;
        var c = Math.Min(col, lines[line].Length);
        var offset = 0;
        for (var i = 0; i < line; i++) offset += lines[i].Length + 1;
        return offset + c;
    }

    private int PrevWordBoundary(int from)
    {
        var i = from;
        while (i > 0 && !char.IsLetterOrDigit(Text[i - 1]) && Text[i - 1] != '\n') i--;
        while (i > 0 && char.IsLetterOrDigit(Text[i - 1])) i--;
        return i;
    }

    private int NextWordBoundary(int from)
    {
        var i = from;
        while (i < Text.Length && !char.IsLetterOrDigit(Text[i]) && Text[i] != '\n') i++;
        while (i < Text.Length && char.IsLetterOrDigit(Text[i])) i++;
        return i;
    }

    private int IndexFromPoint(SKPoint p)
    {
        var fm = _font.Metrics;
        var lineHeight = -fm.Ascent + fm.Descent + LineSpacing;
        var localX = p.X - Padding;
        var localY = p.Y - Padding;

        var lines = Text.Split('\n');
        if (lines.Length == 0) return 0;

        var lineIndex = localY <= 0 ? 0 : (int)(localY / lineHeight);
        if (lineIndex >= lines.Length) lineIndex = lines.Length - 1;

        var line = lines[lineIndex];
        var col = ColumnFromX(localX, line);
        return LineColToIndex(lineIndex, col, lines);
    }

    private int ColumnFromX(float x, string line)
    {
        if (x <= 0) return 0;
        var prevW = 0f;
        for (var i = 0; i < line.Length; i++)
        {
            var w = _font.MeasureText(line.AsSpan(0, i + 1));
            if (x <= w)
                return (x - prevW) < (w - x) ? i : i + 1;
            prevW = w;
        }
        return line.Length;
    }

    private void ResizeToContent()
    {
        var fm = _font.Metrics;
        var lineHeight = -fm.Ascent + fm.Descent + LineSpacing;
        var sample = !string.IsNullOrEmpty(Text) ? Text : Placeholder;

        float contentW, contentH;
        if (string.IsNullOrEmpty(sample))
        {
            contentW = Padding * 2f;
            contentH = lineHeight + (Padding * 2f);
        }
        else
        {
            var lines = sample.Split('\n');
            var maxW = 0f;
            foreach (var line in lines)
            {
                var lw = _font.MeasureText(line);
                if (lw > maxW) maxW = lw;
            }
            contentW = maxW + (Padding * 2f);
            contentH = (lines.Length * lineHeight) + (Padding * 2f);
        }

        var w = Width > 0f ? Width : contentW;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);
    }

    private void StartFocusAnimation(float target)
    {
        if (Tweener.Current is not { } tweener) return;
        var current = _focusAmount;
        _focusTween.Cancel();
        _focusAmount = current;
        if (Math.Abs(current - target) < 0.001f) return;
        _focusTween = tweener.To(current, target, 0.15f,
            v => { _focusAmount = v; Invalidate(); }, EaseType.SineOut);
    }

    private void StartCursorBlink()
    {
        if (Tweener.Current is not { } tweener) return;
        _cursorTween.Cancel();
        _cursorBlink = 1f;
        _cursorTween = tweener.To(1f, 0f, 0.5f,
            v => { _cursorBlink = v; Invalidate(); },
            EaseType.Linear,
            RestartCursorBlink);
    }

    private void RestartCursorBlink()
    {
        if (!IsFocused) return;
        StartCursorBlink();
    }

    private void StopCursorBlink()
    {
        _cursorTween.Cancel();
        _cursorBlink = 0f;
        Invalidate();
    }

    #endregion

    #region Fluent APIs

    public MultilineTextInput WithWidth(float width) { Width = width; return Self; }
    public MultilineTextInput WithHeight(float height) { Height = height; return Self; }
    public MultilineTextInput WithText(string text) { Text = text; return Self; }
    public MultilineTextInput WithPlaceholder(string placeholder) { Placeholder = placeholder; return Self; }
    public MultilineTextInput WithFontSize(float size) { FontSize = size; return Self; }
    public MultilineTextInput WithLineSpacing(float spacing) { LineSpacing = spacing; return Self; }
    public MultilineTextInput WithCornerRadius(float radius) { CornerRadius = radius; return Self; }
    public MultilineTextInput WithPadding(float padding) { Padding = padding; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _focusTween.Cancel();
        _cursorTween.Cancel();
        _font.Dispose();
        _bgPaint.Dispose();
        _textPaint.Dispose();
        _placeholderPaint.Dispose();
        _borderPaint.Dispose();
        _cursorPaint.Dispose();
        _selectionPaint.Dispose();
    }

    #endregion
}