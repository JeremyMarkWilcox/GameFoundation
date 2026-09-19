using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MenuManager : CanvasLayer
{
    // Dictionary to hold all available menus in the scene by their MenuId
    private Dictionary<string, BaseMenu> _registeredMenus = new Dictionary<string, BaseMenu>();
    
    // The stack keeping track of navigation history
    private Stack<BaseMenu> _menuStack = new Stack<BaseMenu>();

    public IReadOnlyCollection<BaseMenu> OpenMenus => _menuStack;

    public override void _Ready()
    {
        GD.Print("[MenuManager] Ready");
        // Crucial: This node must continue processing even when the SceneTree is paused
        ProcessMode = ProcessModeEnum.Always;

        // Subscribe to the Event Bus
        UIEventBus.OnMenuOpenRequested += OpenMenu;
        UIEventBus.OnMenuBackRequested += CloseTopMenu;
        UIEventBus.OnCloseAllMenusRequested += CloseAllMenus;

        // Register any menus that already exist under this node. Menus loaded later
        // register themselves through BaseMenu._Ready().
        RegisterMenus(this);
    }

    public override void _ExitTree()
    {
        // Clean up subscriptions to prevent memory leaks when changing scenes
        UIEventBus.OnMenuOpenRequested -= OpenMenu;
        UIEventBus.OnMenuBackRequested -= CloseTopMenu;
        UIEventBus.OnCloseAllMenusRequested -= CloseAllMenus;
    }

    private void RegisterMenus(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is BaseMenu menu && !string.IsNullOrEmpty(menu.MenuId))
            {
                _registeredMenus[menu.MenuId] = menu;
            }
            RegisterMenus(child);
        }
    }

    public void RegisterMenu(BaseMenu menu)
    {
        if (menu == null || string.IsNullOrWhiteSpace(menu.MenuId)) return;
        _registeredMenus[menu.MenuId] = menu;
        GD.Print($"[MenuManager] Registered menu '{menu.MenuId}'");
    }

    public void UnregisterMenu(BaseMenu menu)
    {
        if (menu == null || string.IsNullOrWhiteSpace(menu.MenuId)) return;
        if (_registeredMenus.TryGetValue(menu.MenuId, out BaseMenu registered) && registered == menu)
        {
            _registeredMenus.Remove(menu.MenuId);
        }
    }

    private void OpenMenu(string menuId)
    {
        GD.Print($"[MenuManager] Open requested: '{menuId}'");
        if (!_registeredMenus.TryGetValue(menuId, out BaseMenu menuToOpen))
        {
            GD.PrintErr($"[MenuManager] Menu with ID '{menuId}' not found!");
            return;
        }

        if (_menuStack.Contains(menuToOpen))
        {
            GD.Print($"[MenuManager] Menu '{menuId}' is already open.");
            return;
        }

        // If there's a menu currently open and the new menu isn't an overlay, hide the current one
        if (_menuStack.Count > 0 && !menuToOpen.IsOverlay)
        {
            _menuStack.Peek().Hide();
        }

        _menuStack.Push(menuToOpen);
        menuToOpen.Open();

        UpdatePauseState();
    }

    private void CloseTopMenu()
    {
        if (_menuStack.Count == 0) return;

        BaseMenu topMenu = _menuStack.Pop();
        topMenu.Close();

        // If there is still a menu underneath, resume it
        if (_menuStack.Count > 0)
        {
            BaseMenu nextMenu = _menuStack.Peek();
            nextMenu.Resume();
        }

        UpdatePauseState();
    }

    private void CloseAllMenus()
    {
        while (_menuStack.Count > 0)
        {
            _menuStack.Pop().Close();
        }
        UpdatePauseState();
    }

    public void OpenMenuById(string menuId) => OpenMenu(menuId);

    public void CloseMenu() => CloseTopMenu();

    private void UpdatePauseState()
    {
        // Pause the game if any open menu requires pausing
        bool shouldPause = _menuStack.Any(menu => menu.PausesGame);
        GetTree().Paused = shouldPause;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Generic "Back" action (e.g., Escape key or Gamepad B/Circle)
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (_menuStack.Count > 0)
            {
                // Consume the input so it doesn't trigger other game logic
                GetViewport().SetInputAsHandled(); 
                UIEventBus.RequestBack();
            }
        }
    }
}
