using Godot;
using CoreUI;

[GlobalClass]
public partial class BaseMenu : Control
{
    [ExportCategory("Menu Configuration")]
    [Export] public string MenuId = "";
    [Export] public bool IsOverlay;
    [Export] public bool PausesGame = true;
    [Export] public bool CanGoBack = true;
    [ExportCategory("Navigation")]
    [Export] public Control FirstFocusElement;
    [Export] public Control FallbackFocusElement;
    private Control _lastFocus;
    public bool IsActive => MenuManager.Instance?.ActiveMenu == this;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Hide();
        MenuManager.Instance?.RegisterMenu(this);
        AutoHookupButtons(this);
        if (InputManager.Instance != null)
            InputManager.Instance.OnDeviceChanged += HandleDeviceChanged;
    }
    public override void _ExitTree()
    {
        MenuManager.Instance?.UnregisterMenu(this);
        if (InputManager.Instance != null)
            InputManager.Instance.OnDeviceChanged -= HandleDeviceChanged;
    }
    public virtual void Open()
    {
        ProcessMode = ProcessModeEnum.Always;
        Show();
        UIEventBus.PlaySound(UIEventBus.UISound.Open);
        CallDeferred(MethodName.RestoreFocus);
    }
    public virtual void Suspend(bool hide)
    {
        var focused = GetViewport().GuiGetFocusOwner();
        if (focused != null && IsAncestorOf(focused))
        {
            _lastFocus = focused;
            focused.ReleaseFocus();
        }
        // Visible menus underneath an overlay must not receive input.
        ProcessMode = ProcessModeEnum.Disabled;
        if (hide) Hide();
    }
    public virtual void Close()
    {
        Suspend(true);
        UIEventBus.PlaySound(UIEventBus.UISound.Close);
    }
    public virtual void Resume()
    {
        ProcessMode = ProcessModeEnum.Always;
        Show();
        CallDeferred(MethodName.RestoreFocus);
    }
    private void HandleDeviceChanged(InputManager.InputDevice device)
    {
        if (IsActive && device == InputManager.InputDevice.Gamepad) RestoreFocus();
    }
    private void RestoreFocus()
    {
        if (!IsActive) return;
        // Switching input devices must keep the player's current selection.
        var focused = GetViewport().GuiGetFocusOwner();
        if (IsInstanceValid(focused) && IsAncestorOf(focused) && focused.IsVisibleInTree()
            && focused.FocusMode != FocusModeEnum.None
            && !(focused is BaseButton focusedButton && focusedButton.Disabled)) return;
        foreach (var target in new[] { _lastFocus, FirstFocusElement, FallbackFocusElement })
        {
            if (!IsInstanceValid(target) || !target.IsInsideTree() || !target.IsVisibleInTree()) continue;
            if (target is BaseButton button && button.Disabled) continue;
            if (target.FocusMode == FocusModeEnum.None) continue;
            target.GrabFocus();
            return;
        }
    }
    private void AutoHookupButtons(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is BaseMenu) continue;
            if (child is BaseButton button)
            {
                button.MouseEntered += () => { if (IsActive && !button.Disabled) button.GrabFocus(); };
                button.FocusEntered += () => UIEventBus.PlaySound(UIEventBus.UISound.Hover);
                button.Pressed += () => UIEventBus.PlaySound(UIEventBus.UISound.Click);
            }
            AutoHookupButtons(child);
        }
    }
}
