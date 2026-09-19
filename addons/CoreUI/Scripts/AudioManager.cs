using Godot;

public partial class AudioManager : Node
{
    public override void _Ready()
    {
        UIEventBus.OnPlayUISound += HandleUISound;
    }

    public override void _ExitTree()
    {
        UIEventBus.OnPlayUISound -= HandleUISound;
    }

    private void HandleUISound(UIEventBus.UISound sound)
    {
        // Add shared UI AudioStreamPlayers here when a project supplies its sound library.
        // The event hook is intentionally usable before any audio assets exist.
    }
}
