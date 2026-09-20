using Godot;
using CoreUI;
using System;
using System.Threading.Tasks;

public partial class FoundationSmokeTest : Node
{
    private int _checks;
    private bool HasArg(string value) => Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == value);
    private async Task Capture(string name)
    {
        if (!HasArg("--capture-previews")) return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var size = GetViewport().GetVisibleRect().Size;
        GetViewport().GetTexture().GetImage().SavePng($"res://.godot/{name}-{size.X}x{size.Y}.png");
    }
    public override void _Ready() => CallDeferred(MethodName.Run);
    private void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        GD.Print($"PASS: {message}");
        _checks++;
    }
    private async Task Frames(int count = 3)
    {
        for (int i = 0; i < count; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    private async Task WaitForTransition()
    {
        for (int i = 0; i < 300 && SceneFlowManager.Instance.IsTransitioning; i++) await Frames(1);
        Check(!SceneFlowManager.Instance.IsTransitioning, "transition completes");
        await Frames();
    }
    private async Task GamepadButton(JoyButton button, int device = 0)
    {
        Input.ParseInputEvent(new InputEventJoypadButton { Device = device, ButtonIndex = button, Pressed = true });
        await Frames();
        Input.ParseInputEvent(new InputEventJoypadButton { Device = device, ButtonIndex = button, Pressed = false });
        await Frames();
    }
    private async void Run()
    {
        if (!Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--smoke-test"))
        {
            GD.PushError("Run this scene with -- --smoke-test to isolate saved data.");
            GetTree().Quit(1);
            return;
        }
        // Keep the test harness outside CurrentScene while exercising real scene changes.
        GetTree().CurrentScene = null;
        ProcessMode = ProcessModeEnum.Always;
        try
        {
            var flow = SceneFlowManager.Instance;
            flow.FadeDuration = 0.03f;
            if (HasArg("--small-window"))
            {
                GetTree().Root.ContentScaleSize = new Vector2I(640, 360);
                GetTree().Root.Size = new Vector2I(640, 360);
            }
            SaveManager.Instance.DeleteSave(1);
            SaveManager.Instance.NewGame();
            Check(await flow.ChangeSceneAsync(flow.MainMenuScene.LoadScene()), "title scene loads");
            await Frames();
            var main = MenuManager.Instance.ActiveMenu as MainMenuController;
            Check(main != null && !main.CanGoBack, "title is registered and cannot be dismissed");
            Check(GetViewport().GuiGetFocusOwner() == main.StartButton, "keyboard starts on Start button");
            await Capture("title");
            MenuManager.Instance.CloseMenu();
            Check(MenuManager.Instance.ActiveMenu == main, "Back retains title");
            await GamepadButton(JoyButton.Start);
            Check(MenuManager.Instance.ActiveMenu == main, "Menu button cannot dismiss the title");
            main.SettingsButton.GrabFocus();
            await GamepadButton(JoyButton.A);
            await Frames();
            Check(MenuManager.Instance.ActiveMenu == main.SettingsMenu && !main.Visible, "Settings replaces title");
            var settings = (SettingsMenu)main.SettingsMenu;
            settings.BackButton.GrabFocus();
            await GamepadButton(JoyButton.A, 1);
            Check(MenuManager.Instance.ActiveMenu == main, "A on a second controller activates Settings Back");
            await GamepadButton(JoyButton.A, 1);
            Check(MenuManager.Instance.ActiveMenu == settings, "A reopens Settings using restored title selection");
            await Capture("settings");
            Check(GetViewport().GetVisibleRect().Encloses(settings.BackButton.GetGlobalRect()), "Settings fits the viewport");
            var rebind = settings.GetNode<RebindButton>("Center/Content/SaveBinding");
            rebind.EmitSignal(BaseButton.SignalName.Pressed);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            await Frames();
            Check(MenuManager.Instance.ActiveMenu == settings && rebind.Text != "Press any key...", "Escape cancels rebinding without closing Settings");
            // Real keyboard dispatch: hidden title must not steal Enter or Escape.
            settings.FullscreenToggle.GrabFocus();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false });
            await Frames();
            Check(MenuManager.Instance.ActiveMenu == settings && !flow.IsTransitioning, "Enter in Settings does not start gameplay");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            await Frames();
            Check(MenuManager.Instance.ActiveMenu == main, "Escape returns from Settings");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            InputManager.Instance.RemapAction("save_game", new InputEventKey { PhysicalKeycode = Key.F6 });
            SettingsManager.Instance.SetMasterVolume(0.35f);
            SettingsManager.Instance.SetFullscreen(false);
            Check(SettingsStore.Read().HasSectionKey("KeyboardBinds", "save_game"), "volume and display preserve keybindings");
            InputMap.ActionEraseEvents("save_game");
            InputManager.Instance.LoadKeybinds(new[] { "save_game" });
            Check(InputMap.ActionHasEvent("save_game", new InputEventKey { PhysicalKeycode = Key.F6 }), "bindings reload from disk");
            SettingsManager.Instance.LoadSettings();
            Check(Mathf.IsEqualApprox(SettingsManager.Instance.MasterVolume, 0.35f), "volume reloads from disk");
            main.StartButton.GrabFocus();
            await GamepadButton(JoyButton.A);
            await WaitForTransition();
            Check(MenuManager.Instance.ActiveMenu == null && !GetTree().Paused, "Start clears title and unpauses gameplay");
            await GamepadButton(JoyButton.Start);
            var controllerPause = MenuManager.Instance.ActiveMenu as PauseMenu;
            Check(controllerPause != null && GetTree().Paused, "Menu button opens Pause during gameplay");
            controllerPause.SettingsButton.GrabFocus();
            await GamepadButton(JoyButton.A);
            Check(MenuManager.Instance.ActiveMenu == controllerPause.SettingsMenu, "controller opens nested pause Settings");
            await GamepadButton(JoyButton.Start, 1);
            Check(MenuManager.Instance.ActiveMenu == controllerPause && GetTree().Paused, "Menu button returns from Settings without unpausing");
            await GamepadButton(JoyButton.Start, 1);
            Check(MenuManager.Instance.ActiveMenu == null && !GetTree().Paused, "Menu button resumes from Pause on a second controller");
            Input.MouseMode = Input.MouseModeEnum.Captured;
            var gameplayMouseMode = Input.MouseMode; // Headless display drivers cannot capture a cursor.
            if (DisplayServer.GetName() != "headless")
                Check(gameplayMouseMode == Input.MouseModeEnum.Captured, "windowed gameplay can capture the mouse");
            await GamepadButton(JoyButton.A);
            await Frames();
            Check(Input.MouseMode == gameplayMouseMode, "device detection leaves gameplay mouse mode alone");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.K, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            await Frames();
            var pause = MenuManager.Instance.ActiveMenu as PauseMenu;
            Check(pause != null && GetTree().Paused, "Escape pauses gameplay");
            Check(Input.MouseMode == Input.MouseModeEnum.Visible, "pause releases gameplay mouse capture");
            await Capture("pause");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            pause.SettingsButton.EmitSignal(BaseButton.SignalName.Pressed);
            await Frames();
            Check(MenuManager.Instance.ActiveMenu == pause.SettingsMenu, "pause opens its assigned Settings");
            MenuManager.Instance.CloseMenu();
            Check(MenuManager.Instance.ActiveMenu == pause && GetTree().Paused, "closing Settings keeps gameplay paused");
            await Frames();
            pause.ResumeButton.GrabFocus();
            await GamepadButton(JoyButton.A);
            Check(MenuManager.Instance.ActiveMenu == null && !GetTree().Paused, "A activates Resume while paused");
            Check(Input.MouseMode == gameplayMouseMode, "Resume restores gameplay mouse mode");
            Input.MouseMode = Input.MouseModeEnum.Visible;
            MenuManager.Instance.OpenMenu(pause);
            pause.RestartButton.EmitSignal(BaseButton.SignalName.Pressed);
            await WaitForTransition();
            Check(!GetTree().Paused && MenuManager.Instance.ActiveMenu == null, "restart clears menus and pause");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            await Frames();
            pause = MenuManager.Instance.ActiveMenu as PauseMenu;
            Check(pause != null, "pause works after restart");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            MenuManager.Instance.CloseMenu();
            // Exercise the game's actual save action and Continue flow.
            Input.ParseInputEvent(new InputEventAction { Action = "save_game", Pressed = true });
            await Frames();
            Check(SaveManager.Instance.LoadGame(), "gameplay save action writes a loadable save");
            Check(SaveManager.Instance.CurrentData.LastLevelPath == GetTree().CurrentScene.SceneFilePath, "save records gameplay scene");
            flow.ReturnToMainMenu();
            await WaitForTransition();
            main = MenuManager.Instance.ActiveMenu as MainMenuController;
            Check(main?.ContinueButton.Visible == true, "Continue appears for the saved game");
            main.SettingsButton.EmitSignal(BaseButton.SignalName.Pressed);
            await Frames();
            Check(MenuManager.Instance.ActiveMenu == main.SettingsMenu, "Settings works after returning to title");
            MenuManager.Instance.CloseMenu();
            main.ContinueButton.EmitSignal(BaseButton.SignalName.Pressed);
            await WaitForTransition();
            Check(!GetTree().Paused && MenuManager.Instance.ActiveMenu == null, "Continue restores gameplay");
            SaveManager.Instance.LoadGame(99);
            Check(SaveManager.Instance.CurrentSlot == 99, "missing slot changes the active slot safely");
            SaveManager.Instance.NewGame(98);
            SaveManager.Instance.CurrentData.GameStats["score"] = 12;
            Check(SaveManager.Instance.SaveGame(), "first slot save succeeds");
            SaveManager.Instance.CurrentData.GameStats["score"] = 24;
            Check(SaveManager.Instance.SaveGame(), "replacement save succeeds");
            Check(SaveManager.Instance.LoadGame() && SaveManager.Instance.CurrentData.GameStats["score"] == 24, "save round-trip preserves data");
            var savePath = ProjectSettings.GlobalizePath("user://foundation_test_save_98.json");
            System.IO.File.WriteAllText(savePath, "null");
            Check(SaveManager.Instance.LoadGame() && SaveManager.Instance.CurrentData.GameStats["score"] == 12, "unreadable primary falls back to previous save");
            SaveManager.Instance.DeleteSave(98);
            var transition = flow.ChangeSceneAsync(flow.MainMenuScene.LoadScene());
            Check(!await flow.ChangeSceneAsync(flow.MainMenuScene.LoadScene()), "duplicate transition is rejected");
            Check(await transition, "first transition still completes");
            await Frames();
            GD.Print($"FOUNDATION SMOKE PASS ({_checks} checks)");
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError($"FOUNDATION SMOKE FAIL: {error}");
            GetTree().Quit(1);
        }
    }
}
