using Godot;

public partial class SettingsMenu : BaseMenu
{
    private HSlider _volumeSlider;
    private CheckButton _fullscreenToggle;
    private Button _backButton;

    public override void _Ready()
    {
        base._Ready();

        _volumeSlider = GetNodeOrNull<HSlider>("MarginContainer/MainLayout/Controls/VolumeSlider");
        _fullscreenToggle = GetNodeOrNull<CheckButton>("MarginContainer/MainLayout/Controls/FullscreenToggle");
        _backButton = GetNodeOrNull<Button>("MarginContainer/MainLayout/Controls/BackButton");

        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        if (_volumeSlider != null)
        {
            _volumeSlider.Value = settings.MasterVolume;
            _volumeSlider.ValueChanged += value => settings.SetMasterVolume((float)value);
        }

        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.ButtonPressed = settings.Fullscreen;
            _fullscreenToggle.Toggled += enabled => settings.SetFullscreen(enabled);
        }

        if (_backButton != null) _backButton.Pressed += () => UIEventBus.RequestBack();
    }
}
