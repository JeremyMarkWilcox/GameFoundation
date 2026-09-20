using System.Collections.Generic;

namespace CoreUI
{
    /// <summary>
    /// A generic data container serialized to JSON. 
    /// Add specific properties here per-project, or use the dictionaries for rapid jam prototyping.
    /// </summary>
    public class SaveData
    {
        public int SchemaVersion { get; set; } = 1;
        // Core tracking
        public string LastLevelPath { get; set; } = string.Empty;
        public string LastCheckpointId { get; set; } = string.Empty;

        // Generic collections for quick jam additions without rewriting classes
        public Dictionary<string, float> GameStats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, bool> GameFlags { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, string> StringData { get; set; } = new Dictionary<string, string>();
    }
}
