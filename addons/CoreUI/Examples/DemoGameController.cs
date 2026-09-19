using Godot;

public partial class DemoGameController : Node2D
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            UIEventBus.RequestOpenMenu("Pause");
        }
    }
}
