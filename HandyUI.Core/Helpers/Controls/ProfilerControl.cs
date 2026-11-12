using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using SkiaSharp;

namespace HandyUI.Core.Helpers.Controls;

public class ProfilerControl : BaseControl<ProfilerControl>
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
    }

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
    }

    public bool ShowGraph
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    } = true;

    public bool ShowMemory
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    } = true;

    public bool ShowGcCounts
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    } = false;

    public bool ShowTotalTime
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    } = true;

    public bool ShowButtons
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ResizeToContent();
            Invalidate();
        }
    } = true;

    public bool ContinuousUpdate
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (value) Invalidate();
        }
    } = true;

    public int RefreshRate
    {
        get;
        set
        {
            var v = Math.Max(1, value);
            if (field == v) return;
            field = v;
            Invalidate();
        }
    } = 30;

    public ProfilerAnchor Anchor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = ProfilerAnchor.TopRight;

    public float AnchorMargin { get; set; } = 12f;

    public float GraphHeight
    {
        get;
        set
        {
            var v = Math.Max(10f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 60f;

    public float CornerRadius { get; set; } = 6f;

    public float Padding
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
    } = 10f;

    public float FontSize
    {
        get;
        set
        {
            var v = Math.Max(6f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            _font.Size = v;
            RebuildBlobs();
            ResizeToContent();
            Invalidate();
        }
    } = 12f;

    public SKTypeface Typeface
    {
        get => _font.Typeface ?? FontManager.DefaultFont!;
        set
        {
            var v = value ?? FontManager.DefaultFont!;
            if (ReferenceEquals(_font.Typeface, v)) return;
            _font.Typeface = v;
            RebuildBlobs();
            ResizeToContent();
            Invalidate();
        }
    }

    public int SampleCount
    {
        get;
        set
        {
            var v = Math.Max(2, value);
            if (field == v) return;
            field = v;
            _samples = new float[v];
            _sampleHead = 0;
            _sampleCount = 0;
            _runningSum = 0f;
            Invalidate();
        }
    } = 120;

    public float MaxFrameTimeForGraph { get; set; } = 33.3f;

    public int TargetFpsForGraph { get; set; } = 60;

    public float ButtonHeight
    {
        get;
        set
        {
            var v = Math.Max(10f, value);
            if (Math.Abs(field - v) < 0.0001f) return;
            field = v;
            ResizeToContent();
            Invalidate();
        }
    } = 22f;

    public float ButtonGap
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
    } = 6f;

    public SKColor BackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = new SKColor(0, 0, 0, 180);

    public SKColor TextColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = SKColors.White;

    public SKColor AccentColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate();
        }
    } = new SKColor(100, 220, 100);

    #endregion

    #region Stats

    public float CurrentFps { get; private set; }
    public float CurrentFrameMs { get; private set; }
    public float AverageFrameMs { get; private set; }
    public float MaxFrameMs { get; private set; }
    public float MemoryMb { get; private set; }
    public double TotalTime { get; private set; }

    #endregion

    #region Events

    public event Action? WhenDestroyRequested;
    public event Action? WhenRestartRequested;

    #endregion

    #region Internal Fields

    private const string SepText = "  •  ";
    private const string DestroyButtonText = "Destroy Profiler";
    private const string RestartButtonText = "Restart Profiler";

    private readonly SKFont _font;
    private readonly SKPaint _bgPaint = new() { IsAntialias = true };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _graphBgPaint = new() { IsAntialias = true };
    private readonly SKPaint _graphLinePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
    private readonly SKPaint _graphRefPaint = new() { IsAntialias = false, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
    private readonly SKPaint _buttonBgPaint = new() { IsAntialias = true };
    private readonly SKPaint _buttonBorderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPath _graphPath = new();

    private float[] _samples;
    private int _sampleHead;
    private int _sampleCount;
    private float _runningSum;
    private int _framesThisSecond;
    private float _secondTimer;
    private int _gc0, _gc1, _gc2;

    private string _fpsText = "";
    private string _msText = "";
    private string _avgMaxText = "";
    private string _memText = "";
    private string _gcText = "";
    private string _totalTimeText = "";

    private SKTextBlob? _fpsBlob;
    private SKTextBlob? _msBlob;
    private SKTextBlob? _avgMaxBlob;
    private SKTextBlob? _memBlob;
    private SKTextBlob? _gcBlob;
    private SKTextBlob? _totalTimeBlob;
    private SKTextBlob? _sepBlob;
    private SKTextBlob? _destroyBlob;
    private SKTextBlob? _restartBlob;

    private float _fpsWidth;
    private float _msWidth;
    private float _avgMaxWidth;
    private float _memWidth;
    private float _gcWidth;
    private float _totalTimeWidth;
    private float _sepWidth;
    private float _destroyWidth;
    private float _restartWidth;

    private float _lastFormattedFps = float.NaN;
    private float _lastFormattedMs = float.NaN;
    private float _lastFormattedAvg = float.NaN;
    private float _lastFormattedMax = float.NaN;
    private float _lastFormattedMem = float.NaN;
    private double _lastFormattedTotalTime = double.NaN;
    private int _lastGc0 = -1, _lastGc1 = -1, _lastGc2 = -1;

    private SKRect _destroyButtonRect;
    private SKRect _restartButtonRect;
    private int _hoveredButton;

    private float _refreshAccum;
    private float _statsAccum;

    #endregion

    #region Constructor

    public ProfilerControl()
    {
        _samples = new float[SampleCount];
        _font = new SKFont(FontManager.DefaultFont, FontSize)
        {
            Edging = SKFontEdging.Antialias,
            Subpixel = true,
        };
        ZIndex = 10000;
        RebuildBlobs();
        ResizeToContent();
    }

    #endregion

    #region Overrides

    public override bool DoesIntersect(SKPoint point) => Bounds.Contains(point);

    public override void OnUpdate(UpdateContext updateContext)
    {
        if (Anchor != ProfilerAnchor.None)
        {
            var hostSize = updateContext.Host.Size;
            var newLocation = Anchor switch
            {
                ProfilerAnchor.TopLeft => new SKPoint(AnchorMargin, AnchorMargin),
                ProfilerAnchor.TopRight => new SKPoint(hostSize.Width - Bounds.Width - AnchorMargin, AnchorMargin),
                ProfilerAnchor.BottomLeft => new SKPoint(AnchorMargin, hostSize.Height - Bounds.Height - AnchorMargin),
                ProfilerAnchor.BottomRight => new SKPoint(hostSize.Width - Bounds.Width - AnchorMargin, hostSize.Height - Bounds.Height - AnchorMargin),
                _ => Location,
            };

            if (Location != newLocation)
                Location = newLocation;
        }

        TotalTime = updateContext.Host.TotalTime;

        if (ShowTotalTime)
        {
            var roundedTime = Math.Round(TotalTime, 1);
            if (roundedTime != _lastFormattedTotalTime)
            {
                _lastFormattedTotalTime = roundedTime;
                _totalTimeText = $"T  {FormatTotalTime(TotalTime)}";
                ReplaceBlob(ref _totalTimeBlob, _totalTimeText, ref _totalTimeWidth);
            }
        }

        if (!ContinuousUpdate) return;

        var dt = updateContext.DeltaTime;
        if (dt <= 0f) return;

        CurrentFrameMs = dt * 1000f;

        _framesThisSecond++;
        _secondTimer += dt;
        if (_secondTimer >= 1f)
        {
            CurrentFps = _framesThisSecond / _secondTimer;
            _framesThisSecond = 0;
            _secondTimer = 0f;
        }

        if (_sampleCount == SampleCount)
            _runningSum -= _samples[_sampleHead];

        _samples[_sampleHead] = CurrentFrameMs;
        _runningSum += CurrentFrameMs;
        _sampleHead = (_sampleHead + 1) % SampleCount;
        if (_sampleCount < SampleCount) _sampleCount++;

        _statsAccum += dt;
        if (_statsAccum >= 1f)
        {
            _statsAccum = 0f;
            MemoryMb = (float)(GC.GetTotalMemory(false) / (1024.0 * 1024.0));
            _gc0 = GC.CollectionCount(0);
            _gc1 = GC.CollectionCount(1);
            _gc2 = GC.CollectionCount(2);
        }

        _refreshAccum += dt;
        var interval = 1f / RefreshRate;
        if (_refreshAccum >= interval)
        {
            _refreshAccum = 0f;

            AverageFrameMs = _sampleCount > 0 ? _runningSum / _sampleCount : 0f;

            var max = 0f;
            for (var i = 0; i < _sampleCount; i++)
            {
                if (_samples[i] > max) max = _samples[i];
            }
            MaxFrameMs = max;

            UpdateFormattedStrings();
            Invalidate();
        }
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        _bgPaint.Color = BackgroundColor;
        canvas.DrawRoundRect(new SKRect(0, 0, w, h), CornerRadius, CornerRadius, _bgPaint);

        _borderPaint.Color = AccentColor.WithAlpha(90);
        _borderPaint.StrokeWidth = 1f;
        canvas.DrawRoundRect(new SKRect(0.5f, 0.5f, w - 0.5f, h - 0.5f), CornerRadius, CornerRadius, _borderPaint);

        var x = Padding;
        var y = Padding;
        var lineHeight = _font.Size + 4f;
        var baseline = y + _font.Size;

        if (_fpsBlob != null)
        {
            _textPaint.Color = CurrentFps >= 55f ? AccentColor
                : CurrentFps >= 30f ? new SKColor(255, 180, 80)
                : new SKColor(255, 90, 90);
            canvas.DrawText(_fpsBlob, x, baseline, _textPaint);
            x += _fpsWidth;

            if (_sepBlob != null)
            {
                _textPaint.Color = TextColor.WithAlpha(120);
                canvas.DrawText(_sepBlob, x, baseline, _textPaint);
                x += _sepWidth;
            }

            if (_msBlob != null)
            {
                _textPaint.Color = TextColor;
                canvas.DrawText(_msBlob, x, baseline, _textPaint);
                x += _msWidth;
            }

            if (_avgMaxBlob != null)
            {
                _textPaint.Color = TextColor.WithAlpha(150);
                canvas.DrawText(_avgMaxBlob, x, baseline, _textPaint);
            }
        }

        y += lineHeight;

        if (ShowTotalTime)
        {
            x = Padding;
            baseline = y + _font.Size;

            if (_totalTimeBlob != null)
            {
                _textPaint.Color = TextColor.WithAlpha(180);
                canvas.DrawText(_totalTimeBlob, x, baseline, _textPaint);
            }

            y += lineHeight;
        }

        if (ShowMemory)
        {
            x = Padding;
            baseline = y + _font.Size;

            if (_memBlob != null)
            {
                _textPaint.Color = TextColor;
                canvas.DrawText(_memBlob, x, baseline, _textPaint);
                x += _memWidth;
            }

            if (ShowGcCounts && _gcBlob != null)
            {
                _textPaint.Color = TextColor.WithAlpha(150);
                canvas.DrawText(_gcBlob, x, baseline, _textPaint);
            }

            y += lineHeight;
        }

        if (ShowGraph && GraphHeight > 0f)
        {
            var graphRect = new SKRect(Padding, y, w - Padding, y + GraphHeight);

            _graphBgPaint.Color = TextColor.WithAlpha(20);
            canvas.DrawRoundRect(graphRect, 3f, 3f, _graphBgPaint);

            var targetMs = 1000f / Math.Max(1, TargetFpsForGraph);
            var targetNorm = targetMs / MaxFrameTimeForGraph;
            if (targetNorm is > 0f and < 1f)
            {
                var refY = graphRect.Bottom - (targetNorm * graphRect.Height);
                _graphRefPaint.Color = AccentColor.WithAlpha(60);
                canvas.DrawLine(graphRect.Left + 2f, refY, graphRect.Right - 2f, refY, _graphRefPaint);
            }

            if (_sampleCount >= 2)
            {
                _graphPath.Reset();
                var stepX = graphRect.Width / (SampleCount - 1);
                var clipTop = graphRect.Top + 2f;
                var clipBottom = graphRect.Bottom - 2f;

                for (var i = 0; i < _sampleCount; i++)
                {
                    var idx = (((_sampleHead - _sampleCount + i) % SampleCount) + SampleCount) % SampleCount;
                    var val = Math.Clamp(_samples[idx], 0f, MaxFrameTimeForGraph);
                    var norm = val / MaxFrameTimeForGraph;
                    var px = graphRect.Left + (i * stepX);
                    var py = Math.Clamp(graphRect.Bottom - (norm * graphRect.Height), clipTop, clipBottom);

                    if (i == 0) _graphPath.MoveTo(px, py);
                    else _graphPath.LineTo(px, py);
                }

                _graphLinePaint.Color = AccentColor;
                _graphLinePaint.StrokeWidth = 1.5f;
                canvas.DrawPath(_graphPath, _graphLinePaint);
            }
        }

        if (ShowButtons)
        {
            DrawButton(canvas, _destroyButtonRect, _destroyBlob, _destroyWidth, buttonId: 1);
            DrawButton(canvas, _restartButtonRect, _restartBlob, _restartWidth, buttonId: 2);
        }
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (!ShowButtons) return false;
        if (eventContext.MousePosition is not { } pos) return false;

        var hovered = 0;
        if (_destroyButtonRect.Contains(pos)) hovered = 1;
        else if (_restartButtonRect.Contains(pos)) hovered = 2;

        if (hovered != _hoveredButton)
        {
            _hoveredButton = hovered;
            Invalidate();
        }

        if (eventContext.Type == MouseEventType.ClickEvent)
        {
            if (hovered == 1)
            {
                WhenDestroyRequested?.Invoke();
                return true;
            }

            if (hovered == 2)
            {
                WhenRestartRequested?.Invoke();
                return true;
            }
        }

        return hovered != 0;
    }

    #endregion

    #region Internal Helpers

    private void ResizeToContent()
    {
        var lineHeight = _font.Size + 4f;
        var lineCount = 1;
        if (ShowTotalTime) lineCount++;
        if (ShowMemory) lineCount++;

        var contentH = (Padding * 2) + (lineCount * lineHeight);
        if (ShowGraph) contentH += GraphHeight + 4f;
        if (ShowButtons) contentH += ButtonHeight + 6f;

        var minWidth = _font.MeasureText("60.0 FPS  •  16.6 ms  avg 16.6  max 16.6") + (Padding * 2);

        if (ShowTotalTime)
            minWidth = Math.Max(minWidth, _font.MeasureText("T  12:34:56") + (Padding * 2));

        if (ShowMemory)
        {
            var sampleMem = ShowGcCounts
                ? "999.9 MB  GC 99/99/99"
                : "999.9 MB";
            minWidth = Math.Max(minWidth, _font.MeasureText(sampleMem) + (Padding * 2));
        }

        if (ShowButtons)
        {
            var destroyW = _font.MeasureText(DestroyButtonText);
            var restartW = _font.MeasureText(RestartButtonText);
            var buttonsMinWidth = destroyW + restartW + (ButtonGap * 2) + (Padding * 2) + 16f;
            minWidth = Math.Max(minWidth, buttonsMinWidth);
        }

        var w = Width > 0f ? Width : minWidth;
        var h = Height > 0f ? Height : contentH;
        Bounds = new SKRect(0, 0, w, h);

        if (ShowButtons)
        {
            var btnY = h - Padding - ButtonHeight;
            var btnW = (w - (Padding * 2) - ButtonGap) / 2f;
            _destroyButtonRect = new SKRect(Padding, btnY, Padding + btnW, btnY + ButtonHeight);
            _restartButtonRect = new SKRect(Padding + btnW + ButtonGap, btnY, w - Padding, btnY + ButtonHeight);
        }
        else
        {
            _destroyButtonRect = SKRect.Empty;
            _restartButtonRect = SKRect.Empty;
        }
    }

    private void DrawButton(SKCanvas canvas, SKRect rect, SKTextBlob? blob, float textWidth, int buttonId)
    {
        if (rect.IsEmpty) return;

        var hovered = _hoveredButton == buttonId;

        _buttonBgPaint.Color = hovered
            ? TextColor.WithAlpha(50)
            : TextColor.WithAlpha(20);
        canvas.DrawRoundRect(rect, 3f, 3f, _buttonBgPaint);

        _buttonBorderPaint.Color = hovered
            ? AccentColor.WithAlpha(180)
            : TextColor.WithAlpha(60);
        _buttonBorderPaint.StrokeWidth = 1f;
        canvas.DrawRoundRect(new SKRect(rect.Left + 0.5f, rect.Top + 0.5f, rect.Right - 0.5f, rect.Bottom - 0.5f), 3f, 3f, _buttonBorderPaint);

        if (blob is null) return;

        var tx = rect.MidX - (textWidth * 0.5f);
        var ty = rect.MidY - ((_font.Metrics.Ascent + _font.Metrics.Descent) * 0.5f);

        _textPaint.Color = TextColor;
        canvas.DrawText(blob, tx, ty, _textPaint);
    }

    private static string FormatTotalTime(double seconds)
    {
        if (seconds < 0) seconds = 0;

        if (seconds >= 3600)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        var mins = (int)(seconds / 60);
        var secs = seconds - (mins * 60);
        return $"{mins:D2}:{secs:00.0}";
    }

    private void RebuildBlobs()
    {
        DisposeBlob(ref _fpsBlob);
        DisposeBlob(ref _msBlob);
        DisposeBlob(ref _avgMaxBlob);
        DisposeBlob(ref _memBlob);
        DisposeBlob(ref _gcBlob);
        DisposeBlob(ref _totalTimeBlob);
        DisposeBlob(ref _sepBlob);
        DisposeBlob(ref _destroyBlob);
        DisposeBlob(ref _restartBlob);

        _sepBlob = SKTextBlob.Create(SepText, _font);
        _sepWidth = _font.MeasureText(SepText);

        _destroyBlob = SKTextBlob.Create(DestroyButtonText, _font);
        _destroyWidth = _font.MeasureText(DestroyButtonText);

        _restartBlob = SKTextBlob.Create(RestartButtonText, _font);
        _restartWidth = _font.MeasureText(RestartButtonText);

        _lastFormattedFps = float.NaN;
        _lastFormattedMs = float.NaN;
        _lastFormattedAvg = float.NaN;
        _lastFormattedMax = float.NaN;
        _lastFormattedMem = float.NaN;
        _lastFormattedTotalTime = double.NaN;
        _lastGc0 = _lastGc1 = _lastGc2 = -1;

        UpdateFormattedStrings();
    }

    private void UpdateFormattedStrings()
    {
        var roundedFps = MathF.Round(CurrentFps * 10f) / 10f;
        if (roundedFps != _lastFormattedFps)
        {
            _lastFormattedFps = roundedFps;
            _fpsText = $"{roundedFps:F1} FPS";
            ReplaceBlob(ref _fpsBlob, _fpsText, ref _fpsWidth);
        }

        var roundedMs = MathF.Round(CurrentFrameMs * 10f) / 10f;
        if (roundedMs != _lastFormattedMs)
        {
            _lastFormattedMs = roundedMs;
            _msText = $"{roundedMs:F1} ms";
            ReplaceBlob(ref _msBlob, _msText, ref _msWidth);
        }

        var roundedAvg = MathF.Round(AverageFrameMs * 10f) / 10f;
        var roundedMax = MathF.Round(MaxFrameMs * 10f) / 10f;
        if (roundedAvg != _lastFormattedAvg || roundedMax != _lastFormattedMax)
        {
            _lastFormattedAvg = roundedAvg;
            _lastFormattedMax = roundedMax;
            _avgMaxText = $"  avg {roundedAvg:F1}  max {roundedMax:F1}";
            ReplaceBlob(ref _avgMaxBlob, _avgMaxText, ref _avgMaxWidth);
        }

        if (ShowMemory)
        {
            var roundedMem = MathF.Round(MemoryMb * 10f) / 10f;
            if (roundedMem != _lastFormattedMem)
            {
                _lastFormattedMem = roundedMem;
                _memText = $"{roundedMem:F1} MB";
                ReplaceBlob(ref _memBlob, _memText, ref _memWidth);
            }

            if (ShowGcCounts && (_gc0 != _lastGc0 || _gc1 != _lastGc1 || _gc2 != _lastGc2))
            {
                _lastGc0 = _gc0;
                _lastGc1 = _gc1;
                _lastGc2 = _gc2;
                _gcText = $"  GC {_gc0}/{_gc1}/{_gc2}";
                ReplaceBlob(ref _gcBlob, _gcText, ref _gcWidth);
            }
        }
    }

    private void ReplaceBlob(ref SKTextBlob? blob, string text, ref float width)
    {
        DisposeBlob(ref blob);

        if (text.Length == 0)
        {
            width = 0f;
            return;
        }

        blob = SKTextBlob.Create(text, _font);
        width = _font.MeasureText(text);
    }

    private static void DisposeBlob(ref SKTextBlob? blob)
    {
        blob?.Dispose();
        blob = null;
    }

    #endregion

    #region Fluent APIs

    public ProfilerControl WithWidth(float width) { Width = width; return Self; }
    public ProfilerControl WithHeight(float height) { Height = height; return Self; }
    public ProfilerControl WithShowGraph(bool show) { ShowGraph = show; return Self; }
    public ProfilerControl WithShowMemory(bool show) { ShowMemory = show; return Self; }
    public ProfilerControl WithShowGcCounts(bool show) { ShowGcCounts = show; return Self; }
    public ProfilerControl WithShowTotalTime(bool show) { ShowTotalTime = show; return Self; }
    public ProfilerControl WithShowButtons(bool show) { ShowButtons = show; return Self; }
    public ProfilerControl WithContinuousUpdate(bool continuous) { ContinuousUpdate = continuous; return Self; }
    public ProfilerControl WithRefreshRate(int hz) { RefreshRate = hz; return Self; }
    public ProfilerControl WithAnchor(ProfilerAnchor anchor, float margin = 12f) { Anchor = anchor; AnchorMargin = margin; return Self; }
    public ProfilerControl WithGraphHeight(float height) { GraphHeight = height; return Self; }
    public ProfilerControl WithCornerRadius(float radius) { CornerRadius = radius; return Self; }
    public ProfilerControl WithPadding(float padding) { Padding = padding; return Self; }
    public ProfilerControl WithFontSize(float size) { FontSize = size; return Self; }
    public ProfilerControl WithTypeface(SKTypeface? typeface) { Typeface = typeface!; return Self; }
    public ProfilerControl WithSampleCount(int count) { SampleCount = count; return Self; }
    public ProfilerControl WithMaxFrameTime(float maxMs) { MaxFrameTimeForGraph = maxMs; return Self; }
    public ProfilerControl WithTargetFps(int targetFps) { TargetFpsForGraph = targetFps; return Self; }
    public ProfilerControl WithButtonHeight(float height) { ButtonHeight = height; return Self; }
    public ProfilerControl WithButtonGap(float gap) { ButtonGap = gap; return Self; }
    public ProfilerControl WithBackgroundColor(SKColor color) { BackgroundColor = color; return Self; }
    public ProfilerControl WithTextColor(SKColor color) { TextColor = color; return Self; }
    public ProfilerControl WithAccentColor(SKColor color) { AccentColor = color; return Self; }

    public ProfilerControl WithWhenDestroyRequested(Action handler) { WhenDestroyRequested += handler; return Self; }
    public ProfilerControl WithWhenRestartRequested(Action handler) { WhenRestartRequested += handler; return Self; }

    #endregion

    #region Disposal

    protected override void OnDispose()
    {
        DisposeBlob(ref _fpsBlob);
        DisposeBlob(ref _msBlob);
        DisposeBlob(ref _avgMaxBlob);
        DisposeBlob(ref _memBlob);
        DisposeBlob(ref _gcBlob);
        DisposeBlob(ref _totalTimeBlob);
        DisposeBlob(ref _sepBlob);
        DisposeBlob(ref _destroyBlob);
        DisposeBlob(ref _restartBlob);

        _font.Dispose();
        _bgPaint.Dispose();
        _textPaint.Dispose();
        _borderPaint.Dispose();
        _graphBgPaint.Dispose();
        _graphLinePaint.Dispose();
        _graphRefPaint.Dispose();
        _buttonBgPaint.Dispose();
        _buttonBorderPaint.Dispose();
        _graphPath.Dispose();
    }

    #endregion
}