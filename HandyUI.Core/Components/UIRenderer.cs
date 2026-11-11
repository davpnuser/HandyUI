using HandyUI.Core.Abstractions.Interface;
using HandyUI.Core.Helpers;
using SkiaSharp;

namespace HandyUI.Core.Components;

public class UIRenderer
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

    private readonly bool _backgroundColorCustom = false;

    private readonly List<IControl> _rootControls = [];
    private readonly List<IControl> _pendingRootControlsAdd = [];
    private readonly List<IControl> _pendingRootControlsRemove = [];

    private readonly Lock _rootControlsLock = new();
    private readonly bool _isRootControlsOrderDirty = true;

    private int _framesToRender = 2;
    private readonly bool _isDisposed;

    private readonly double _totalTime;

    private readonly IControl? _pressedControl;
    private readonly IControl? _focusedControl;

    private readonly List<IModule> _modules = [];
    private readonly List<IModule> _pendingModulesAdd = [];
    private readonly List<IModule> _pendingModulesRemove = [];

    private readonly Lock _modulesLock = new();
    private readonly GRContext _drawingContext;

    #endregion

    #region Events

    public event Action? WhenInvalidated;

    #endregion

    #region Constructor

    public UIRenderer(IHostWindow host)
    {
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

    #region Controls

    public void AddRootControl(IControl control)
    {
        lock (_pendingRootControlsAdd) _pendingRootControlsAdd.Add(control);
        Invalidate();
    }

    public void RemoveRootControl(IControl control)
    {
        lock (_pendingRootControlsRemove) _pendingRootControlsRemove.Add(control);
        Invalidate();
    }

    #endregion

    #region Modules

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

    #endregion
}