using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Components;
using SkiaSharp;

namespace HandyUI.Core.Interfaces;

#region Core Interface

public interface IControl : IDisposable
{
    #region Properties

    IReadOnlyList<IControl> Children { get; }
    BaseControl? Parent { get; set; }

    SKPoint Location { get; set; }
    SKRect Bounds { get; set; }
    int ZIndex { get; set; }
    bool ParentChildPositioningEnabled { get; set; }
    bool ScissoringEnabled { get; set; }

    SKPaint AlphaPaint { get; }
    float Opacity { get; set; }

    bool IsHovered { get; }
    bool IsMouseDown { get; }
    bool IsFocused { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }

    #endregion

    #region Events

    event Action<IControl>? FocusRequested;
    event Action? Invalidated;

    #endregion

    #region Event Methods

    void Invalidate();
    void RequestFocus();
    bool ProcessMouseEvent(MouseEventContext mouseContext);
    bool ProcessKeyEvent(KeyEventContext keyContext);

    #endregion

    #region Abstract/Virtual Methods

    bool Intersects(SKPoint clientPoint);
    void Update(float deltaTime, SKPoint clientMousePosition);
    void Draw(SKCanvas canvas);

    #endregion
}

#endregion

#region Fluent APIs

public interface IControl<TSelf> : IControl where TSelf : IControl<TSelf>
{
    TSelf WithLocation(SKPoint location);
    TSelf WithLocation(float x, float y);
    TSelf WithBounds(SKRect bounds);
    TSelf WithBounds(float x, float y, float w, float h);
    TSelf WithAddToRenderer(UIRenderer renderer);
}

#endregion