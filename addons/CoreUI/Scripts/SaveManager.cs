using Godot;
using System;
using System.Text.Json;

namespace CoreUI
{
    public partial class SaveManager : Node
    {
        public static SaveManager Instance { get; private set; }

        // The currently active save data in memory
        public SaveData CurrentData { get; private set; } = new SaveData();
        public int CurrentSlot { get; private set; } = 1;

        private string GetSavePath(int slot) => $"user://save_slot_{slot}.json";

        public override void _EnterTree()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Serializes the current data to a JSON file.
        /// </summary>
        public void SaveGame(int slot = -1)
        {
            if (slot == -1) slot = CurrentSlot;

            try
            {
                // WriteIndented makes the JSON readable for debugging during jams
                string jsonString = JsonSerializer.Serialize(CurrentData, new JsonSerializerOptions { WriteIndented = true });
                
                using var file = FileAccess.Open(GetSavePath(slot), FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"[SaveManager] Failed to open save file for writing: {FileAccess.GetOpenError()}");
                    return;
                }

                file.StoreString(jsonString);
                CurrentSlot = slot;
                GD.Print($"[SaveManager] Saved game to slot {slot}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveManager] Exception during SaveGame: {e.Message}");
            }
        }

        /// <summary>
        /// Deserializes JSON from a save slot back into the CurrentData object.
        /// </summary>
        public bool LoadGame(int slot = -1)
        {
            if (slot == -1) slot = CurrentSlot;
            string path = GetSavePath(slot);

            if (!FileAccess.FileExists(path))
            {
                GD.Print($"[SaveManager] No save found in slot {slot}. Starting fresh with default data.");
                CurrentData = new SaveData();
                return false;
            }

            try
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                string jsonString = file.GetAsText();
                
                CurrentData = JsonSerializer.Deserialize<SaveData>(jsonString) ?? new SaveData();
                CurrentSlot = slot;
                
                GD.Print($"[SaveManager] Successfully loaded game from slot {slot}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveManager] Failed to load save, data might be corrupted: {e.Message}");
                CurrentData = new SaveData(); // Fallback to safe state
                return false;
            }
        }

        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (FileAccess.FileExists(path))
            {
                DirAccess.RemoveAbsolute(path);
                GD.Print($"[SaveManager] Deleted save slot {slot}");
            }
        }
    }
}