using Godot;

namespace CoreUI;

/// <summary>Shared read/modify/write entry point for preferences and bindings.</summary>
public static class SettingsStore
{
    internal static bool IsTestRun => System.Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--smoke-test");
    public static string Path => IsTestRun ? "user://foundation_test_settings.cfg" : "user://settings.cfg";

    public static ConfigFile Read()
    {
        var config = new ConfigFile();
        var error = config.Load(Path);
        if (error != Error.Ok && error != Error.FileNotFound)
            GD.PushWarning($"Cannot read settings: {error}");
        return config;
    }

    public static bool Write(ConfigFile config)
    {
        var error = config.Save(Path);
        if (error != Error.Ok) GD.PushError($"Cannot save settings: {error}");
        return error == Error.Ok;
    }
}
