using Godot;

public partial class MainMenuController : BaseMenu
{
    [Export(PropertyHint.File, "*.tscn")]
    private string demoScenePath = "res://addons/CoreUI/Examples/DemoGame.tscn";

    private Button _startButton;
    private Button _settingsButton;
    private Button _quitButton;
    private bool _transitioning;

    public override void _Ready()
    {
        base._Ready();

        GD.Print($"[MainMenu] Ready. MenuId='{MenuId}'");

        CallDeferred(MethodName.OpenInitialMenu);

        _startButton = GetNodeOrNull<Button>("MarginContainer/MainLayout/VBoxContainer/StartGameButton");
        _settingsButton = GetNodeOrNull<Button>("MarginContainer/MainLayout/VBoxContainer/SettingsButton");
        _quitButton = GetNodeOrNull<Button>("MarginContainer/MainLayout/VBoxContainer/QuitButton");

        GD.Print($"[MainMenu] Buttons found: start={_startButton != null}, settings={_settingsButton != null}, quit={_quitButton != null}");

        if (_startButton != null)
        {
            _startButton.Pressed += StartGame;
            _startButton.ButtonDown += () => GD.Print("[MainMenu] Start button ButtonDown");
            GD.Print("[MainMenu] Start button Pressed handler connected");
        }
        if (_settingsButton != null) _settingsButton.Pressed += OpenSettings;
        if (_quitButton != null) _quitButton.Pressed += QuitGame;
    }

    private void OpenInitialMenu() => UIEventBus.RequestOpenMenu(MenuId);

    private void StartGame()
    {
        if (_transitioning) return;
        _transitioning = true;

        GD.Print($"[MainMenu] Starting game scene: {demoScenePath}");

        if (!ResourceLoader.Exists(demoScenePath))
        {
            _transitioning = false;
            GD.PrintErr($"[MainMenu] Demo scene does not exist: {demoScenePath}");
            return;
        }

        UIEventBus.RequestCloseAll();
        GetTree().Paused = false;

        Error result = GetTree().ChangeSceneToFile(demoScenePath);
        if (result != Error.Ok)
        {
            _transitioning = false;
            GD.PrintErr($"[MainMenu] Failed to load demo scene: {result}");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;

        if (key.Keycode is Key.Enter or Key.KpEnter or Key.Space)
        {
            GD.Print("[MainMenu] Explicit keyboard activation");
            GetViewport().SetInputAsHandled();
            StartGame();
        }
    }

    private void OpenSettings() => UIEventBus.RequestOpenMenu("Settings");

    private void QuitGame() => GetNode<SceneFlowManager>("/root/SceneFlowManager").QuitGame();
}
