using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Data;
using HandyUI.Core.Data.Enums;
using HandyUI.Core.Helpers;
using HandyUI.Core.Helpers.Controls;
using SkiaSharp;
using System.Diagnostics;
using System.Reflection;

namespace HandyUI.Core.Components;

public class UIRenderer : IDisposable
{
    #region Properties

    public SKColor BackgroundColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _backgroundColorCustom = value != ThemeManager.Instance.BackgroundPrimary;
        }
    } = SKColors.White;

    public TweenManager TweenManager { get; private set; }

    public IControl? FocusedControl { get; private set; }

    public bool UseProfiler
    {
        get => _isDebuggable;
        set
        {
            _isDebuggable = value;

            if (value && _profilerControl is null)
                CreateProfiler();
            else if (!value && _profilerControl is not null)
                DestroyProfiler();
        }
    }

    public double TotalTime { get; private set; }

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

    private IControl? _pressedControl;
    private bool _focusFromKeyboard;

    private readonly List<IModule> _modules = [];
    private readonly List<IModule> _pendingModulesAdd = [];
    private readonly List<IModule> _pendingModulesRemove = [];

    private readonly Lock _modulesLock = new();

    private readonly IHostWindow _hostWindow;

    private ProfilerControl? _profilerControl = null;
    private bool _isDebuggable;

    #endregion

    #region Constructor

    public UIRenderer(IHostWindow host)
    {
        _hostWindow = host;

        TweenManager = new TweenManager();
        _modules.Add(TweenManager);

        host.TweenManager = TweenManager;

        BackgroundColor = ThemeManager.Instance.BackgroundPrimary;
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;

        var assembly = Assembly.GetEntryAssembly();

        if (assembly is null)
            _isDebuggable = false;

        var debugAttribute = assembly?.GetCustomAttribute<DebuggableAttribute>();
        _isDebuggable = debugAttribute?.IsJITOptimizerDisabled ?? false;

        UseProfiler = _isDebuggable;
    }

    #endregion

    #region Methods

    public void Invalidate()
    {
        Volatile.Write(ref _framesToRender, 2);
        _hostWindow.Invalidate();
    }

    private void OnThemeChanged()
    {
        if (!_backgroundColorCustom)
            BackgroundColor = ThemeManager.Instance.BackgroundPrimary;

        Invalidate();
    }

    private void CreateProfiler()
    {
        if (_profilerControl is not null)
            return;

        var profilerControl = new ProfilerControl()
                .WithAddToRenderer(this);

        profilerControl.Enabled = false;
        profilerControl.Visible = false;

        _profilerControl = profilerControl;
    }

    private void DestroyProfiler()
    {
        if (_profilerControl is null)
            return;

        _profilerControl.Dispose();
        RemoveControl(_profilerControl);

        _profilerControl = null;
    }

    #region Focus

    public void FocusNext()
    {
        MoveFocus(+1);
    }

    public void FocusPrevious()
    {
        MoveFocus(-1);
    }

    private void MoveFocus(int direction)
    {
        List<IControl> focusables;

        lock (_controlsLock)
        {
            if (_isControlsOrderDirty)
            {
                _controls.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                _isControlsOrderDirty = false;
            }

            focusables = [];
            CollectFocusables(_controls, focusables);
        }

        if (focusables.Count == 0)
        {
            SetFocus(null, fromKeyboard: true);
            return;
        }

        if (focusables.Count == 1 && focusables[0] == FocusedControl)
            return;

        var currentIndex = FocusedControl != null ? focusables.IndexOf(FocusedControl) : -1;
        var nextIndex = (((currentIndex + direction) % focusables.Count) + focusables.Count) % focusables.Count;

        SetFocus(focusables[nextIndex], fromKeyboard: true);
    }

    private static void CollectFocusables(IEnumerable<IControl> controls, List<IControl> result)
    {
        foreach (var control in controls)
        {
            if (!control.Visible || !control.Enabled) continue;

            if (control is BaseControl baseControl)
                baseControl.EnsureChildrenSorted();

            if (control.Focusable)
                result.Add(control);

            CollectFocusables(control.Children, result);
        }
    }

    private void SetFocus(IControl? control, bool fromKeyboard = false)
    {
        if (FocusedControl == control)
        {
            if (_focusFromKeyboard != fromKeyboard)
            {
                _focusFromKeyboard = fromKeyboard;
                Invalidate();
            }
            return;
        }

        FocusedControl?.IsFocused = false;
        FocusedControl = control;
        FocusedControl?.IsFocused = true;
        _focusFromKeyboard = fromKeyboard;
        Invalidate();
    }

    #endregion

    #region Event Processing

    public void ProcessMouseEvent(MouseEventContext context)
    {
        if (context.MousePosition is not { } clientPosition)
            return;

        ProcessPendingControls();

        IModule[] moduleSnapshot;
        lock (_modulesLock)
        {
            moduleSnapshot = [.. _modules];
        }

        foreach (var module in moduleSnapshot)
        {
            if (!module.Enabled || !module.AcceptsInput) continue;
            if (module.ProcessMouseEvent(context)) return;
        }

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
            if (_focusFromKeyboard)
            {
                _focusFromKeyboard = false;
                Invalidate();
            }

            _pressedControl = hitControl;

            if (hitControl is null)
                SetFocus(null);
            else if (hitControl.Focusable && hitControl != FocusedControl)
                SetFocus(hitControl);
        }

        foreach (var root in rootSnapshot)
            DispatchMouseEventRecursive(root, context);

        if (context.Type == MouseEventType.MouseUpEvent)
        {
            if (_pressedControl != null && _pressedControl == hitControl)
            {
                var localPos = _pressedControl.GetLocalPosition(clientPosition);
                _pressedControl.ProcessMouseEvent(context with
                {
                    Type = MouseEventType.ClickEvent,
                    MousePosition = localPos,
                });
            }

            _pressedControl = null;
        }
    }

    public bool ProcessKeyEvent(KeyEventContext context)
    {
        if (_isDebuggable)
        {
            if (_profilerControl is not null && context.Type == KeyEventType.KeyDown && context.IsControlPressed && context.IsShiftPressed && context.KeyCode == 71)
            {
                var profilerState = _profilerControl.Enabled;
                _profilerControl.Enabled = !profilerState;
                _profilerControl.Visible = !profilerState;
            }
        }

        IModule[] moduleSnapshot;
        lock (_modulesLock)
        {
            moduleSnapshot = [.. _modules];
        }

        foreach (var module in moduleSnapshot)
        {
            if (!module.Enabled || !module.AcceptsInput) continue;
            if (module.ProcessKeyEvent(context)) return true;
        }

        if (FocusedControl is not null
            && FocusedControl.Visible
            && FocusedControl.Enabled
            && FocusedControl.ProcessKeyEvent(context))
        {
            return true;
        }

        if (context.Type == KeyEventType.KeyDown && context.KeyCode == 9)
        {
            if (context.IsShiftPressed)
                FocusPrevious();
            else
                FocusNext();
            return true;
        }

        return false;
    }

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
            if (_hostWindow.UseDirtyRendering && Volatile.Read(ref _framesToRender) <= 0)
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
            {
                if (Interlocked.Decrement(ref _framesToRender) < 0)
                    Interlocked.Exchange(ref _framesToRender, 0);
            }

            return true;
        }
    }

    public IControl? GetControlAt(SKPoint clientPosition)
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
                var hit = HitTestRecursive(_controls[i], clientPosition);
                if (hit != null) return hit;
            }
        }

        return null;
    }

    public bool IsPointOverAnyControl(SKPoint clientPosition)
    {
        return GetControlAt(clientPosition) != null;
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

                if (FocusedControl == control)
                {
                    FocusedControl = null;
                    _focusFromKeyboard = false;
                }

                if (_pressedControl == control)
                    _pressedControl = null;

                control.Dispose();
            }
        }

        Invalidate();
    }

    private static IControl? HitTestRecursive(IControl control, SKPoint cursorPosition, bool isMouseClipped = false)
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
            if (hit != null) return hit;
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
            canvas.SaveLayer(control.AlphaPaint);

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

        if (control.ClipChildren)
            canvas.Restore();

        if (control.IsFocused && _focusFromKeyboard && control is BaseControl focusedCtrl)
            focusedCtrl.DrawFocusRect(canvas);

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
            {
                _modules.Add(module);
                module.Start();
            }

            foreach (var module in toRemove)
            {
                if (!_modules.Remove(module)) continue;
                module.Stop();
                module.Dispose();
            }
        }
    }

    public bool UpdateModules(float deltaTime)
    {
        ProcessPendingModules();

        TotalTime += deltaTime;

        lock (_modulesLock)
        {
            // Uncomment this to make modules only update on actual drawn frames (on dirty rendering mode)
            //if (_hostWindow.UseDirtyRendering && Volatile.Read(ref _framesToRender) <= 0) return false;

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
            DisposeRecursively(childControl);

        control.Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;

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

            FocusedControl = null;
            _pressedControl = null;
            _focusFromKeyboard = false;
            TotalTime = 0;

            lock (_modulesLock)
            {
                foreach (var module in _modules)
                {
                    module.Stop();
                    module.Dispose();
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
        }

        _isDisposed = true;
    }

    #endregion
}