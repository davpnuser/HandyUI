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

    public bool IsThumbHovered
    {
        get;
        private set { if (field == value) return; field = value; Invalidate(); }
    }

    public Frame ContentContainer { get; }

    public ScrollingFrame()
    {
        _isRedirecting = true;
        ContentContainer = new Frame
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

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (CanvasSize.Height > Bounds.Height)
        {
            var heightRatio = Bounds.Height / CanvasSize.Height;
            var thumbHeight = Math.Max(20f, Bounds.Height * heightRatio);
            var scrollPercent = ScrollOffset.Y / (CanvasSize.Height - Bounds.Height);
            var thumbY = Bounds.Top + (scrollPercent * (Bounds.Height - thumbHeight));
            var thumbRect = SKRect.Create(Bounds.Right - ScrollBarWidth, thumbY, ScrollBarWidth, thumbHeight);

            IsThumbHovered = IsHovered && thumbRect.Contains(mouseContext.ClientPosition);
        }

        if (mouseContext.Type == MouseEventType.Wheel && IsHovered)
        {
            var newY = ScrollOffset.Y - (mouseContext.WheelDelta * 20f);
            var maxY = Math.Max(0, CanvasSize.Height - Bounds.Height);

            ScrollOffset = new SKPoint(ScrollOffset.X, Math.Clamp(newY, 0, maxY));
            return true;
        }

        return false;
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!IsHovered && IsThumbHovered)
        {
            IsThumbHovered = false;
        }

        RecalculateCanvasSize();

        ContentContainer.Bounds = SKRect.Create(
            Bounds.Left - ScrollOffset.X,
            Bounds.Top - ScrollOffset.Y,
            CanvasSize.Width,
            CanvasSize.Height
        );
    }

    public override void Draw(SKCanvas canvas)
    {
        if (!IsVisible) return;

        using var bgPaint = new SKPaint { Color = BackgroundColor, Style = SKPaintStyle.Fill, IsAntialias = false };
        canvas.DrawRect(Bounds, bgPaint);

        if (CanvasSize.Height > Bounds.Height)
        {
            var scrollTrackRect = SKRect.Create(Bounds.Right - ScrollBarWidth, Bounds.Top, ScrollBarWidth, Bounds.Height);

            using var trackPaint = new SKPaint { Color = TrackColor, Style = SKPaintStyle.Fill, IsAntialias = false };
            canvas.DrawRect(scrollTrackRect, trackPaint);

            var heightRatio = Bounds.Height / CanvasSize.Height;
            var thumbHeight = Math.Max(20f, Bounds.Height * heightRatio);
            var scrollPercent = ScrollOffset.Y / (CanvasSize.Height - Bounds.Height);
            var thumbY = Bounds.Top + (scrollPercent * (Bounds.Height - thumbHeight));

            var thumbRect = SKRect.Create(Bounds.Right - ScrollBarWidth, thumbY, ScrollBarWidth, thumbHeight);

            using var thumbPaint = new SKPaint
            {
                Color = IsThumbHovered ? ThumbHoverColor : ThumbColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = false
            };
            canvas.DrawRect(thumbRect, thumbPaint);
        }
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