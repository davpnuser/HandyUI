using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.Core.Components;

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

            //_backgroundColorCustom = value != ThemeManager.Instance.BackgroundPrimary;
        }
    } = SKColors.White;

    public TweenManager TweenManager { get; }
    public ThemeManager ThemeManager { get; }

    #endregion

    #region Internal Fields

    private bool _backgroundColorCustom = false;

    private readonly List<IControl> _controls = [];
    private readonly List<IControl> _pendingControlsAdd = [];
    private readonly List<IControl> _pendingControlsRemove = [];

    private readonly Lock _controlsLock = new();
    private bool _isControlsOrderDirty = true;

    private int _framesToRender = 2;
    private bool _isDisposed;

    private readonly double _totalTime;

    private IControl? _pressedControl;
    private IControl? _focusedControl;

    private readonly List<IModule> _modules = [];
    private readonly List<IModule> _pendingModulesAdd = [];
    private readonly List<IModule> _pendingModulesRemove = [];

    private readonly Lock _modulesLock = new();

    private readonly IHostWindow _hostWindow;
    private readonly GRContext _drawingContext;

    #endregion

    #region Events

    public event Action? WhenInvalidated;

    #endregion

    #region Constructor

    public UIRenderer(IHostWindow host)
    {
        _hostWindow = host;
        _drawingContext = host.DrawingContext;

        TweenManager = new TweenManager();
        _modules.Add(TweenManager);
        TweenManager.Start();

        ThemeManager = new ThemeManager();

        host.TweenManager = TweenManager;
        host.ThemeManager = ThemeManager;

        host.ThemeManagerReady();
    }

    #endregion

    #region Methods

    public void Invalidate()
    {
        _framesToRender = 2;
        WhenInvalidated?.Invoke();
    }

    #region Event Processing

    public void ProcessMouseEvent(MouseEventContext context)
    {
        if (context.MousePosition is not { } clientPosition)
            return;

        IControl[] rootSnapshot;
        lock (_controlsLock)
        {
            if (_isControlsOrderDirty)
            {
                _controls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                _isControlsOrderDirty = false;
            }

            rootSnapshot = [.. _controls];
        }

        IControl? hitControl = null;
        for (var i = rootSnapshot.Length - 1; i >= 0; i--)
        {
            hitControl = HitTestRecursive(rootSnapshot[i], clientPosition);
            if (hitControl != null) break;
        }

        if (context.Type == MouseEventType.MouseDownEvent)
        {
            _pressedControl = hitControl;
            SetFocus(hitControl);
        }

        foreach (var root in rootSnapshot) DispatchMouseEventRecursive(root, context);

        if (context.Type == MouseEventType.MouseUpEvent)
        {
            if (_pressedControl != null && _pressedControl == hitControl)
            {
                var localPos = _pressedControl.GetLocalPosition(clientPosition);
                _pressedControl.ProcessMouseEvent(context with { Type = MouseEventType.ClickEvent, MousePosition = localPos });
            }

            _pressedControl = null;
        }
    }

    public bool ProcessKeyEvent(KeyEventContext context)
        => _focusedControl?.Visible == true && _focusedControl.Enabled && _focusedControl.ProcessKeyEvent(context);

    #endregion

    #region Controls

    #region Public-sided

    public void AddControl(IControl control)
    {
        lock (_pendingControlsAdd) _pendingControlsAdd.Add(control);
        Invalidate();
    }

    public void RemoveControl(IControl control)
    {
        lock (_pendingControlsRemove) _pendingControlsRemove.Add(control);
        Invalidate();
    }

    public bool RenderControls(SKCanvas canvas, SKPoint cursorPosition, float deltaTime)
    {
        ProcessPendingControls();

        lock (_controlsLock)
        {
            if (_hostWindow.UseDirtyRendering && _framesToRender <= 0)
                return false;

            if (_isControlsOrderDirty)
            {
                _controls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                _isControlsOrderDirty = false;
            }

            canvas.Clear(BackgroundColor);

            foreach (var control in _controls)
                RenderRecursive(canvas, control, cursorPosition, deltaTime);

            if (_hostWindow.UseDirtyRendering)
                _framesToRender--;

            return true;
        }
    }

    public bool IsPointOverAnyControl(SKPoint clientPosition)
    {
        lock (_controlsLock)
        {
            if (_isControlsOrderDirty)
            {
                _controls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                _isControlsOrderDirty = false;
            }

            for (var i = _controls.Count - 1; i >= 0; i--)
            {
                if (HitTestRecursive(_controls[i], clientPosition) != null)
                    return true;
            }
        }

        return false;
    }

    #endregion

    #region Private-sided

    private void ProcessPendingControls()
    {
        IControl[] toAdd, toRemove;

        lock (_pendingControlsAdd)
        {
            toAdd = [.. _pendingControlsAdd];
            _pendingControlsAdd.Clear();
        }

        lock (_pendingControlsRemove)
        {
            toRemove = [.. _pendingControlsRemove];
            _pendingControlsRemove.Clear();
        }

        if (toAdd.Length == 0 && toRemove.Length == 0) return;

        lock (_controlsLock)
        {
            foreach (var control in toAdd)
            {
                control.WhenFocusRequested += SetFocus;
                control.WhenInvalidated += Invalidate;

                _controls.Add(control);
                _isControlsOrderDirty = true;
            }

            foreach (var control in toRemove)
            {
                if (!_controls.Remove(control))
                    continue;

                control.WhenInvalidated -= Invalidate;

                if (_focusedControl == control)
                    _focusedControl = null;

                if (_pressedControl == control)
                    _pressedControl = null;

                control.Dispose();
            }
        }

        Invalidate();
    }

    private void SetFocus(IControl? control)
    {
        if (_focusedControl == control)
            return;

        _focusedControl?.IsFocused = false;
        _focusedControl = control;
        _focusedControl?.IsFocused = true;
    }

    private IControl? HitTestRecursive(IControl control, SKPoint cursorPosition, bool isMouseClipped = false)
    {
        if (!control.Visible || !control.Enabled)
            return null;

        var localCursorPosition = control.GetLocalPosition(cursorPosition);
        isMouseClipped = isMouseClipped || (control.ClipChildren && !control.Bounds.Contains(localCursorPosition));

        if (isMouseClipped)
            return null;

        if (control is BaseControl baseCtrl)
            baseCtrl.EnsureChildrenSorted();

        for (var i = control.Children.Count - 1; i >= 0; i--)
        {
            var hit = HitTestRecursive(control.Children[i], cursorPosition, isMouseClipped);

            if (hit != null)
                return hit;
        }

        return control.DoesIntersect(localCursorPosition) ? control : null;
    }

    private void DispatchMouseEventRecursive(IControl control, MouseEventContext context, bool isMouseClipped = false)
    {
        if (!control.Visible)
            return;

        if (context.MousePosition is not { } mousePosition)
            return;

        var localMousePosition = control.GetLocalPosition(mousePosition);
        var clipForSubtree = isMouseClipped || (control.ClipChildren && !control.Bounds.Contains(localMousePosition));

        var effectivePos = (clipForSubtree && control != _pressedControl) ? (SKPoint?)null : localMousePosition;

        control.ProcessMouseEvent(context with { MousePosition = effectivePos });

        foreach (var child in control.Children)
            DispatchMouseEventRecursive(child, context, clipForSubtree);
    }

    private void RenderRecursive(SKCanvas canvas, IControl control, SKPoint cursorPosition, float deltaTime, bool isMouseClipped = false)
    {
        if (!control.Visible)
            return;

        var localMousePosition = control.GetLocalPosition(cursorPosition);
        var shouldClip = isMouseClipped || (control.ClipChildren && !control.Bounds.Contains(localMousePosition));

        var saveCount = canvas.SaveCount;

        if (control.Opacity < 0.999f)
        {
            using var layerPaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha((byte)(Math.Clamp(control.Opacity, 0f, 1f) * 255))
            };

            canvas.SaveLayer(layerPaint);
        }

        canvas.Save();
        canvas.Translate(control.Location.X, control.Location.Y);

        var updateContext = new UpdateContext(TweenManager, _hostWindow, deltaTime);

        control.OnUpdate(updateContext);
        control.OnDraw(canvas);

        if (control.ClipChildren)
        {
            canvas.Save();
            canvas.ClipRect(control.Bounds, SKClipOperation.Intersect, antialias: true);
        }

        if (control is BaseControl baseCtrl)
            baseCtrl.EnsureChildrenSorted();

        for (var i = 0; i < control.Children.Count; i++)
            RenderRecursive(canvas, control.Children[i], cursorPosition, deltaTime, shouldClip);

        canvas.RestoreToCount(saveCount);
    }

    #endregion

    #endregion

    #region Modules

    #region Public-sided

    public void AddModule(IModule module)
    {
        lock (_pendingModulesAdd) _pendingModulesAdd.Add(module);
        Invalidate();
    }

    public void RemoveModule(IModule module)
    {
        lock (_pendingModulesRemove) _pendingModulesRemove.Add(module);
        Invalidate();
    }

    #endregion

    #region Private-sided

    private void ProcessPendingModules()
    {
        IModule[] toAdd, toRemove;
        lock (_pendingModulesAdd)
        {
            toAdd = [.. _pendingModulesAdd];
            _pendingModulesAdd.Clear();
        }

        lock (_pendingModulesRemove)
        {
            toRemove = [.. _pendingModulesRemove];
            _pendingModulesRemove.Clear();
        }

        if (toAdd.Length == 0 && toRemove.Length == 0)
            return;

        lock (_modulesLock)
        {
            foreach (var module in toAdd)
                _modules.Add(module);

            foreach (var module in toRemove)
            {
                if (_modules.Remove(module) && module is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }

    public bool UpdateModules(float deltaTime)
    {
        ProcessPendingModules();

        lock (_modulesLock)
        {
            if (_hostWindow.UseDirtyRendering && _framesToRender <= 0) return false;

            foreach (var module in _modules)
            {
                if (!module.Enabled)
                    continue;

                var updateContext = new UpdateContext(TweenManager, _hostWindow, deltaTime);
                module.OnUpdate(updateContext);
            }

            return true;
        }
    }

    #endregion

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
            lock (_controlsLock)
            {
                foreach (var control in _controls)
                {
                    control.WhenInvalidated -= Invalidate;
                    DisposeRecursively(control);
                }

                _controls.Clear();
            }
            lock (_pendingControlsAdd)
            {
                foreach (var control in _pendingControlsAdd)
                    control.Dispose();
                _pendingControlsAdd.Clear();
            }

            lock (_pendingControlsRemove)
                _pendingControlsRemove.Clear();

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
            lock (_pendingModulesAdd)
            {
                foreach (var module in _pendingModulesAdd)
                    module.Dispose();

                _pendingModulesAdd.Clear();
            }

            lock (_pendingModulesRemove)
                _pendingModulesRemove.Clear();

            //ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
        }

        _isDisposed = true;
    }

    #endregion

    #endregion
}