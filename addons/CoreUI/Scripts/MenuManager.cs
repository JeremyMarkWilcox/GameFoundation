using Godot;
using CoreUI;
using System.Collections.Generic;
using System.Linq;

public partial class MenuManager : CanvasLayer
{
    public static MenuManager Instance { get; private set; }
    [Export] public StringName BackAction = "ui_cancel";
    private readonly Dictionary<string, BaseMenu> _registeredMenus = new();
    private readonly Stack<BaseMenu> _menuStack = new();
    private Input.MouseModeEnum _gameplayMouseMode;
    public IReadOnlyCollection<BaseMenu> OpenMenus => _menuStack;
    public BaseMenu ActiveMenu => _menuStack.TryPeek(out var menu) ? menu : null;
    public override void _EnterTree() { Instance = this; ProcessMode = ProcessModeEnum.Always; }
    public override void _Ready()
    {
        UIEventBus.OnMenuOpenRequested += OpenMenuById;
        UIEventBus.OnMenuBackRequested += CloseMenu;
        UIEventBus.OnCloseAllMenusRequested += CloseAllMenus;
        CallDeferred(MethodName.ConnectInput);
    }
    private void ConnectInput()
    {
        if (InputManager.Instance != null) InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
    }
    private void OnDeviceChanged(InputManager.InputDevice device)
    {
        if (ActiveMenu != null) ApplyMenuMouseMode();
    }
    private void ApplyMenuMouseMode() => Input.MouseMode = InputManager.Instance?.CurrentDevice == InputManager.InputDevice.Gamepad
        ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
    public override void _ExitTree()
    {
        UIEventBus.OnMenuOpenRequested -= OpenMenuById;
        UIEventBus.OnMenuBackRequested -= CloseMenu;
        UIEventBus.OnCloseAllMenusRequested -= CloseAllMenus;
        if (InputManager.Instance != null) InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
        if (Instance == this) Instance = null;
    }
    public void RegisterMenu(BaseMenu menu)
    {
        if (string.IsNullOrWhiteSpace(menu.MenuId)) return;
        if (_registeredMenus.TryGetValue(menu.MenuId, out var existing) && existing != menu)
        {
            GD.PushError($"Duplicate MenuId '{menu.MenuId}' on {menu.GetPath()}");
            return;
        }
        _registeredMenus[menu.MenuId] = menu;
    }
    public void UnregisterMenu(BaseMenu menu)
    {
        if (_registeredMenus.TryGetValue(menu.MenuId, out var existing) && existing == menu)
            _registeredMenus.Remove(menu.MenuId);
        if (!_menuStack.Contains(menu)) return;
        var remaining = _menuStack.Reverse().Where(item => item != menu).ToArray();
        _menuStack.Clear();
        foreach (var item in remaining) _menuStack.Push(item);
        ActiveMenu?.Resume();
        UpdatePauseState();
    }
    public void OpenMenuById(string id)
    {
        if (_registeredMenus.TryGetValue(id, out var menu)) OpenMenu(menu);
        else GD.PushError($"No menu registered with ID '{id}'.");
    }
    public void OpenMenu(BaseMenu menu)
    {
        if (!IsInstanceValid(menu) || !menu.IsInsideTree())
        {
            GD.PushError("Assign the destination menu in the Inspector.");
            return;
        }
        if (_menuStack.Contains(menu)) return;
        if (ActiveMenu == null) _gameplayMouseMode = Input.MouseMode;
        ActiveMenu?.Suspend(!menu.IsOverlay);
        _menuStack.Push(menu);
        menu.Open();
        ApplyMenuMouseMode();
        UpdatePauseState();
    }
    public void CloseMenu()
    {
        if (ActiveMenu == null || !ActiveMenu.CanGoBack || SceneFlowManager.Instance?.IsTransitioning == true) return;
        _menuStack.Pop().Close();
        ActiveMenu?.Resume();
        if (ActiveMenu == null) Input.MouseMode = _gameplayMouseMode;
        UpdatePauseState();
    }
    public void CloseAllMenus()
    {
        bool hadMenus = ActiveMenu != null;
        while (_menuStack.TryPop(out var menu))
            if (IsInstanceValid(menu)) menu.Close();
        if (hadMenus) Input.MouseMode = _gameplayMouseMode;
        UpdatePauseState();
    }
    public void UpdatePauseState() => GetTree().Paused =
        _menuStack.Any(menu => menu.PausesGame) || PauseController.Instance?.ManualPause == true;
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(BackAction) && !@event.IsEcho() && ActiveMenu != null)
        {
            GetViewport().SetInputAsHandled();
            CloseMenu();
        }
    }
}
