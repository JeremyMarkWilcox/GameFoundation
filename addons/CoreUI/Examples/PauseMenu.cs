using Godot;
using CoreUI;

public partial class PauseMenu : BaseMenu
{
    private Button _resumeButton;
    private Button _settingsButton;
    private Button _restartButton;
    private Button _mainMenuButton;

    public override void _Ready()
    {
        base._Ready();
        _resumeButton = GetNodeOrNull<Button>("Panel/Margin/MainLayout/ResumeButton");
        _settingsButton = GetNodeOrNull<Button>("Panel/Margin/MainLayout/SettingsButton");
        _restartButton = GetNodeOrNull<Button>("Panel/Margin/MainLayout/RestartButton");
        _mainMenuButton = GetNodeOrNull<Button>("Panel/Margin/MainLayout/MainMenuButton");

        if (_resumeButton != null) _resumeButton.Pressed += () => UIEventBus.RequestBack();
        if (_settingsButton != null) _settingsButton.Pressed += () => UIEventBus.RequestOpenMenu("Settings");
        
        if (_restartButton != null) _restartButton.Pressed += () => SceneFlowManager.Instance.RestartCurrentScene();
        if (_mainMenuButton != null) _mainMenuButton.Pressed += ReturnToMainMenu;
    }

    private void ReturnToMainMenu()
    {
        // Bypass the Inspector entirely and load the file directly from disk
        string menuPath = "res://addons/CoreUI/Examples/KitbashMainMenu.tscn";
        
        if (ResourceLoader.Exists(menuPath))
        {
            SceneFlowManager.Instance.ChangeScene(menuPath);
        }
        else
        {
            GD.PrintErr($"[PauseMenu] Hardcoded path not found: {menuPath}");
        }
    }
}