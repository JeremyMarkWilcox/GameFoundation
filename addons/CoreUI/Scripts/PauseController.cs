using Godot;

public partial class PauseController : Node
{
    public bool IsPaused => GetTree().Paused;

    public void SetPaused(bool paused) => GetTree().Paused = paused;

    public void TogglePaused() => SetPaused(!IsPaused);
}
