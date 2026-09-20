using Godot;

public partial class PauseController : Node
{
    public static PauseController Instance { get; private set; }
    public bool ManualPause { get; private set; }
    public bool IsPaused => GetTree().Paused;
    public override void _EnterTree() { Instance = this; ProcessMode = ProcessModeEnum.Always; }
    public override void _ExitTree() { if (Instance == this) Instance = null; }
    public void SetPaused(bool paused)
    {
        ManualPause = paused;
        if (MenuManager.Instance != null) MenuManager.Instance.UpdatePauseState();
        else GetTree().Paused = paused;
    }
    public void TogglePaused() => SetPaused(!ManualPause);
}
