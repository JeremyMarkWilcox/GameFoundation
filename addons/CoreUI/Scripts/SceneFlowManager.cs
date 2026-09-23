using Godot;
using System;
using System.Threading.Tasks;

namespace CoreUI;

public partial class SceneFlowManager : CanvasLayer
{
    [Signal] public delegate void TransitionFinishedEventHandler(bool succeeded);
    public static SceneFlowManager Instance { get; private set; }
    [Export] public SceneReference MainMenuScene;
    [Export(PropertyHint.Range, "0,2,0.05")] public float FadeDuration = 0.2f;
    [Export] public Color FadeColor = Colors.Black;
    public bool IsTransitioning { get; private set; }
    private ColorRect _overlay;

    public override void _EnterTree() { Instance = this; ProcessMode = ProcessModeEnum.Always; }
    public override void _ExitTree() { if (Instance == this) Instance = null; }
    public override void _Ready()
    {
        Layer = 100;
        _overlay = new ColorRect { Color = FadeColor, MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(_overlay);
        _overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _overlay.Modulate = new Color(1, 1, 1, 0);
    }
    public override void _Input(InputEvent @event)
    {
        if (IsTransitioning) GetViewport().SetInputAsHandled();
    }
    public void ChangeScene(PackedScene scene) => _ = ChangeSceneAsync(scene);
    public void ChangeScene(SceneReference scene) => _ = ChangeSceneAsync(scene?.LoadScene());
    // Distinct names avoid overloaded-method ambiguity across languages.
    // True means accepted. Await TransitionFinished for the eventual result.
    public bool RequestScene(PackedScene scene)
    {
        if (IsTransitioning || scene == null || !scene.CanInstantiate()) return false;
        _ = ChangeSceneAsync(scene);
        return true;
    }
    public bool RequestSceneReference(SceneReference scene) => RequestScene(scene?.LoadScene());
    public void ReturnToMainMenu() => ChangeScene(MainMenuScene);
    public void RestartCurrentScene()
    {
        var path = GetTree().CurrentScene?.SceneFilePath;
        if (!string.IsNullOrEmpty(path)) ChangeScene(ResourceLoader.Load<PackedScene>(path));
    }
    public async Task<bool> ChangeSceneAsync(PackedScene scene)
    {
        if (IsTransitioning) return false;
        if (scene == null || !scene.CanInstantiate())
        {
            GD.PushError("Scene destination is missing or invalid. Assign it in the Inspector.");
            return false;
        }
        IsTransitioning = true;
        bool succeeded = false;
        var previousMenu = MenuManager.Instance?.ActiveMenu;
        _overlay.MouseFilter = Control.MouseFilterEnum.Stop;
        try
        {
            await Fade(1);
            MenuManager.Instance?.CloseAllMenus();
            PauseController.Instance?.SetPaused(false);
            GetTree().Paused = false;
            var error = GetTree().ChangeSceneToPacked(scene);
            if (error != Error.Ok)
            {
                GD.PushError($"Scene change failed: {error}");
                if (IsInstanceValid(previousMenu)) MenuManager.Instance?.OpenMenu(previousMenu);
                return false;
            }
            await ToSignal(GetTree(), SceneTree.SignalName.SceneChanged);
            await Fade(0);
            succeeded = true;
            return true;
        }
        catch (Exception error)
        {
            GD.PushError($"Scene transition failed: {error.Message}");
            return false;
        }
        finally
        {
            _overlay.Modulate = new Color(1, 1, 1, 0);
            _overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
            IsTransitioning = false;
            EmitSignal(SignalName.TransitionFinished, succeeded);
        }
    }
    private async Task Fade(float alpha)
    {
        if (FadeDuration <= 0) { _overlay.Modulate = new Color(1, 1, 1, alpha); return; }
        var tween = CreateTween().SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_overlay, "modulate:a", alpha, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
    public void QuitGame() => GetTree().Quit();
}
