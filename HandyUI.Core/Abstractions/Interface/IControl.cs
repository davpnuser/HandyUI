using HandyUI.Core.Data;
using SkiaSharp;

namespace HandyUI.Core.Abstractions.Interface;

public interface IControl : IDisposable
{
    #region Properties

    string? Name { get; set; }
    IControl? Parent { get; set; }
    IReadOnlyList<IControl> Children { get; }
    SKPoint Location { get; set; }
    SKRect Bounds { get; set; }
    int ZIndex { get; set; }

    bool Enabled { get; set; }
    bool Visible { get; set; }
    bool ClipChildren { get; set; }
    bool Focusable { get; set; }

    bool IsDisposed { get; }
    bool IsHovered { get; }
    bool IsFocused { get; internal set; }
    bool IsMouseDown { get; }

    float Opacity { get; set; }
    SKPaint AlphaPaint { get; }

    #endregion

    #region Events

    event Action? WhenInvalidated;
    event Action<IControl?, bool>? WhenFocusRequested;

    #endregion

    #region Overrideables

    abstract bool DoesIntersect(SKPoint point);
    abstract void OnDraw(SKCanvas canvas);
    abstract void OnUpdate(UpdateContext updateContext);

    #endregion

    #region Methods

    void Invalidate();
    void RequestFocus();

    bool ProcessMouseEvent(MouseEventContext eventContext);
    bool ProcessKeyEvent(KeyEventContext eventContext);

    SKPoint GetGlobalLocation();
    SKPoint GetLocalPosition(SKPoint point);

    #endregion
}