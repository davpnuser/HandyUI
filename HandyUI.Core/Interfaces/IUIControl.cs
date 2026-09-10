using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Interfaces;

public interface IUIControl : IDisposable
{
    SKPoint Location { get; set; }
    SKRect Bounds { get; set; }
    int ZIndex { get; set; }
    bool RetainedModePositioning { get; set; }

    bool IsHovered { get; }
    bool IsMouseDown { get; }
    bool IsFocused { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }

    event Action<IUIControl>? FocusRequested;

    void RequestFocus();
    bool ProcessMouseEvent(MouseEventContext mouseContext);
    bool ProcessKeyEvent(KeyEventContext keyContext);

    bool Intersects(SKPoint clientPoint);
    void Update(float deltaTime, SKPoint clientMousePosition);
    void Draw(SKCanvas canvas);
}