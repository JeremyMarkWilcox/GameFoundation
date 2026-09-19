using Godot;
using CoreUI; 

public partial class MainMenuController : BaseMenu
{
    [Export] public PackedScene DemoScene;

    [ExportCategory("Menu Buttons")] [Export]
    public Button ContinueButton;

    [Export] public Button StartButton;
    [Export] public Button SettingsButton;
    [Export] public Button QuitButton;

    private bool _transitioning;

    public override void _Ready()
    {
        base._Ready();

        GD.Print($"[MainMenu] Ready. MenuId='{MenuId}'");
        CallDeferred(MethodName.OpenInitialMenu);

        bool hasSave = false;
        if (SaveManager.Instance != null)
        {
            hasSave = SaveManager.Instance.LoadGame();
        }

        if (ContinueButton != null)
        {
            if (hasSave && !string.IsNullOrEmpty(SaveManager.Instance.CurrentData.LastLevelPath))
            {
                ContinueButton.Show();
                ContinueButton.Pressed += ContinueGame;
                FirstFocusElement = ContinueButton; 
            }
            else
            {
                ContinueButton.Hide();
                FirstFocusElement = StartButton; 
            }
        }
        else
        {
            GD.PrintErr("[MainMenu] ContinueButton is not assigned in the Inspector!");
        }

        if (StartButton != null) StartButton.Pressed += StartGame;
        else GD.PrintErr("[MainMenu] StartButton is not assigned in the Inspector!");

        if (SettingsButton != null) SettingsButton.Pressed += OpenSettings;
        if (QuitButton != null) QuitButton.Pressed += QuitGame;
    }

    private void OpenInitialMenu() => UIEventBus.RequestOpenMenu(MenuId);

    private void ContinueGame()
    {
        if (_transitioning) return;
        _transitioning = true;
        
        string savedPath = SaveManager.Instance.CurrentData.LastLevelPath;
        
        if (!ResourceLoader.Exists(savedPath))
        {
            GD.PrintErr($"[MainMenu] Saved scene '{savedPath}' not found. Falling back to default.");
            savedPath = DemoScene?.ResourcePath;
        }
        
        if (!string.IsNullOrEmpty(savedPath))
        {
            SceneFlowManager.Instance.ChangeScene(savedPath);
        }
        else
        {
            _transitioning = false;
            GD.PrintErr("[MainMenu] Cannot continue: DemoScene is not assigned and saved path is invalid.");
        }
    }

    private void StartGame()
    {
        if (_transitioning) return;
        
        if (DemoScene == null)
        {
            GD.PrintErr("[MainMenu] DemoScene is not assigned in the Inspector!");
            return;
        }

        _transitioning = true;
        GD.Print($"[MainMenu] Starting NEW game scene: {DemoScene.ResourcePath}");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave(SaveManager.Instance.CurrentSlot);
            SaveManager.Instance.LoadGame(); 
        }

        SceneFlowManager.Instance.ChangeScene(DemoScene.ResourcePath);
    }

    public override void _Input(InputEvent @event)
    {
        // Block ESC so it doesn't bubble up to your global pause manager
        if (@event is InputEventKey checkKey && checkKey.Pressed && !checkKey.Echo && checkKey.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        // Your existing code stays exactly the same below...
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;

        if (key.Keycode is Key.Enter or Key.KpEnter or Key.Space)
        {
            GD.Print("[MainMenu] Explicit keyboard activation");
            GetViewport().SetInputAsHandled();
        
            if (ContinueButton != null && ContinueButton.Visible)
            {
                ContinueGame();
            }
            else if (StartButton != null)
            {
                StartGame();
            }
        }
    }

    private void OpenSettings() => UIEventBus.RequestOpenMenu("Settings");

    private void QuitGame() => SceneFlowManager.Instance.QuitGame();
}