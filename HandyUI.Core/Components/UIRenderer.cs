using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Interfaces;
using SkiaSharp;
using System.Diagnostics;

namespace HandyUI.Core.Components;

public class UIRenderer : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private double _lastFrameTime;

    private readonly List<IUIControl> _rootControls = [];
    private readonly List<IUIControl> _pendingAdd = [];
    private readonly List<IUIControl> _pendingRemove = [];

    private readonly Lock _controlsLock = new();
    private bool _isDirty = true;
    private bool _isDisposed;

    private IUIControl? _pressedControl;
    private IUIControl? _focusedControl;

    public void AddRootControl(IUIControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        lock (_pendingAdd) _pendingAdd.Add(control);
    }

    public void RemoveRootControl(IUIControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        lock (_pendingRemove) _pendingRemove.Add(control);
    }

    private void ProcessPendingControls()
    {
        IUIControl[] toAdd, toRemove;
        lock (_pendingAdd) { toAdd = [.. _pendingAdd]; _pendingAdd.Clear(); }
        lock (_pendingRemove) { toRemove = [.. _pendingRemove]; _pendingRemove.Clear(); }

        if (toAdd.Length == 0 && toRemove.Length == 0) return;

        lock (_controlsLock)
        {
            foreach (var control in toAdd)
            {
                control.FocusRequested += OnControlFocusRequested;
                _rootControls.Add(control);
                _isDirty = true;
            }

            foreach (var control in toRemove)
            {
                if (!_rootControls.Remove(control)) continue;
                if (_focusedControl == control) _focusedControl = null;
                if (_pressedControl == control) _pressedControl = null;
                control.Dispose();
            }
        }
    }

    public void ProcessMouseEvent(MouseEventContext context)
    {
        IUIControl[] rootSnapshot;
        lock (_controlsLock)
        {
            if (_isDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isDirty = false; }
            rootSnapshot = [.. _rootControls];
        }

        IUIControl? hitControl = null;
        for (var i = rootSnapshot.Length - 1; i >= 0; i--)
        {
            hitControl = HitTestRecursive(rootSnapshot[i], context.ClientPosition);
            if (hitControl != null) break;
        }

        if (context.Type == MouseEventType.MouseDown)
        {
            _pressedControl = hitControl;
            SetFocus(hitControl);
        }

        foreach (var root in rootSnapshot) DispatchMouseEventRecursive(root, context);

        if (context.Type == MouseEventType.MouseUp)
        {
            if (_pressedControl != null && _pressedControl == hitControl)
            {
                var localPos = GetLocalMousePosition(_pressedControl, context.ClientPosition);
                _pressedControl.ProcessMouseEvent(context with { Type = MouseEventType.Click, ClientPosition = localPos });
            }
            _pressedControl = null;
        }
    }

    private static IUIControl? HitTestRecursive(IUIControl control, SKPoint cursorPosition, bool isMouseClipped = false)
    {
        if (!control.IsVisible || !control.IsEnabled) return null;

        var localCursor = GetLocalMousePosition(control, cursorPosition);
        isMouseClipped = isMouseClipped || (control.ScissoringEnabled && !control.Bounds.Contains(localCursor));

        if (isMouseClipped) return null;
        if (control is UIControlBase baseCtrl) baseCtrl.EnsureChildrenSorted();

        for (var i = control.Children.Count - 1; i >= 0; i--)
        {
            var hit = HitTestRecursive(control.Children[i], cursorPosition, isMouseClipped);
            if (hit != null) return hit;
        }

        return control.Intersects(localCursor) ? control : null;
    }

    private void DispatchMouseEventRecursive(IUIControl control, MouseEventContext context, bool isMouseClipped = false)
    {
        if (!control.IsVisible) return;

        var localPos = GetLocalMousePosition(control, context.ClientPosition);
        var clipForSubtree = isMouseClipped || (control.ScissoringEnabled && !control.Bounds.Contains(localPos));
        var effectivePos = (clipForSubtree && control != _pressedControl) ? new SKPoint(-99999f, -99999f) : localPos;

        control.ProcessMouseEvent(context with { ClientPosition = effectivePos });

        foreach (var child in control.Children)
            DispatchMouseEventRecursive(child, context, clipForSubtree);
    }

    public bool ProcessKeyEvent(KeyEventContext context) =>
        _focusedControl?.IsVisible == true && _focusedControl.IsEnabled && _focusedControl.ProcessKeyEvent(context);

    private void OnControlFocusRequested(IUIControl control) => SetFocus(control);

    private void SetFocus(IUIControl? target)
    {
        if (_focusedControl == target) return;
        _focusedControl?.IsFocused = false;
        _focusedControl = target;
        _focusedControl?.IsFocused = true;
    }

    public void RenderControls(SKCanvas canvas, SKPoint cursorPosition)
    {
        var currentTime = _stopwatch.Elapsed.TotalSeconds;
        var deltaTime = (float)(currentTime - _lastFrameTime);
        _lastFrameTime = currentTime;

        ProcessPendingControls();

        lock (_controlsLock)
        {
            if (_isDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isDirty = false; }
            canvas.Clear(SKColors.White);

            foreach (var control in _rootControls)
                RenderRecursive(canvas, control, cursorPosition, deltaTime);
        }
    }

    private void RenderRecursive(SKCanvas canvas, IUIControl control, SKPoint cursorPosition, float deltaTime, bool isMouseClipped = false)
    {
        if (!control.IsVisible) return;

        var localCursor = GetLocalMousePosition(control, cursorPosition);
        var clipForSubtree = isMouseClipped || (control.ScissoringEnabled && !control.Bounds.Contains(localCursor));
        var effectiveCursor = (clipForSubtree && control != _pressedControl) ? new SKPoint(-99999f, -99999f) : localCursor;

        var saveCount = canvas.SaveCount;

        if (control.Opacity < 0.999f)
        {
            using var layerPaint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(Math.Clamp(control.Opacity, 0f, 1f) * 255)) };
            canvas.SaveLayer(layerPaint);
        }

        canvas.Save();
        if (control.RetainedModePositioning) canvas.Translate(control.Location.X, control.Location.Y);

        control.Update(deltaTime, effectiveCursor);
        control.Draw(canvas);

        if (control.ScissoringEnabled)
        {
            canvas.Save();
            canvas.ClipRect(control.Bounds, SKClipOperation.Intersect, antialias: true);
        }

        if (control is UIControlBase baseCtrl) baseCtrl.EnsureChildrenSorted();

        for (var i = 0; i < control.Children.Count; i++)
            RenderRecursive(canvas, control.Children[i], cursorPosition, deltaTime, clipForSubtree);

        canvas.RestoreToCount(saveCount);
    }

    private static SKPoint GetLocalMousePosition(IUIControl control, SKPoint globalPoint)
    {
        if (!control.RetainedModePositioning) return globalPoint;
        var absolutePos = GetAbsoluteLocation(control);
        return new SKPoint(globalPoint.X - absolutePos.X, globalPoint.Y - absolutePos.Y);
    }

    private static SKPoint GetAbsoluteLocation(IUIControl control)
    {
        float x = 0, y = 0;
        for (var current = control; current != null; current = current.Parent)
        {
            if (current.RetainedModePositioning) { x += current.Location.X; y += current.Location.Y; }
        }
        return new SKPoint(x, y);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            lock (_controlsLock) { foreach (var control in _rootControls) control.Dispose(); _rootControls.Clear(); }
            lock (_pendingAdd) { foreach (var control in _pendingAdd) control.Dispose(); _pendingAdd.Clear(); }
            lock (_pendingRemove) _pendingRemove.Clear();
            _focusedControl = null;
            _pressedControl = null;
        }

        _isDisposed = true;
    }
}