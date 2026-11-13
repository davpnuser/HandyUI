using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers.Extensions;
using SkiaSharp;

namespace HandyUI.Core.Helpers.Controls;

internal class ProfilerControl : BaseControl<ProfilerControl>
{
    private string _header = "Loading...";
    private float _targetFps = 0f;
    private float _fps = 0f;
    private double _totalTime = 0d;
    private bool _vSync = false;
    private bool _useDirtyRendering = false;
    private bool _isForeground = false;
    private string _currentCursor = "Loading...";
    private float _memory = 0f;
    private int _gen0 = 0;
    private int _gen1 = 0;
    private int _gen2 = 0;

    private readonly Queue<float> _frameTimeHistory = new();
    private const int _maxHistoryCount = 200;

    private readonly SKColor _regularColor = SKColors.White.WithTransparency(0.7f);
    private readonly SKColor _mouseDownColor = SKColors.White.WithTransparency(0.5f);
    private readonly SKColor _hoveredColor = SKColors.LightGray.WithTransparency(0.5f);
    private readonly SKFont _font = new(FontManager.DefaultFont, 16);
    private readonly SKPaint _fgPaint = new() { Color = SKColors.Black, Style = SKPaintStyle.Fill };
    private readonly SKPaint _fgStrokePaint = new() { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
    private readonly SKPaint _fgThickPaint = new() { Color = SKColors.Black, Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0.35f, IsAntialias = true };
    private readonly SKPaint _bgPaint = new() { Color = SKColors.White.WithTransparency(0.7f), Style = SKPaintStyle.Fill };
    private readonly SKPaint _redPaint = new() { Color = SKColors.Red, Style = SKPaintStyle.Fill };
    private readonly SKPaint _yellowPaint = new() { Color = SKColors.Yellow, Style = SKPaintStyle.Fill };
    private readonly SKPaint _greenPaint = new() { Color = SKColors.LimeGreen, Style = SKPaintStyle.Fill };

    private SKRect _graphBounds;
    private readonly SKPathBuilder _pathBuilder = new();
    private float _mouseLastSeenAtX = 0f;
    private float _mouseLastSeenAtY = 0f;

    private SKRect _diReButtonBounds;
    private SKRect _vSyncButtonBounds;
    private SKRect _increaseFpsButtonBounds;
    private SKRect _decreaseFpsButtonBounds;

    private bool _wantToToggleDirtyRendering = false;
    private bool _wantToToggleVSync = false;
    private bool _wantToIncreaseTargetFps = false;
    private bool _wantToDecreaseTargetFps = false;

    internal ProfilerControl()
    {
        Bounds = new SKRect(0, 0, 220, 370);
        _graphBounds = new(0, 200, Bounds.Width, Bounds.Height - 120);
        _diReButtonBounds = new(0, 250, 220, 280);
        _vSyncButtonBounds = new(0, 280, 220, 310);
        _increaseFpsButtonBounds = new(0, 310, 220, 340);
        _decreaseFpsButtonBounds = new(0, 340, 220, 370);
    }

    public override bool DoesIntersect(SKPoint point)
    {
        return Bounds.Contains(point);
    }

    public override void OnDraw(SKCanvas canvas)
    {
        var _targetFpsText = _vSync ? "V-Sync" : _targetFps.ToString();
        canvas.DrawRect(Bounds, _bgPaint);
        canvas.DrawRect(_graphBounds, _bgPaint);

        canvas.DrawText(_header, 0, 16, SKTextAlign.Left, _font, _fgThickPaint);
        canvas.DrawText($"FPS: {_fps:F0}", 0, 32, SKTextAlign.Left, _font, _fps > 10 ? (_fps > 40 ? _greenPaint : _yellowPaint) : _redPaint);
        canvas.DrawText($"Target FPS: {_targetFpsText}", 0, 48, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText($"V-Sync: {_vSync}", 0, 64, SKTextAlign.Left, _font, _vSync ? _greenPaint : _redPaint);
        canvas.DrawText($"Dirty Rendering: {_useDirtyRendering}", 0, 82, SKTextAlign.Left, _font, _useDirtyRendering ? _greenPaint : _redPaint);
        canvas.DrawText($"Is Foreground: {_isForeground}", 0, 98, SKTextAlign.Left, _font, _isForeground ? _greenPaint : _redPaint);
        canvas.DrawText($"Cursor: {_currentCursor}", 0, 114, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText($"Memory (MBs): {_memory:F2}", 0, 130, SKTextAlign.Left, _font, _memory < 20 ? (_memory < 10 ? _greenPaint : _yellowPaint) : _redPaint);
        canvas.DrawText($"Gen0s: {_gen0}", 0, 146, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText($"Gen1s: {_gen1}", 0, 162, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText($"Gen2s: {_gen2}", 0, 178, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText($"Total Tm. (secs): {_totalTime:F2}", 0, 194, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText("Toggle DiRe", 5, 271, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText("Toggle V-Sync", 5, 301, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText("Inc. Target FPS", 5, 331, SKTextAlign.Left, _font, _fgPaint);
        canvas.DrawText("Dec. Target FPS", 5, 361, SKTextAlign.Left, _font, _fgPaint);

        var targetMs = (_targetFps > 0) ? (1000f / _targetFps) : 13.33f;
        var maxMs = 33.33f;
        var targetY = _graphBounds.Bottom - (Math.Clamp(targetMs / maxMs, 0f, 1f) * _graphBounds.Height);
        canvas.DrawLine(_graphBounds.Left, targetY, _graphBounds.Right, targetY, _fgPaint);

        var i = 0;
        foreach (var frameTimeMs in _frameTimeHistory)
        {
            var x = _graphBounds.Left + (i * (_graphBounds.Width / _maxHistoryCount));
            var normalized = Math.Clamp(frameTimeMs / maxMs, 0f, 1f);
            var y = _graphBounds.Bottom - (normalized * _graphBounds.Height);

            if (i == 0) _pathBuilder.MoveTo(x, y);
            else _pathBuilder.LineTo(x, y);
            i++;
        }

        using (var path = _pathBuilder.Detach())
        {
            canvas.DrawPath(path, _fgStrokePaint);
        }

        if (Bounds.Contains(_mouseLastSeenAtX, _mouseLastSeenAtY))
            canvas.DrawCircle(_mouseLastSeenAtX, _mouseLastSeenAtY, 5, _redPaint);
    }

    public override void OnUpdate(UpdateContext updateContext)
    {
        var deltaTime = updateContext.DeltaTime;
        if (deltaTime <= 0f) return;

        var frameTimeMs = deltaTime * 1000f;
        _frameTimeHistory.Enqueue(frameTimeMs);

        if (_frameTimeHistory.Count > _maxHistoryCount)
        {
            _frameTimeHistory.Dequeue();
        }

        var instantFps = 1f / deltaTime;

        if (_fps == 0f)
        {
            _fps = instantFps;
        }
        else
        {
            var smoothingSpeed = 10f;
            var alpha = 1f - MathF.Exp(-smoothingSpeed * deltaTime);
            _fps += (instantFps - _fps) * alpha;
        }

        if (_targetFps == 0f || (_targetFps != updateContext.Host.FramesPerSecond))
            _targetFps = updateContext.Host.FramesPerSecond;

        if (_vSync != updateContext.Host.VSync)
            _vSync = updateContext.Host.VSync;

        if (_useDirtyRendering != updateContext.Host.UseDirtyRendering)
            _useDirtyRendering = updateContext.Host.UseDirtyRendering;

        if (_isForeground != updateContext.Host.IsForeground)
            _isForeground = updateContext.Host.IsForeground;

        if (_currentCursor != updateContext.Host.Cursor.ToString())
            _currentCursor = updateContext.Host.Cursor.ToString();

        if (IsHovered && !IsMouseDown && _bgPaint.Color != _hoveredColor)
            _bgPaint.Color = _hoveredColor;
        else if (IsHovered && IsMouseDown && _bgPaint.Color != _mouseDownColor)
            _bgPaint.Color = _mouseDownColor;
        else if (!IsHovered && !IsMouseDown && _bgPaint.Color != _regularColor)
            _bgPaint.Color = _regularColor;

        if (_useDirtyRendering && _header != "HandyUI Profiler (DiRe)")
            _header = "HandyUI Profiler (DiRe)";
        else if (!_useDirtyRendering && _header != "HandyUI Profiler")
            _header = "HandyUI Profiler";

        _totalTime = updateContext.Host.TotalTime;

        _memory = (float)(GC.GetTotalMemory(false) / (1024.0 * 1024.0));
        _gen0 = GC.CollectionCount(0);
        _gen1 = GC.CollectionCount(1);
        _gen2 = GC.CollectionCount(2);

        if (_wantToToggleDirtyRendering)
        {
            _wantToToggleDirtyRendering = false;
            updateContext.Host.UseDirtyRendering = !updateContext.Host.UseDirtyRendering;
        }
        if (_wantToToggleVSync)
        {
            _wantToToggleVSync = false;
            updateContext.Host.VSync = !updateContext.Host.VSync;
        }
        if (_wantToIncreaseTargetFps)
        {
            _wantToIncreaseTargetFps = false;
            updateContext.Host.FramesPerSecond += 5;
        }
        if (_wantToDecreaseTargetFps)
        {
            _wantToDecreaseTargetFps = false;
            updateContext.Host.FramesPerSecond -= 5;
        }
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        if (eventContext.MousePosition is { } point)
        {
            _mouseLastSeenAtX = point.X;
            _mouseLastSeenAtY = point.Y;
        }

        if (eventContext.Type == MouseEventType.ClickEvent && eventContext.Button == MouseButton.Left && eventContext.MousePosition is { } nnpoint)
        {
            if (_diReButtonBounds.Contains(nnpoint))
                _wantToToggleDirtyRendering = true;
            else if (_vSyncButtonBounds.Contains(nnpoint))
                _wantToToggleVSync = true;
            else if (_increaseFpsButtonBounds.Contains(nnpoint))
                _wantToIncreaseTargetFps = true;
            else if (_decreaseFpsButtonBounds.Contains(nnpoint))
                _wantToDecreaseTargetFps = true;
        }

        return false;
    }

    protected override void OnDispose()
    {
        _font.Dispose();
        _fgPaint.Dispose();
        _fgStrokePaint.Dispose();
        _fgThickPaint.Dispose();
        _bgPaint.Dispose();
        _redPaint.Dispose();
        _yellowPaint.Dispose();
        _greenPaint.Dispose();
        _pathBuilder.Dispose();
    }
}