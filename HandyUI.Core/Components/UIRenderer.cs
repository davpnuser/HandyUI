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
                    RegisterControlEvents(control);

                    if (control.Parent == null && !_rootControls.Contains(control))
                    {
                        _rootControls.Add(control);
                    }
                }
                _isDirty = true;
            }

            if (toRemove.Count > 0)
            {
                foreach (var control in toRemove)
                {
                    UnregisterControlEvents(control);

                    if (_rootControls.Remove(control))
                    {
                        control.Dispose();
                    }
                }
            }
        }
    }

    private void RegisterControlEvents(IUIControl control)
    {
        control.FocusRequested += OnControlFocusRequested;
        control.ChildAdded += OnChildAdded;
        control.ChildRemoved += OnChildRemoved;

        foreach (var child in control.Children)
        {
            RegisterControlEvents(child);
        }
    }

    private void UnregisterControlEvents(IUIControl control)
    {
        control.FocusRequested -= OnControlFocusRequested;
        control.ChildAdded -= OnChildAdded;
        control.ChildRemoved -= OnChildRemoved;

        if (_focusedControl == control) _focusedControl = null;
        if (_pressedControl == control) _pressedControl = null;

        foreach (var child in control.Children)
        {
            UnregisterControlEvents(child);
        }
    }

    private void OnChildAdded(IUIControl child)
    {
        RegisterControlEvents(child);
    }

    private void OnChildRemoved(IUIControl child)
    {
        UnregisterControlEvents(child);
    }

    public void ProcessMouseEvent(MouseEventContext context)
    {
        IUIControl[] snapshot;

        lock (_controlsLock)
        {
            snapshot = [.. _rootControls];
        }

        IUIControl? hitTarget = null;

        var sortedRoots = snapshot.OrderByDescending(c => c.ZIndex);
        foreach (var root in sortedRoots)
        {
            var localPos = GetControlLocalPoint(root, context.ClientPosition);
            hitTarget = root.HitTest(localPos);

            if (hitTarget != null)
            {
                break;
            }
        }

        if (context.Type == MouseEventType.MouseDown)
        {
            _pressedControl = hitTarget;
            SetFocus(hitTarget);
        }

        if (hitTarget != null)
        {
            var hitLocalPos = GetControlLocalPoint(hitTarget, context.ClientPosition);
            var targetedContext = context with { ClientPosition = hitLocalPos };
            hitTarget.ProcessMouseEvent(targetedContext);
        }

        if (context.Type == MouseEventType.MouseUp)
        {
            if (_pressedControl != null)
            {
                var pressedLocalPos = GetControlLocalPoint(_pressedControl, context.ClientPosition);
                var upContext = context with { ClientPosition = pressedLocalPos };
                _pressedControl.ProcessMouseEvent(upContext);

                if (_pressedControl == hitTarget)
                {
                    var clickContext = context with { Type = MouseEventType.Click, ClientPosition = pressedLocalPos };
                    _pressedControl.ProcessMouseEvent(clickContext);
                }
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
                _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                _isDirty = false;
            }

            canvas.Clear(SKColors.White);

            foreach (var control in _rootControls)
            {
                RenderControlTree(canvas, control, deltaTime, cursorPosition);
            }
        }
    }

    private void RenderControlTree(SKCanvas canvas, IUIControl control, float deltaTime, SKPoint currentCursorPos)
    {
        if (!control.IsVisible) return;

        canvas.Save();
        canvas.Translate(control.Location.X, control.Location.Y);

        var localCursor = new SKPoint(currentCursorPos.X - control.Location.X, currentCursorPos.Y - control.Location.Y);
        control.Update(deltaTime, localCursor);

        control.Draw(canvas);

        if (control.ClipChildren)
        {
            var clipRect = SKRect.Create(0, 0, control.Width, control.Height);
            canvas.ClipRect(clipRect, SKClipOperation.Intersect, true);
        }

        var sortedChildren = control.Children.OrderBy(c => c.ZIndex);
        foreach (var child in sortedChildren)
        {
            RenderControlTree(canvas, child, deltaTime, localCursor);
        }

        canvas.Restore();
    }

    private static SKPoint GetControlLocalPoint(IUIControl control, SKPoint screenPoint)
    {
        var current = control;
        var accumulatedX = 0f;
        var accumulatedY = 0f;

        while (current != null)
        {
            accumulatedX += current.Location.X;
            accumulatedY += current.Location.Y;
            current = current.Parent;
        }

        return new SKPoint(screenPoint.X - accumulatedX, screenPoint.Y - accumulatedY);
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
                foreach (var control in _rootControls)
                {
                    UnregisterControlEvents(control);
                    control.Dispose();
                }

                _rootControls.Clear();
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