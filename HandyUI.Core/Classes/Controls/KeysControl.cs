using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using SkiaSharp;
using System.Text;

namespace HandyUI.Core.Classes.Controls;

public class KeysControl : UIControlBase
{
    private readonly SKPaint _paint;
    private readonly SKFont _font;
    private readonly StringBuilder _text = new();

    public string Text => _text.ToString();
    public bool IsMultiline { get; set; } = true;

    public event Action<string>? Submitted;

    public KeysControl()
    {
        _paint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _font = new SKFont(SKTypeface.Default, 14);
    }

    public override bool Intersects(SKPoint clientPoint) => true;

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!IsFocused)
            RequestFocus();
    }

    public override void Draw(SKCanvas canvas)
    {
        var lines = _text.ToString().Split('\n');
        float y = 0;

        foreach (var line in lines)
        {
            canvas.DrawText(line, 0, y, SKTextAlign.Left, _font, _paint);
            y += _font.Spacing;
        }
    }

    protected override bool OnKey(KeyEventContext keyContext)
    {
        if (keyContext.Type == KeyEventType.KeyDown)
        {
            switch (keyContext.KeyCode)
            {
                case 8:
                    if (_text.Length > 0)
                    {
                        _text.Length -= 1;
                        return true;
                    }
                    break;

                case 13:
                    if (IsMultiline)
                    {
                        _text.Append('\n');
                    }
                    else
                    {
                        Submitted?.Invoke(Text);
                    }
                    return true;

                case 9:
                    _text.Append("    ");
                    return true;

                case 27:
                    IsFocused = false;
                    return true;
            }
        }

        else if (keyContext.Type == KeyEventType.CharInput)
        {
            var c = keyContext.Character;

            if (!char.IsControl(c))
            {
                _text.Append(c);
                return true;
            }
        }

        return false;
    }

    protected override void OnDispose()
    {
        _font.Dispose();
        _paint.Dispose();
    }
}