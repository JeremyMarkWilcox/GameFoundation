using Godot;
using System;
using System.Text.Json;

namespace CoreUI;

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }
    public SaveData CurrentData { get; private set; } = new();
    // Keep the JSON schema and C# data model; expose built-in values to GDScript.
    public float GetStat(string key, float fallback = 0) => CurrentData.GameStats.TryGetValue(key, out var value) ? value : fallback;
    public void SetStat(string key, float value) => CurrentData.GameStats[key] = value;
    public bool GetFlag(string key, bool fallback = false) => CurrentData.GameFlags.TryGetValue(key, out var value) ? value : fallback;
    public void SetFlag(string key, bool value) => CurrentData.GameFlags[key] = value;
    public string GetText(string key, string fallback = "") => CurrentData.StringData.TryGetValue(key, out var value) ? value : fallback;
    public void SetText(string key, string value) => CurrentData.StringData[key] = value;
    public void CaptureCurrentScene() => CurrentData.LastLevelPath = GetTree().CurrentScene?.SceneFilePath ?? "";
    public string GetLastLevelPath() => CurrentData.LastLevelPath;
    public void SetLastLevel(PackedScene scene) => CurrentData.LastLevelPath = scene?.ResourcePath ?? "";
    public string GetCheckpointId() => CurrentData.LastCheckpointId;
    public void SetCheckpointId(string checkpointId) => CurrentData.LastCheckpointId = checkpointId;
    public int CurrentSlot { get; private set; } = 1;
    private string GetSavePath(int slot) => SettingsStore.IsTestRun
        ? $"user://foundation_test_save_{slot}.json" : $"user://save_slot_{slot}.json";
    public override void _EnterTree() => Instance = this;
    public override void _ExitTree() { if (Instance == this) Instance = null; }
    public void NewGame(int slot = 1)
    {
        if (slot < 1) throw new ArgumentOutOfRangeException(nameof(slot));
        CurrentSlot = slot;
        CurrentData = new SaveData();
    }
    public bool SaveGame(int slot = -1)
    {
        if (slot == -1) slot = CurrentSlot;
        if (slot < 1) return false;
        var path = ProjectSettings.GlobalizePath(GetSavePath(slot));
        var temporary = path + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(CurrentData, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(temporary, json);
            if (System.IO.File.Exists(path)) System.IO.File.Replace(temporary, path, path + ".bak");
            else System.IO.File.Move(temporary, path);
            CurrentSlot = slot;
            return true;
        }
        catch (Exception error)
        {
            GD.PushError($"Save failed: {error.Message}");
            return false;
        }
    }
    public bool LoadGame(int slot = -1)
    {
        if (slot == -1) slot = CurrentSlot;
        if (slot < 1) return false;
        CurrentSlot = slot;
        CurrentData = new SaveData();
        var path = ProjectSettings.GlobalizePath(GetSavePath(slot));
        foreach (var candidate in new[] { path, path + ".bak" })
        {
            if (!System.IO.File.Exists(candidate)) continue;
            try
            {
                var data = JsonSerializer.Deserialize<SaveData>(System.IO.File.ReadAllText(candidate));
                if (data == null || data.SchemaVersion != 1 || data.GameStats == null || data.GameFlags == null || data.StringData == null)
                    continue;
                CurrentData = data;
                return true;
            }
            catch (Exception error) { GD.PushWarning($"Cannot read save: {error.Message}"); }
        }
        return false;
    }
    public bool DeleteSave(int slot)
    {
        if (slot < 1) return false;
        try
        {
            var path = ProjectSettings.GlobalizePath(GetSavePath(slot));
            System.IO.File.Delete(path);
            System.IO.File.Delete(path + ".bak");
            return true;
        }
        catch (Exception error) { GD.PushError(error.Message); return false; }
    }
}
