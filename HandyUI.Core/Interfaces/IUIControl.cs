using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Components;
using SkiaSharp;

namespace HandyUI.Core.Interfaces;

public interface IUIControl : IDisposable
{
    IReadOnlyList<IUIControl> Children { get; }
    UIControlBase? Parent { get; set; }

    SKPoint Location { get; set; }
    SKRect Bounds { get; set; }
    int ZIndex { get; set; }
    bool InheritedPositioningEnabled { get; set; }
    bool ScissoringEnabled { get; set; }

    SKPaint AlphaPaint { get; }
    float Opacity { get; set; }

    bool IsHovered { get; }
    bool IsMouseDown { get; }
    bool IsFocused { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }

    event Action<IUIControl>? FocusRequested;
    event Action? Invalidated;

    void Invalidate();
    void RequestFocus();
    bool ProcessMouseEvent(MouseEventContext mouseContext);
    bool ProcessKeyEvent(KeyEventContext keyContext);

    bool Intersects(SKPoint clientPoint);
    void Update(float deltaTime, SKPoint clientMousePosition);
    void Draw(SKCanvas canvas);

    UIControlBase WithLocation(SKPoint location);
    UIControlBase WithLocation(float x, float y);
    UIControlBase WithBounds(SKRect bounds);
    UIControlBase WithBounds(float x, float y, float w, float h);
    UIControlBase WithAddToRenderer(UIRenderer renderer);
}