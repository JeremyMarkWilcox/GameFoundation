using Godot;
using CoreUI;

public partial class DemoGameController : Node
{
    [Export] public BaseMenu PauseMenu;
    [Export] public StringName PauseAction = "ui_cancel";
    [Export] public StringName SaveAction = "save_game";
    [Export] public Label StatusLabel;
    [Export] public Node Player;
    public override void _Ready()
    {
        var data = SaveManager.Instance.CurrentData;
        if (data.LastLevelPath != SceneFilePath || !data.GameStats.TryGetValue("player_x", out var x)) return;
        data.GameStats.TryGetValue("player_y", out var y);
        data.GameStats.TryGetValue("player_z", out var z);
        if (Player is Node2D player2D) player2D.Position = new Vector2(x, y);
        if (Player is Node3D player3D) player3D.Position = new Vector3(x, y, z);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho()) return;
        if (@event.IsActionPressed(PauseAction))
        {
            GetViewport().SetInputAsHandled();
            MenuManager.Instance.OpenMenu(PauseMenu);
        }
        else if (@event.IsActionPressed(SaveAction))
        {
            GetViewport().SetInputAsHandled();
            SaveManager.Instance.CurrentData.LastLevelPath = SceneFilePath;
            var position = Player is Node3D player3D ? player3D.Position :
                Player is Node2D player2D ? new Vector3(player2D.Position.X, player2D.Position.Y, 0) : Vector3.Zero;
            var stats = SaveManager.Instance.CurrentData.GameStats;
            stats["player_x"] = position.X;
            stats["player_y"] = position.Y;
            stats["player_z"] = position.Z;
            bool saved = SaveManager.Instance.SaveGame();
            if (StatusLabel != null) StatusLabel.Text = saved ? "Saved. Return to title and choose Continue." : "Save failed.";
        }
    }
}
