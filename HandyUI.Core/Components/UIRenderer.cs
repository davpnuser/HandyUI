using HandyUI.Core.Classes.Base;
using HandyUI.Core.Classes.Records;
using HandyUI.Core.Classes.Themes;
using HandyUI.Core.Interfaces;
using SkiaSharp;

namespace HandyUI.Core.Components;

public class UIRenderer(bool useDirtyRendering = true) : IDisposable
{
    #region Properties

    public SKColor BackgroundColor { get; set; } = DefaultTheme.GetTheme().DarkerBackgroundColor;

    #endregion

    #region Internal Fields

    private readonly List<IUIControl> _rootControls = [];
    private readonly List<IUIControl> _pendingRootControlsAdd = [];
    private readonly List<IUIControl> _pendingRootControlsRemove = [];

    private readonly Lock _rootControlsLock = new();
    private bool _isRootControlsOrderDirty = true;

    private int _framesToRender = 2;
    private bool _isDisposed;

    private readonly bool _useDirtyRendering = useDirtyRendering;

    private IUIControl? _pressedControl;
    private IUIControl? _focusedControl;

    private readonly List<IModule> _modules = [];
    private readonly List<IModule> _pendingModulesAdd = [];
    private readonly List<IModule> _pendingModulesRemove = [];

    private readonly Lock _modulesLock = new();

    #endregion

    #region Methods

    public void Invalidate()
    {
        _framesToRender = 2;
    }

    public void AddRootControl(IUIControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        lock (_pendingRootControlsAdd) _pendingRootControlsAdd.Add(control);
        Invalidate();
    }

    public void RemoveRootControl(IUIControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        lock (_pendingRootControlsRemove) _pendingRootControlsRemove.Add(control);
        Invalidate();
    }

    public void AddModule(IModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        lock (_pendingModulesAdd) _pendingModulesAdd.Add(module);
        Invalidate();
    }

    public void RemoveModule(IModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        lock (_pendingModulesRemove) _pendingModulesRemove.Add(module);
        Invalidate();
    }

    #endregion

    #region Event Handling Methods

    public void ProcessMouseEvent(MouseEventContext context)
    {
        IUIControl[] rootSnapshot;
        lock (_rootControlsLock)
        {
            if (_isRootControlsOrderDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isRootControlsOrderDirty = false; }
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

    public bool ProcessKeyEvent(KeyEventContext context)
    {
        return _focusedControl?.IsVisible == true && _focusedControl.IsEnabled && _focusedControl.ProcessKeyEvent(context);
    }

    public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime)
    {
        ProcessPendingControls();

        lock (_rootControlsLock)
        {
            if (_useDirtyRendering && _framesToRender <= 0) return false;

            if (_isRootControlsOrderDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isRootControlsOrderDirty = false; }
            canvas.Clear(BackgroundColor);

            foreach (var control in _rootControls)
                RenderRecursive(canvas, control, cursorPosition, deltaTime);

            if (_useDirtyRendering) _framesToRender--;

            return true;
        }
    }

    public bool UpdateModules(float deltaTime)
    {
        ProcessPendingModules();

        lock (_modulesLock)
        {
            if (_useDirtyRendering && _framesToRender <= 0) return false;

            foreach (var module in _modules)
            {
                if (!module.IsEnabled) continue;

                module.Update(deltaTime);
            }

            return true;
        }
    }

    #endregion

    #region Internal Functions

    private void OnControlInvalidated()
    {
        Invalidate();
    }

    private void ProcessPendingControls()
    {
        IUIControl[] toAdd, toRemove;
        lock (_pendingRootControlsAdd) { toAdd = [.. _pendingRootControlsAdd]; _pendingRootControlsAdd.Clear(); }
        lock (_pendingRootControlsRemove) { toRemove = [.. _pendingRootControlsRemove]; _pendingRootControlsRemove.Clear(); }

        if (toAdd.Length == 0 && toRemove.Length == 0) return;

        lock (_rootControlsLock)
        {
            foreach (var control in toAdd)
            {
                control.FocusRequested += OnControlFocusRequested;
                control.Invalidated += OnControlInvalidated;
                _rootControls.Add(control);
                _isRootControlsOrderDirty = true;
            }

            foreach (var control in toRemove)
            {
                if (!_rootControls.Remove(control)) continue;
                control.Invalidated -= OnControlInvalidated;
                if (_focusedControl == control) _focusedControl = null;
                if (_pressedControl == control) _pressedControl = null;
                control.Dispose();
            }
        }

        Invalidate();
    }

    private void ProcessPendingModules()
    {
        IModule[] toAdd, toRemove;
        lock (_pendingModulesAdd) { toAdd = [.. _pendingModulesAdd]; _pendingModulesAdd.Clear(); }
        lock (_pendingModulesRemove) { toRemove = [.. _pendingModulesRemove]; _pendingModulesRemove.Clear(); }

        if (toAdd.Length == 0 && toRemove.Length == 0) return;

        lock (_modulesLock)
        {
            foreach (var module in toAdd)
            {
                _modules.Add(module);
            }

            foreach (var module in toRemove)
            {
                if (_modules.Remove(module) && module is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
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
        if (control.InheritedPositioningEnabled) canvas.Translate(control.Location.X, control.Location.Y);

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
        if (!control.InheritedPositioningEnabled) return globalPoint;
        var absolutePos = GetAbsoluteLocation(control);
        return new SKPoint(globalPoint.X - absolutePos.X, globalPoint.Y - absolutePos.Y);
    }

    private static SKPoint GetAbsoluteLocation(IUIControl control)
    {
        float x = 0, y = 0;
        for (var current = control; current != null; current = current.Parent)
        {
            if (current.InheritedPositioningEnabled) { x += current.Location.X; y += current.Location.Y; }
        }
        return new SKPoint(x, y);
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static void DisposeRecursively(IUIControl control)
    {
        foreach (var childControl in control.Children)
        {
            DisposeRecursively(childControl);
        }

        control.Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            lock (_rootControlsLock)
            {
                foreach (var control in _rootControls)
                {
                    control.Invalidated -= OnControlInvalidated;
                    DisposeRecursively(control);
                }

                _rootControls.Clear();
            }
            lock (_pendingRootControlsAdd) { foreach (var control in _pendingRootControlsAdd) control.Dispose(); _pendingRootControlsAdd.Clear(); }
            lock (_pendingRootControlsRemove) _pendingRootControlsRemove.Clear();
            _focusedControl = null;
            _pressedControl = null;

            lock (_modulesLock)
            {
                foreach (var module in _modules)
                {
                    if (module is IDisposable disposable) disposable.Dispose();
                }

                _modules.Clear();
            }
            lock (_pendingModulesAdd) { foreach (var module in _pendingModulesAdd) module.Dispose(); _pendingModulesAdd.Clear(); }
            lock (_pendingModulesRemove) _pendingModulesRemove.Clear();
        }

        _isDisposed = true;
    }

    #endregion
}