using Godot;

namespace CoreUI;

/// <summary>A lazy scene reference avoids circular PackedScene dependencies.</summary>
[GlobalClass]
public partial class SceneReference : Resource
{
    [Export(PropertyHint.File, "*.tscn,*.scn")]
    public string ScenePath = "";

    public PackedScene LoadScene() => string.IsNullOrWhiteSpace(ScenePath)
        ? null : ResourceLoader.Load<PackedScene>(ScenePath);
}
