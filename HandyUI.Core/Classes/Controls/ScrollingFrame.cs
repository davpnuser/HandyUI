using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using HandyUI.Core.Interfaces;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class ScrollingFrame : UIControlBase
{
    private bool _isRedirecting;

    private SKSize _canvasSize = new(500, 1000);
    private SKPoint _scrollOffset = SKPoint.Empty;
    private bool _isDraggingVBar;
    private bool _isDraggingHBar;
    private SKPoint _dragStartMousePos;
    private SKPoint _dragStartScrollOffset;

    private bool _isShiftPressed;

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
            Bounds = SKRect.Create(Location.X, Location.Y, value.Width, value.Height);
            Invalidate();
        }
    }

    public bool AutoCanvasSize
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = true;

    public SKSize CanvasSize
    {
        get => _canvasSize;
        set { if (_canvasSize == value) return; _canvasSize = value; Invalidate(); }
    }

    public SKPoint ScrollOffset
    {
        get => _scrollOffset;
        set { if (_scrollOffset == value) return; _scrollOffset = value; Invalidate(); }
    }

    public SKColor BackgroundColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.PanelBackground;

    public SKColor TrackColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ScrollTrack;

    public SKColor ThumbColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ScrollThumb;

    public SKColor ThumbHoverColor
    {
        get;
        set { if (field == value) return; field = value; Invalidate(); }
    } = VS2017Theme.ScrollThumbHover;

    public float ScrollBarWidth
    {
        get;
        set { if (Math.Abs(field - value) < 0.001f) return; field = value; Invalidate(); }
    } = 12.0f;

    public bool IsVThumbHovered
    {
        get;
        private set { if (field == value) return; field = value; Invalidate(); }
    }

    public bool IsHThumbHovered
    {
        get;
        private set { if (field == value) return; field = value; Invalidate(); }
    }

    public NonFocusableFrame ContentContainer { get; }

    public ScrollingFrame()
    {
        _isRedirecting = true;
        ContentContainer = new()
        {
            BackgroundColor = SKColors.Transparent,
            BorderThickness = 0,
            Parent = this
        };
        _isRedirecting = false;
    }

    protected override void OnChildAdded(IUIControl child)
    {
        if (_isRedirecting || child == ContentContainer) return;

        _isRedirecting = true;
        child.Parent = ContentContainer;
        _isRedirecting = false;
    }

    public void RecalculateCanvasSize()
    {
        if (!AutoCanvasSize) return;

        var maxX = Width;
        var maxY = Height;

        foreach (var child in ContentContainer.Children)
        {
            var right = child.Location.X + child.Bounds.Width;
            var bottom = child.Location.Y + child.Bounds.Height;

            if (right > maxX) maxX = right;
            if (bottom > maxY) maxY = bottom;
        }

        CanvasSize = new SKSize(maxX, maxY);
    }

    public override bool Intersects(SKPoint clientPoint) => Bounds.Contains(clientPoint.X, clientPoint.Y);

    protected override bool OnKey(KeyEventContext keyContext)
    {
        _isShiftPressed = keyContext.IsShiftPressed;
        return true;
    }

    private SKRect GetVThumbRect()
    {
        if (CanvasSize.Height <= Bounds.Height) return SKRect.Empty;

        var hasHBar = CanvasSize.Width > Bounds.Width;
        var trackHeight = Bounds.Height - (hasHBar ? ScrollBarWidth : 0f);

        var heightRatio = trackHeight / CanvasSize.Height;
        var thumbHeight = Math.Max(20f, trackHeight * heightRatio);
        var scrollPercent = ScrollOffset.Y / (CanvasSize.Height - Bounds.Height);
        var thumbY = Bounds.Top + (scrollPercent * (trackHeight - thumbHeight));

        return SKRect.Create(Bounds.Right - ScrollBarWidth, thumbY, ScrollBarWidth, thumbHeight);
    }

    private SKRect GetHThumbRect()
    {
        if (CanvasSize.Width <= Bounds.Width) return SKRect.Empty;

        var hasVBar = CanvasSize.Height > Bounds.Height;
        var trackWidth = Bounds.Width - (hasVBar ? ScrollBarWidth : 0f);

        var widthRatio = trackWidth / CanvasSize.Width;
        var thumbWidth = Math.Max(20f, trackWidth * widthRatio);
        var scrollPercent = ScrollOffset.X / (CanvasSize.Width - Bounds.Width);
        var thumbX = Bounds.Left + (scrollPercent * (trackWidth - thumbWidth));

        return SKRect.Create(thumbX, Bounds.Bottom - ScrollBarWidth, thumbWidth, ScrollBarWidth);
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        var vThumbRect = GetVThumbRect();
        var hThumbRect = GetHThumbRect();

        IsVThumbHovered = IsHovered && vThumbRect.Contains(mouseContext.ClientPosition);
        IsHThumbHovered = IsHovered && hThumbRect.Contains(mouseContext.ClientPosition);

        if (mouseContext.Button == MouseButton.Left && mouseContext.Type == MouseEventType.MouseDown)
        {
            if (IsVThumbHovered)
            {
                _isDraggingVBar = true;
                _dragStartMousePos = mouseContext.ClientPosition;
                _dragStartScrollOffset = ScrollOffset;
                return true;
            }
            if (IsHThumbHovered)
            {
                _isDraggingHBar = true;
                _dragStartMousePos = mouseContext.ClientPosition;
                _dragStartScrollOffset = ScrollOffset;
                return true;
            }
        }

        if (mouseContext.Button == MouseButton.Left && mouseContext.Type == MouseEventType.MouseUp)
        {
            _isDraggingVBar = false;
            _isDraggingHBar = false;
        }

        if (mouseContext.Type == MouseEventType.Move)
        {
            if (_isDraggingVBar)
            {
                var hasHBar = CanvasSize.Width > Bounds.Width;
                var trackHeight = Bounds.Height - (hasHBar ? ScrollBarWidth : 0f);
                var thumbHeight = vThumbRect.Height;
                var availableTrack = trackHeight - thumbHeight;

                if (availableTrack > 0)
                {
                    var deltaY = mouseContext.ClientPosition.Y - _dragStartMousePos.Y;
                    var scrollDelta = deltaY / availableTrack * (CanvasSize.Height - Bounds.Height);
                    var maxScrollY = Math.Max(0, CanvasSize.Height - Bounds.Height);

                    ScrollOffset = new SKPoint(
                        ScrollOffset.X,
                        Math.Clamp(_dragStartScrollOffset.Y + scrollDelta, 0, maxScrollY)
                    );
                }
                return true;
            }

            if (_isDraggingHBar)
            {
                var hasVBar = CanvasSize.Height > Bounds.Height;
                var trackWidth = Bounds.Width - (hasVBar ? ScrollBarWidth : 0f);
                var thumbWidth = hThumbRect.Width;
                var availableTrack = trackWidth - thumbWidth;

                if (availableTrack > 0)
                {
                    var deltaX = mouseContext.ClientPosition.X - _dragStartMousePos.X;
                    var scrollDelta = deltaX / availableTrack * (CanvasSize.Width - Bounds.Width);
                    var maxScrollX = Math.Max(0, CanvasSize.Width - Bounds.Width);

                    ScrollOffset = new SKPoint(
                        Math.Clamp(_dragStartScrollOffset.X + scrollDelta, 0, maxScrollX),
                        ScrollOffset.Y
                    );
                }
                return true;
            }
        }

        if (mouseContext.Type == MouseEventType.Wheel && IsHovered)
        {
            var scrollDelta = mouseContext.WheelDelta * 20f;

            if (_isShiftPressed)
            {
                var maxScrollX = Math.Max(0, CanvasSize.Width - Bounds.Width);
                var newX = Math.Clamp(ScrollOffset.X - scrollDelta, 0, maxScrollX);
                ScrollOffset = new SKPoint(newX, ScrollOffset.Y);
            }
            else
            {
                var maxScrollY = Math.Max(0, CanvasSize.Height - Bounds.Height);
                var newY = Math.Clamp(ScrollOffset.Y - scrollDelta, 0, maxScrollY);
                ScrollOffset = new SKPoint(ScrollOffset.X, newY);
            }

            return true;
        }

        return false;
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!IsHovered && !_isDraggingVBar && !_isDraggingHBar)
        {
            IsVThumbHovered = false;
            IsHThumbHovered = false;
        }

        RecalculateCanvasSize();

        var maxScrollX = Math.Max(0, CanvasSize.Width - Bounds.Width);
        var maxScrollY = Math.Max(0, CanvasSize.Height - Bounds.Height);

        ScrollOffset = new SKPoint(
            Math.Clamp(ScrollOffset.X, 0, maxScrollX),
            Math.Clamp(ScrollOffset.Y, 0, maxScrollY)
        );

        ContentContainer.Location = new SKPoint(-ScrollOffset.X, -ScrollOffset.Y);

        ContentContainer.Size = new SKSize(
            CanvasSize.Width + ScrollOffset.X,
            CanvasSize.Height + ScrollOffset.Y
        );
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        using (var bgPaint = new SKPaint { Color = BackgroundColor, Style = SKPaintStyle.Fill, IsAntialias = false })
        {
            canvas.DrawRect(Bounds, bgPaint);
        }

        ContentContainer.Draw(canvas);

        var hasVBar = CanvasSize.Height > Bounds.Height;
        var hasHBar = CanvasSize.Width > Bounds.Width;

        if (hasVBar)
        {
            var trackHeight = Bounds.Height - (hasHBar ? ScrollBarWidth : 0f);
            var scrollTrackRect = SKRect.Create(Bounds.Right - ScrollBarWidth, Bounds.Top, ScrollBarWidth, trackHeight);

            using var trackPaint = new SKPaint { Color = TrackColor, Style = SKPaintStyle.Fill, IsAntialias = false };
            canvas.DrawRect(scrollTrackRect, trackPaint);

            var vThumbRect = GetVThumbRect();
            using var thumbPaint = new SKPaint
            {
                Color = (IsVThumbHovered || _isDraggingVBar) ? ThumbHoverColor : ThumbColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = false
            };
            canvas.DrawRect(vThumbRect, thumbPaint);
        }

        if (hasHBar)
        {
            var trackWidth = Bounds.Width - (hasVBar ? ScrollBarWidth : 0f);
            var scrollTrackRect = SKRect.Create(Bounds.Left, Bounds.Bottom - ScrollBarWidth, trackWidth, ScrollBarWidth);

            using var trackPaint = new SKPaint { Color = TrackColor, Style = SKPaintStyle.Fill, IsAntialias = false };
            canvas.DrawRect(scrollTrackRect, trackPaint);

            var hThumbRect = GetHThumbRect();
            using var thumbPaint = new SKPaint
            {
                Color = (IsHThumbHovered || _isDraggingHBar) ? ThumbHoverColor : ThumbColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = false
            };
            canvas.DrawRect(hThumbRect, thumbPaint);
        }

        if (hasVBar && hasHBar)
        {
            var cornerRect = SKRect.Create(Bounds.Right - ScrollBarWidth, Bounds.Bottom - ScrollBarWidth, ScrollBarWidth, ScrollBarWidth);
            using var cornerPaint = new SKPaint { Color = TrackColor, Style = SKPaintStyle.Fill, IsAntialias = false };
            canvas.DrawRect(cornerRect, cornerPaint);
        }
    }

    protected override void OnDispose()
    {
        ContentContainer.Dispose();
        base.OnDispose();
    }

    public ScrollingFrame WithWidth(float width) { Width = width; return this; }
    public ScrollingFrame WithHeight(float height) { Height = height; return this; }
    public ScrollingFrame WithSize(float width, float height) { Size = new SKSize(width, height); return this; }
    public ScrollingFrame WithAutoCanvasSize(bool autoCanvas) { AutoCanvasSize = autoCanvas; return this; }
    public ScrollingFrame WithCanvasSize(float width, float height) { CanvasSize = new SKSize(width, height); return this; }
    public ScrollingFrame WithScrollOffset(SKPoint offset) { ScrollOffset = offset; return this; }
    public ScrollingFrame WithScrollBarWidth(float width) { ScrollBarWidth = width; return this; }
    public ScrollingFrame WithColors(SKColor background, SKColor track, SKColor thumb)
    {
        BackgroundColor = background;
        TrackColor = track;
        ThumbColor = thumb;
        return this;
    }
    public ScrollingFrame WithBounds(SKRect bounds) { Bounds = bounds; return this; }
    public ScrollingFrame WithParent(UIControlBase? parent) { Parent = parent; return this; }
}