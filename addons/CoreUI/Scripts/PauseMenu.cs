using Godot;
using CoreUI;

public partial class PauseMenu : BaseMenu
{
    [Export] public Button ResumeButton;
    [Export] public Button SettingsButton;
    [Export] public Button RestartButton;
    [Export] public Button MainMenuButton;
    [Export] public BaseMenu SettingsMenu;
    public override void _Ready()
    {
        base._Ready();
        if (ResumeButton != null) ResumeButton.Pressed += () => MenuManager.Instance.CloseMenu();
        if (SettingsButton != null) SettingsButton.Pressed += () => MenuManager.Instance.OpenMenu(SettingsMenu);
        if (RestartButton != null) RestartButton.Pressed += () => SceneFlowManager.Instance.RestartCurrentScene();
        if (MainMenuButton != null) MainMenuButton.Pressed += () => SceneFlowManager.Instance.ReturnToMainMenu();
        if (ResumeButton == null || SettingsMenu == null)
            GD.PushError($"{Name}: assign the Resume button and Settings menu.");
    }
}
