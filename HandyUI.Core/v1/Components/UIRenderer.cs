using HandyUI.Core.v1.Base;
using HandyUI.Core.v1.Data;
using HandyUI.Core.v1.Enums;
using HandyUI.Core.v1.Interface;
using HandyUI.Core.v1.Theming;
using SkiaSharp;

namespace HandyUI.Core.v1.Components;

public class UIRenderer : IDisposable
{
    #region Properties

    public SKColor BackgroundColor
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            _backgroundColorCustom = value != ThemeManager.Instance.BackgroundPrimary;
        }
    } = ThemeManager.Instance.BackgroundPrimary;

    public Tweener Tweener { get; }

    #endregion

    #region Internal Fields

    private bool _backgroundColorCustom = false;

    private readonly List<IControl> _rootControls = [];
    private readonly List<IControl> _pendingRootControlsAdd = [];
    private readonly List<IControl> _pendingRootControlsRemove = [];

    private readonly Lock _rootControlsLock = new();
    private bool _isRootControlsOrderDirty = true;

    private int _framesToRender = 2;
    private bool _isDisposed;

    private double _totalTime;

    private IControl? _pressedControl;
    private IControl? _focusedControl;

    private readonly List<IModule> _modules = [];
    private readonly List<IModule> _pendingModulesAdd = [];
    private readonly List<IModule> _pendingModulesRemove = [];

    private readonly Lock _modulesLock = new();

    #endregion

    #region Events

    public event Action? WhenInvalidated;

    #endregion

    #region Constructor

    public UIRenderer(bool UseDirtyRendering)
    {
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;

        Tweener = new Tweener();
        _modules.Add(Tweener);
        Tweener.Start();
    }

    #endregion

    #region Methods

    public void Invalidate()
    {
        _framesToRender = 2;
        WhenInvalidated?.Invoke();
    }

    public void AddRootControl(IControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        lock (_pendingRootControlsAdd) _pendingRootControlsAdd.Add(control);
        Invalidate();
    }

    public void RemoveRootControl(IControl control)
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

    public bool IsPointOverAnyControl(SKPoint clientPosition)
    {
        lock (_rootControlsLock)
        {
            if (_isRootControlsOrderDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isRootControlsOrderDirty = false; }

            for (var i = _rootControls.Count - 1; i >= 0; i--)
            {
                if (HitTestRecursive(_rootControls[i], clientPosition) != null)
                    return true;
            }
        }

        return false;
    }

    #endregion

    #region Event Handling Methods

    public void ProcessMouseEvent(MouseEventContext context)
    {
        context = context with { Tweener = Tweener };

        var clientPosition = context.Position;

        IControl[] rootSnapshot;
        lock (_rootControlsLock)
        {
            if (_isRootControlsOrderDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isRootControlsOrderDirty = false; }
            rootSnapshot = [.. _rootControls];
        }

        IControl? hitControl = null;
        for (var i = rootSnapshot.Length - 1; i >= 0; i--)
        {
            hitControl = HitTestRecursive(rootSnapshot[i], clientPosition);
            if (hitControl != null) break;
        }

        if (context.Type == MouseEventType.MouseDown)
        {
            _pressedControl = hitControl;
            SetFocus(hitControl);
        }

        foreach (var root in rootSnapshot)
            DispatchMouseEventRecursive(root, context);

        if (context.Type == MouseEventType.MouseUp)
        {
            if (_pressedControl != null && _pressedControl == hitControl)
            {
                var localPos = GetLocalMousePosition(_pressedControl, clientPosition);
                _pressedControl.ProcessMouseEvent(new MouseEventContext(localPos, MouseEventType.Click, context.Button, context.WheelDelta, Tweener));
            }
            _pressedControl = null;
        }
    }

    public bool ProcessKeyEvent(KeyEventContext context)
    {
        context = context with { Tweener = Tweener };
        return _focusedControl?.IsVisible == true && _focusedControl.IsEnabled && _focusedControl.ProcessKeyEvent(context);
    }

    public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime)
    {
        ProcessPendingControls();

        lock (_rootControlsLock)
        {
            _totalTime += deltaTime;

            if (UseDirtyRendering && _framesToRender <= 0) return false;

            if (_isRootControlsOrderDirty) { _rootControls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex)); _isRootControlsOrderDirty = false; }
            canvas.Clear(BackgroundColor);

            var baseContext = new UpdateContext(
                Host,
                deltaTime,
                _totalTime,
                Tweener,
                cursorPosition,
                Host.DpiScale,
                Host.DisplayDist,
                Host.Orientation);

            foreach (var control in _rootControls)
                RenderRecursive(canvas, control, baseContext, cursorPosition);

            if (Host.UseDirtyRendering) _framesToRender--;

            return true;
        }
    }

    public bool UpdateModules(float deltaTime)
    {
        ProcessPendingModules();

        lock (_modulesLock)
        {
            if (Host.UseDirtyRendering && _framesToRender <= 0) return false;

            foreach (var module in _modules)
            {
                if (!module.IsEnabled) continue;

                module.OnUpdate(deltaTime);
            }

            return true;
        }
    }

    #endregion

    #region Internal Functions

    private void OnThemeChanged()
    {
        if (!_backgroundColorCustom)
        {
            BackgroundColor = ThemeManager.Instance.BackgroundPrimary;
        }
    }

    private void OnControlInvalidated() => Invalidate();

    private void ProcessPendingControls()
    {
        IControl[] toAdd, toRemove;
        lock (_pendingRootControlsAdd) { toAdd = [.. _pendingRootControlsAdd]; _pendingRootControlsAdd.Clear(); }
        lock (_pendingRootControlsRemove) { toRemove = [.. _pendingRootControlsRemove]; _pendingRootControlsRemove.Clear(); }

        if (toAdd.Length == 0 && toRemove.Length == 0) return;

        lock (_rootControlsLock)
        {
            foreach (var control in toAdd)
            {
                control.WhenFocusRequested += OnControlFocusRequested;
                control.WhenInvalidated += OnControlInvalidated;
                _rootControls.Add(control);
                _isRootControlsOrderDirty = true;
            }

            foreach (var control in toRemove)
            {
                if (!_rootControls.Remove(control)) continue;
                control.WhenInvalidated -= OnControlInvalidated;
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

    private static IControl? HitTestRecursive(IControl control, SKPoint cursorPosition, bool isMouseClipped = false)
    {
        if (!control.IsVisible || !control.IsEnabled) return null;

        var localCursor = GetLocalMousePosition(control, cursorPosition);
        isMouseClipped = isMouseClipped || (control.ScissoringEnabled && !control.Bounds.Contains(localCursor));

        if (isMouseClipped) return null;
        if (control is BaseControl baseCtrl) baseCtrl.EnsureChildrenSorted();

        for (var i = control.Children.Count - 1; i >= 0; i--)
        {
            var hit = HitTestRecursive(control.Children[i], cursorPosition, isMouseClipped);
            if (hit != null) return hit;
        }

        return control.DoesIntersect(localCursor) ? control : null;
    }

    private void DispatchMouseEventRecursive(IControl control, MouseEventContext context, bool isMouseClipped = false)
    {
        if (!control.IsVisible) return;

        var localPos = GetLocalMousePosition(control, context.Position);
        var clipForSubtree = isMouseClipped || (control.ScissoringEnabled && !control.Bounds.Contains(localPos));

        control.ProcessMouseEvent(context with { Position = localPos });

        foreach (var child in control.Children)
            DispatchMouseEventRecursive(child, context, clipForSubtree);
    }

    private void OnControlFocusRequested(IControl control) => SetFocus(control);

    private void SetFocus(IControl? target)
    {
        if (_focusedControl == target) return;
        _focusedControl?.IsFocused = false;
        _focusedControl = target;
        _focusedControl?.IsFocused = true;
    }

    private void RenderRecursive(SKCanvas canvas, IControl control, UpdateContext baseContext, SKPoint clientCursor, bool isMouseClipped = false)
    {
        if (!control.IsVisible) return;

        var localCursor = GetLocalMousePosition(control, clientCursor);
        var clipForSubtree = isMouseClipped || (control.ScissoringEnabled && !control.Bounds.Contains(localCursor));

        var saveCount = canvas.SaveCount;

        if (control.Opacity < 0.999f)
        {
            using var layerPaint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(Math.Clamp(control.Opacity, 0f, 1f) * 255)) };
            canvas.SaveLayer(layerPaint);
        }

        canvas.Save();
        if (control.ParentChildPositioningEnabled) canvas.Translate(control.Location.X, control.Location.Y);

        control.OnUpdate(baseContext with { MousePosition = localCursor });
        control.OnDraw(canvas);

        if (control.ScissoringEnabled)
        {
            canvas.Save();
            canvas.ClipRect(control.Bounds, SKClipOperation.Intersect, antialias: true);
        }

        if (control is BaseControl baseCtrl) baseCtrl.EnsureChildrenSorted();

        for (var i = 0; i < control.Children.Count; i++)
            RenderRecursive(canvas, control.Children[i], baseContext, clientCursor, clipForSubtree);

        canvas.RestoreToCount(saveCount);
    }

    private static SKPoint GetLocalMousePosition(IControl control, SKPoint globalPoint)
    {
        if (!control.ParentChildPositioningEnabled) return globalPoint;
        var absolutePos = GetAbsoluteLocation(control);
        return new SKPoint(globalPoint.X - absolutePos.X, globalPoint.Y - absolutePos.Y);
    }

    private static SKPoint GetAbsoluteLocation(IControl control)
    {
        float x = 0, y = 0;
        for (var current = control; current != null; current = current.Parent)
        {
            if (current.ParentChildPositioningEnabled) { x += current.Location.X; y += current.Location.Y; }
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

    private static void DisposeRecursively(IControl control)
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
                    control.WhenInvalidated -= OnControlInvalidated;
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

            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
        }

        _isDisposed = true;
    }

    #endregion
}