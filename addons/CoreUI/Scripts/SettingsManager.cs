using Godot;

public partial class SettingsManager : Node
{
    private const string SettingsPath = "user://settings.cfg";
    private const string AudioSection = "audio";
    private const string DisplaySection = "display";

    public float MasterVolume { get; private set; } = 1.0f;
    public bool Fullscreen { get; private set; }

    public override void _Ready()
    {
        LoadSettings();
        ApplySettings();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp(value, 0.0f, 1.0f);
        ApplyMasterVolume();
        SaveSettings();
    }

    public void SetFullscreen(bool enabled)
    {
        Fullscreen = enabled;
        ApplyDisplaySettings();
        SaveSettings();
    }

    public void LoadSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) != Error.Ok) return;

        MasterVolume = (float)config.GetValue(AudioSection, "master_volume", MasterVolume);
        Fullscreen = (bool)config.GetValue(DisplaySection, "fullscreen", Fullscreen);
    }

    public void SaveSettings()
    {
        var config = new ConfigFile();
        config.SetValue(AudioSection, "master_volume", MasterVolume);
        config.SetValue(DisplaySection, "fullscreen", Fullscreen);
        config.Save(SettingsPath);
    }

    private void ApplySettings()
    {
        ApplyMasterVolume();
        ApplyDisplaySettings();
    }

    private void ApplyMasterVolume()
    {
        int busIndex = AudioServer.GetBusIndex("Master");
        if (busIndex >= 0)
        {
            AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(Mathf.Max(MasterVolume, 0.0001f)));
        }
    }

    private void ApplyDisplaySettings()
    {
        DisplayServer.WindowSetMode(Fullscreen
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
    }
}
