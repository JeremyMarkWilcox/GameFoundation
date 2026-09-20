using Godot;

public partial class SettingsMenu : BaseMenu
{
    [Export] public HSlider VolumeSlider;
    [Export] public CheckButton FullscreenToggle;
    [Export] public Button BackButton;
    public override void _Ready()
    {
        base._Ready();
        var settings = SettingsManager.Instance;
        if (VolumeSlider != null)
        {
            VolumeSlider.Value = settings.MasterVolume;
            VolumeSlider.ValueChanged += value => settings.SetMasterVolume((float)value);
        }
        if (FullscreenToggle != null)
        {
            FullscreenToggle.ButtonPressed = settings.Fullscreen;
            FullscreenToggle.Toggled += settings.SetFullscreen;
        }
        if (BackButton != null) BackButton.Pressed += () => MenuManager.Instance.CloseMenu();
        if (VolumeSlider == null || FullscreenToggle == null || BackButton == null)
            GD.PushError($"{Name}: assign all settings controls in the Inspector.");
    }
}
