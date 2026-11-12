using HandyUI.Controls.Enums;
using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class Frame : BaseControl<Frame>
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
            WhenSizeChanged?.Invoke(Bounds.Width, Bounds.Height);
            Invalidate();
        }
    } = 100f;

    public float Height
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            WhenSizeChanged?.Invoke(Bounds.Width, Bounds.Height);
            Invalidate();
        }
    } = 100f;

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

    public int Elevation
    {
        get;
        set
        {
            var v = Math.Max(0, value);
            if (field == v) return;
            field = v;
            Invalidate();
        }
    } = 1;

    public bool HasBorder
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    public float BorderWidth
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            Invalidate();
        }
    } = 1f;

    public BorderDirection BorderDirection
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = BorderDirection.Inside;

    public bool HasShadow
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    public bool HasBackground
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = true;

    #endregion

    #region Internal Fields

    private readonly SKPaint _bgPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _shadowPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private bool _lastHovered;

    #endregion

    #region Events

    public event Action? WhenMouseDown;
    public event Action? WhenMouseUp;
    public event Action? WhenClicked;
    public event Action? WhenHoverEntered;
    public event Action? WhenHoverExited;
    public event Action<float, float>? WhenSizeChanged;

    #endregion

    #region Constructor

    public Frame()
    {
        ResizeToContent();
    }

    #endregion

    #region Overrides

    public override bool DoesIntersect(SKPoint point)
    {
        return Bounds.Contains(point);
    }

    public override void OnUpdate(UpdateContext updateContext)
    {
        if (_lastHovered == IsHovered) return;
        _lastHovered = IsHovered;
        if (IsHovered) WhenHoverEntered?.Invoke();
        else WhenHoverExited?.Invoke();
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var rect = new SKRect(0, 0, w, h);

        if (HasShadow && Elevation > 0)
        {
            _shadowPaint.Color = ThemeManager.Instance.Shadow;
            var shadowRect = new SKRect(0, Elevation * 0.5f, w, h + (Elevation * 0.5f));
            canvas.DrawRoundRect(shadowRect, CornerRadius, CornerRadius, _shadowPaint);
        }

        if (HasBackground)
        {
            _bgPaint.Color = Enabled
                ? ThemeManager.Instance.SurfacePrimary
                : ThemeManager.Instance.DisabledSurface;
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, _bgPaint);
        }

        if (HasBorder)
        {
            _borderPaint.Color = ThemeManager.Instance.Border;
            _borderPaint.StrokeWidth = BorderWidth;

            var inset = BorderDirection == BorderDirection.Outside
                ? -BorderWidth * 0.5f
                : BorderWidth * 0.5f;

            var borderRect = new SKRect(
                rect.Left + inset, rect.Top + inset,
                rect.Right - inset, rect.Bottom - inset);

            canvas.DrawRoundRect(borderRect, CornerRadius, CornerRadius, _borderPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (!IsHovered) return false;

        if (eventContext.Type == MouseEventType.MouseDownEvent)
            WhenMouseDown?.Invoke();

        if (eventContext.Type == MouseEventType.MouseUpEvent)
            WhenMouseUp?.Invoke();

        if (eventContext.Type == MouseEventType.ClickEvent)
            WhenClicked?.Invoke();

        return true;
    }

    protected override void OnChildAdded(IControl child)
    {
        Invalidate();
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        Bounds = new SKRect(0, 0, Width, Height);
    }

    #endregion

    #region Fluent APIs

    public Frame WithSize(float width, float height) { Width = width; Height = height; return Self; }
    public Frame WithSize(SKSize size) { Width = size.Width; Height = size.Height; return Self; }
    public Frame WithWidth(float width) { Width = width; return Self; }
    public Frame WithHeight(float height) { Height = height; return Self; }
    public Frame WithCornerRadius(float radius) { CornerRadius = radius; return Self; }
    public Frame WithElevation(int elevation) { Elevation = elevation; return Self; }
    public Frame WithHasBorder(bool hasBorder) { HasBorder = hasBorder; return Self; }
    public Frame WithHasShadow(bool hasShadow) { HasShadow = hasShadow; return Self; }
    public Frame WithHasBackground(bool hasBackground) { HasBackground = hasBackground; return Self; }
    public Frame WithBorderWidth(float width) { BorderWidth = width; return Self; }
    public Frame WithBorderDirection(BorderDirection dir) { BorderDirection = dir; return Self; }
    public Frame WithWhenMouseDown(Action handler) { WhenMouseDown += handler; return Self; }
    public Frame WithWhenMouseUp(Action handler) { WhenMouseUp += handler; return Self; }
    public Frame WithWhenClicked(Action handler) { WhenClicked += handler; return Self; }
    public Frame WithWhenHoverEntered(Action handler) { WhenHoverEntered += handler; return Self; }
    public Frame WithWhenHoverExited(Action handler) { WhenHoverExited += handler; return Self; }
    public Frame WithWhenSizeChanged(Action<float, float> handler) { WhenSizeChanged += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        _bgPaint.Dispose();
        _borderPaint.Dispose();
        _shadowPaint.Dispose();
    }

    #endregion
}