using Godot;

public partial class AudioManager : Node
{
    [ExportCategory("Optional UI Sounds")]
    [Export] public AudioStream Hover;
    [Export] public AudioStream Click;
    [Export] public AudioStream Cancel;
    [Export] public AudioStream Open;
    [Export] public AudioStream Close;
    [Export] public AudioStream ErrorSound;
    private AudioStreamPlayer _player;
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _player = new AudioStreamPlayer { MaxPolyphony = 8 };
        AddChild(_player);
        UIEventBus.OnPlayUISound += HandleUISound;
    }
    public override void _ExitTree() => UIEventBus.OnPlayUISound -= HandleUISound;
    // Callable from either language; sound uses the UIEventBus.UISound values.
    public void PlayUISound(int sound)
    {
        if (System.Enum.IsDefined(typeof(UIEventBus.UISound), sound))
            UIEventBus.PlaySound((UIEventBus.UISound)sound);
    }
    private void HandleUISound(UIEventBus.UISound sound)
    {
        var stream = sound switch
        {
            UIEventBus.UISound.Hover => Hover,
            UIEventBus.UISound.Click => Click,
            UIEventBus.UISound.Cancel => Cancel,
            UIEventBus.UISound.Open => Open,
            UIEventBus.UISound.Close => Close,
            _ => ErrorSound
        };
        if (stream == null) return;
        _player.Stream = stream;
        _player.Play();
    }
}
