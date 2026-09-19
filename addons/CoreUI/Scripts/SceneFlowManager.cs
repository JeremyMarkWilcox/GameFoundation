using Godot;

public partial class SceneFlowManager : Node
{
    public void LoadScene(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            GD.PrintErr("[SceneFlowManager] A scene path is required.");
            return;
        }

        if (!ResourceLoader.Exists(scenePath))
        {
            GD.PrintErr($"[SceneFlowManager] Scene not found: {scenePath}");
            return;
        }

        UIEventBus.RequestCloseAll();
        GetTree().Paused = false;
        Error result = GetTree().ChangeSceneToFile(scenePath);
        if (result != Error.Ok)
        {
            GD.PrintErr($"[SceneFlowManager] Failed to load scene '{scenePath}': {result}");
        }
    }

    public void ReloadCurrentScene()
    {
        UIEventBus.RequestCloseAll();
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    public void QuitGame() => GetTree().Quit();
}
