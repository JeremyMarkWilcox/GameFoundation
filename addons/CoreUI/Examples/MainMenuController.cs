using Godot;
using CoreUI;

public partial class MainMenuController : BaseMenu
{
    [ExportCategory("Destinations")]
    [Export] public PackedScene DemoScene;
    [Export] public BaseMenu SettingsMenu;
    [ExportCategory("Buttons")]
    [Export] public Button ContinueButton;
    [Export] public Button StartButton;
    [Export] public Button SettingsButton;
    [Export] public Button QuitButton;

    public override void _Ready()
    {
        CanGoBack = false;
        base._Ready();
        bool hasSave = SaveManager.Instance.LoadGame();
        if (ContinueButton != null)
        {
            ContinueButton.Visible = hasSave && ResourceLoader.Exists(SaveManager.Instance.CurrentData.LastLevelPath, "PackedScene");
            ContinueButton.Pressed += ContinueGame;
        }
        FirstFocusElement = ContinueButton?.Visible == true ? ContinueButton : StartButton;
        if (StartButton != null) StartButton.Pressed += StartGame;
        if (SettingsButton != null) SettingsButton.Pressed += () => MenuManager.Instance.OpenMenu(SettingsMenu);
        if (QuitButton != null) QuitButton.Pressed += () => SceneFlowManager.Instance.QuitGame();
        if (DemoScene == null || StartButton == null || SettingsMenu == null)
            GD.PushError($"{Name}: assign the gameplay scene, Start button and Settings menu.");
        CallDeferred(MethodName.OpenInitialMenu);
    }
    private void OpenInitialMenu() => MenuManager.Instance.OpenMenu(this);
    private void ContinueGame()
    {
        if (!IsActive || SceneFlowManager.Instance.IsTransitioning) return;
        var path = SaveManager.Instance.CurrentData.LastLevelPath;
        SceneFlowManager.Instance.ChangeScene(ResourceLoader.Load<PackedScene>(path));
    }
    private void StartGame()
    {
        if (!IsActive || SceneFlowManager.Instance.IsTransitioning || DemoScene == null) return;
        // Keep the previous on-disk save until the new game reaches its first save point.
        SaveManager.Instance.NewGame();
        SceneFlowManager.Instance.ChangeScene(DemoScene);
    }
}
