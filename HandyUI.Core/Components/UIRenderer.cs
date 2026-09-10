using HandyUI.Core.Classes.Records;
using HandyUI.Core.Interfaces;
using SkiaSharp;
using System.Diagnostics;

namespace HandyUI.Core.Components;

public class UIRenderer : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private double _lastFrameTime;

    private readonly List<IUIControl> _controls = [];
    private readonly List<IUIControl> _pendingAdd = [];
    private readonly List<IUIControl> _pendingRemove = [];

    private readonly Lock _controlsLock = new();
    private bool _isDirty = true;
    private bool _isDisposed;

    private IUIControl? _pressedControl;
    private IUIControl? _focusedControl;

    public void AddControl(IUIControl control)
    {
        ArgumentNullException.ThrowIfNull(control);

        lock (_pendingAdd)
        {
            _pendingAdd.Add(control);
        }
    }

    public void RemoveControl(IUIControl control)
    {
        ArgumentNullException.ThrowIfNull(control);

        lock (_pendingRemove)
        {
            _pendingRemove.Add(control);
        }
    }

    private void ProcessPendingControls()
    {
        List<IUIControl> toAdd = [];
        List<IUIControl> toRemove = [];

        lock (_pendingAdd)
        {
            if (_pendingAdd.Count > 0)
            {
                toAdd.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }
        }

        lock (_pendingRemove)
        {
            if (_pendingRemove.Count > 0)
            {
                toRemove.AddRange(_pendingRemove);
                _pendingRemove.Clear();
            }
        }

        if (toAdd.Count == 0 && toRemove.Count == 0) return;

        lock (_controlsLock)
        {
            if (toAdd.Count > 0)
            {
                foreach (var control in toAdd)
                {
                    control.FocusRequested += OnControlFocusRequested;
                }

                _controls.AddRange(toAdd);
                _isDirty = true;
            }

            if (toRemove.Count > 0)
            {
                foreach (var control in toRemove)
                {
                    if (_controls.Remove(control))
                    {
                        if (_focusedControl == control) _focusedControl = null;
                        if (_pressedControl == control) _pressedControl = null;
                        control.Dispose();
                    }
                }
            }
        }
    }

    public void ProcessMouseEvent(MouseEventContext context)
    {
        IUIControl[] snapshot;

        lock (_controlsLock)
        {
            snapshot = [.. _controls];
        }

        IUIControl? hitControl = null;

        for (var i = snapshot.Length - 1; i >= 0; i--)
        {
            var control = snapshot[i];

            if (control.IsVisible && control.IsEnabled)
            {
                var localPos = GetLocalMousePosition(control, context.ClientPosition);

                if (control.Intersects(localPos))
                {
                    hitControl = control;
                    break;
                }
            }
        }

        if (context.Type == MouseEventType.MouseDown)
        {
            _pressedControl = hitControl;
            SetFocus(hitControl);
        }

        foreach (var control in snapshot)
        {
            var localPos = GetLocalMousePosition(control, context.ClientPosition);
            var localContext = context with { ClientPosition = localPos };

            control.ProcessMouseEvent(localContext);
        }

        if (context.Type == MouseEventType.MouseUp)
        {
            if (_pressedControl != null && _pressedControl == hitControl)
            {
                var localPos = GetLocalMousePosition(_pressedControl, context.ClientPosition);
                var clickContext = context with { Type = MouseEventType.Click, ClientPosition = localPos };
                _pressedControl.ProcessMouseEvent(clickContext);
            }

            _pressedControl = null;
        }
    }

    public bool ProcessKeyEvent(KeyEventContext context)
    {
        return _focusedControl != null && _focusedControl.IsVisible && _focusedControl.IsEnabled && _focusedControl.ProcessKeyEvent(context);
    }

    private void OnControlFocusRequested(IUIControl control)
    {
        SetFocus(control);
    }

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
            if (_isDirty)
            {
                _controls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                _isDirty = false;
            }

            canvas.Clear(SKColors.White);

            foreach (var control in _controls)
            {
                if (!control.IsVisible) continue;

                canvas.Save();

                if (control.RetainedModePositioning)
                    canvas.Translate(control.Location.X, control.Location.Y);

                var localCursor = GetLocalMousePosition(control, cursorPosition);
                control.Update(deltaTime, localCursor);

                control.Draw(canvas);

                canvas.Restore();
            }
        }
    }

    private static SKPoint GetLocalMousePosition(IUIControl control, SKPoint globalPoint)
    {
        return !control.RetainedModePositioning
            ? globalPoint
            : new SKPoint(
            globalPoint.X - (int)control.Location.X,
            globalPoint.Y - (int)control.Location.Y
        );
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
            lock (_controlsLock)
            {
                foreach (var control in _controls)
                    control.Dispose();

                _controls.Clear();
            }

            lock (_pendingAdd)
            {
                foreach (var control in _pendingAdd)
                    control.Dispose();

                _pendingAdd.Clear();
            }

            lock (_pendingRemove)
            {
                _pendingRemove.Clear();
            }

            _focusedControl = null;
            _pressedControl = null;
        }

        _isDisposed = true;
    }
}