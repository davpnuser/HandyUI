using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using SkiaSharp;

namespace HandyUI.Core.Classes.Controls;

public class CheckBox : UIControlBase<CheckBox>
{
    public CheckBox()
    {
        RecalculateBounds();
        UpdateBrushes();
    }

    public new bool IsEnabled
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = true;

    public bool IsChecked
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            Invalidate();
        }
    } = false;

    public float BoxSize
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            RecalculateBounds();
            Invalidate();
        }
    } = 20f;

    public float Spacing
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            Invalidate();
        }
    } = 5f;

    public float BorderWidth
    {
        get;
        set
        {
            if (Math.Abs(field - value) < 0.001f) return;
            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = 2f;

    public ThemeRecord Theme
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateBrushes();
            Invalidate();
        }
    } = DefaultTheme.GetTheme();

    private bool _colorsInitialized = false;
    private readonly SKPaint _primaryPaint = new() { Style = SKPaintStyle.Fill };
    private readonly SKPaint _backgroundPaint = new() { Style = SKPaintStyle.Fill };
    private readonly SKPaint _darkerBackgroundPaint = new() { Style = SKPaintStyle.Fill };
    private readonly SKPaint _borderPaint = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 2f };

    public Action<bool>? OnCheckChanged { get; set; }

    private void RecalculateBounds()
    {
        Bounds = new(0, 0, BoxSize, BoxSize);
    }

    private void UpdateBrushes()
    {
        _colorsInitialized = true;

        if (IsEnabled)
        {
            _primaryPaint.Color = Theme.PrimaryColor;
            _backgroundPaint.Color = Theme.BackgroundColor;
            _darkerBackgroundPaint.Color = Theme.DarkerBackgroundColor;
            _borderPaint.Color = Theme.BorderColor;
            _borderPaint.StrokeWidth = BorderWidth;
        }
        else
        {
            _primaryPaint.Color = Theme.DisabledColor;
            _backgroundPaint.Color = Theme.DarkerBackgroundColor;
            _borderPaint.Color = new SKColor(0, 0, 0, 0);
        }
    }

    public override bool Intersects(SKPoint clientPoint)
    {
        return Bounds.Contains(clientPoint.X, clientPoint.Y);
    }

    public override void Update(float deltaTime, SKPoint clientMousePosition)
    {
        if (!_colorsInitialized)
            UpdateBrushes();
    }

    public override void Draw(SKCanvas canvas)
    {
        if (IsHovered)
            canvas.DrawRect(Bounds, _darkerBackgroundPaint);
        else
            canvas.DrawRect(Bounds, _backgroundPaint);

        var x = BorderWidth / 2;
        var w = Bounds.Width - BorderWidth;
        canvas.DrawRect(x, x, w, w, _borderPaint);

        if (IsChecked)
        {
            w = Bounds.Width - (Spacing * 2);
            canvas.DrawRect(Spacing, Spacing, w, w, _primaryPaint);
        }
    }

    protected override bool OnMouse(MouseEventContext mouseContext)
    {
        if (mouseContext.Type == MouseEventType.MouseUp && IsHovered && IsEnabled)
        {
            IsChecked = !IsChecked;
            OnCheckChanged?.Invoke(IsChecked);
            return true;
        }

        return false;
    }

    protected override void OnDispose()
    {
        _primaryPaint.Dispose();
        _backgroundPaint.Dispose();
        _darkerBackgroundPaint.Dispose();
        _borderPaint.Dispose();
    }

    public CheckBox WithIsEnabled(bool isEnabled) { IsEnabled = isEnabled; return this; }
    public CheckBox WithIsChecked(bool isChecked) { IsChecked = isChecked; return this; }
    public CheckBox WithBoxSize(float size) { BoxSize = size; return this; }
    public CheckBox WithTheme(ThemeRecord theme) { Theme = theme; return this; }
    public CheckBox WithOnCheckChanged(Action<bool> onCheckChanged) { OnCheckChanged = onCheckChanged; return this; }
    public CheckBox WithParent(UIControlBase? parent) { Parent = parent; return this; }
}