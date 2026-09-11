using HandyUI.Core.Classes.Records;
using SkiaSharp;

namespace HandyUI.Core.Interfaces;

public interface IUIControl : IDisposable
{
    IUIControl? Parent { get; set; }
    IReadOnlyList<IUIControl> Children { get; }

    SKPoint Location { get; set; }
    SKRect Bounds { get; set; }
    float Width { get; set; }
    float Height { get; set; }

    int ZIndex { get; set; }
    bool RetainedModePositioning { get; set; }
    bool ClipChildren { get; set; }

    bool IsHovered { get; }
    bool IsMouseDown { get; }
    bool IsFocused { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }

    event Action<IUIControl>? FocusRequested;
    event Action<IUIControl>? ChildAdded;
    event Action<IUIControl>? ChildRemoved;

    void AddChild(IUIControl child);
    void RemoveChild(IUIControl child);

    void RequestFocus();
    bool ProcessMouseEvent(MouseEventContext mouseContext);
    bool ProcessKeyEvent(KeyEventContext keyContext);

    IUIControl? HitTest(SKPoint localPoint);
    bool Intersects(SKPoint clientPoint);
    void Update(float deltaTime, SKPoint clientMousePosition);
    void Draw(SKCanvas canvas);
}