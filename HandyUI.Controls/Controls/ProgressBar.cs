using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.Controls.Controls;

public class ProgressBar : BaseControl<ProgressBar>
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
            Invalidate();
        }
    } = 200f;

    public float Height
    {
        get;
        set
        {
            var v = Math.Max(0f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 4f;

    public float Value
    {
        get;
        set
        {
            var v = Math.Clamp(value, 0f, 100f);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            WhenValueChanged?.Invoke(v);
            Invalidate();
        }
    }

    public bool IsIndeterminate
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (!value)
            {
                if (_indeterminateTween is { IsValid: true }) _indeterminateTween.Cancel();
                _indeterminateProgress = 0f;
            }
            Invalidate();
        }
    }

    public float BarHeight
    {
        get;
        set
        {
            var v = Math.Max(1f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            if (Height <= 0f) Height = v;
            ResizeToContent();
            Invalidate();
        }
    } = 4f;

    public float CornerRadius { get; set; } = 2f;

    #endregion

    #region Internal Fields

    private readonly SKPaint _bgPaint = new() { IsAntialias = true };
    private readonly SKPaint _fgPaint = new() { IsAntialias = true };
    private float _indeterminateProgress;
    private Tween? _indeterminateTween;

    #endregion

    #region Events

    public event Action<float>? WhenValueChanged;

    #endregion

    #region Constructor

    public ProgressBar()
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
        if (!IsIndeterminate) return;
        if (_indeterminateTween is { IsValid: true }) return;

        _indeterminateTween = updateContext.TweenManager.To(0f, 1f, 1.4f,
            v => { _indeterminateProgress = v; Invalidate(); },
            EaseType.Linear,
            () => { _indeterminateProgress = 0f; Invalidate(); });
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        var h = BarHeight;
        if (w <= 0) return;

        var y = (Bounds.Height - h) * 0.5f;
        var bgRect = new SKRect(0, y, w, y + h);

        _bgPaint.Color = ThemeManager.Instance.BackgroundTertiary;
        canvas.DrawRoundRect(bgRect, CornerRadius, CornerRadius, _bgPaint);

        _fgPaint.Color = Enabled
            ? ThemeManager.Instance.Accent
            : ThemeManager.Instance.TextDisabled;

        if (IsIndeterminate)
        {
            var segWidth = w * 0.35f;
            var startX = (_indeterminateProgress * (w + segWidth)) - segWidth;
            var fgRect = new SKRect(startX, y, startX + segWidth, y + h);
            canvas.Save();
            canvas.ClipRect(bgRect);
            canvas.DrawRoundRect(fgRect, CornerRadius, CornerRadius, _fgPaint);
            canvas.Restore();
        }
        else
        {
            var fillW = w * (Value / 100f);
            if (fillW > 0.01f)
            {
                var fgRect = new SKRect(0, y, fillW, y + h);
                canvas.DrawRoundRect(fgRect, CornerRadius, CornerRadius, _fgPaint);
            }
        }
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        Bounds = new SKRect(0, 0, Width, Height);
    }

    #endregion

    #region Fluent APIs

    public ProgressBar WithWidth(float width) { Width = width; return Self; }
    public ProgressBar WithHeight(float height) { Height = height; return Self; }
    public ProgressBar WithValue(float value) { Value = value; return Self; }
    public ProgressBar WithIsIndeterminate(bool indeterminate) { IsIndeterminate = indeterminate; return Self; }
    public ProgressBar WithBarHeight(float height) { BarHeight = height; return Self; }
    public ProgressBar WithCornerRadius(float radius) { CornerRadius = radius; return Self; }
    public ProgressBar WithWhenValueChanged(Action<float> handler) { WhenValueChanged += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        if (_indeterminateTween is { IsValid: true }) _indeterminateTween.Cancel();
        _bgPaint.Dispose();
        _fgPaint.Dispose();
    }

    #endregion
}