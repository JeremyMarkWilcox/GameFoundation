using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class BaseMenu : Control
{
    private bool _buttonsHooked;
    [ExportCategory("Menu Configuration")]
    [Export(PropertyHint.None, "Unique ID used to call this menu (e.g., 'Inventory', 'Settings')")]
    public string MenuId { get; private set; }
    
    [Export(PropertyHint.None, "If true, menus underneath this will remain visible (good for popups).")]
    public bool IsOverlay { get; private set; }

    [Export(PropertyHint.None, "Does opening this menu pause the game?")]
    public bool PausesGame { get; private set; } = true;

    [ExportCategory("Navigation")]
    [Export(PropertyHint.None, "The Control that gets focus when this menu opens.")]
    public Control FirstFocusElement { get; private set; }

    [Export(PropertyHint.None, "The Control to focus if the previous focus is lost.")]
    public Control FallbackFocusElement { get; private set; }

    public override void _Ready()
    {
        // Menus must remain interactive even when opening them pauses gameplay.
        ProcessMode = ProcessModeEnum.Always;

        GD.Print($"[BaseMenu] Ready: {Name}, MenuId='{MenuId}'");
        // Hide by default when the scene loads
        Hide();

        GetNodeOrNull<MenuManager>("/root/MenuManager")?.RegisterMenu(this);
        
        // Concrete menu controllers own their button actions. Keeping signal
        // connections out of the base class prevents duplicate C# signal errors.
    }

    public override void _ExitTree()
    {
        GetNodeOrNull<MenuManager>("/root/MenuManager")?.UnregisterMenu(this);
    }

    /// <summary>
    /// Called by the MenuManager when this menu is added to the stack.
    /// </summary>
    public virtual void Open()
    {
        Show();
        UIEventBus.PlaySound(UIEventBus.UISound.Open);
        
        // Await a frame to ensure layout is calculated before grabbing focus
        CallDeferred(MethodName.GrabInitialFocus);
    }

    /// <summary>
    /// Called by the MenuManager when this menu is removed from the stack.
    /// </summary>
    public virtual void Close()
    {
        UIEventBus.PlaySound(UIEventBus.UISound.Close);
        Hide();
    }

    /// <summary>
    /// Called if this menu is already open, but another menu on top of it was closed.
    /// </summary>
    public virtual void Resume()
    {
        Show();
        CallDeferred(MethodName.GrabInitialFocus);
    }

    private void GrabInitialFocus()
    {
        if (FirstFocusElement != null && FirstFocusElement.IsInsideTree() && FirstFocusElement.Visible)
        {
            FirstFocusElement.GrabFocus();
        }
        else if (FallbackFocusElement != null)
        {
            FallbackFocusElement.GrabFocus();
        }
    }

    /// <summary>
    /// Recursively finds all buttons and connects them to universal hover/click audio 
    /// and mouse-to-focus routing.
    /// </summary>
    private void AutoHookupButtons(Node node)
    {
        if (_buttonsHooked) return;
        _buttonsHooked = true;

        foreach (Node child in node.GetChildren())
        {
            if (child is Button button)
            {
                // Unify Mouse & Gamepad: When the mouse enters a button, it grabs UI focus.
                // This ensures UI focus rings and keyboard/gamepad navigation stay in sync.
                button.MouseEntered += () => 
                {
                    if (!button.HasFocus()) button.GrabFocus();
                };

                // Audio Hooks
                button.FocusEntered += () => UIEventBus.PlaySound(UIEventBus.UISound.Hover);
                button.Pressed += () => UIEventBus.PlaySound(UIEventBus.UISound.Click);
            }

            // Recurse down the tree
            if (child.GetChildCount() > 0)
            {
                AutoHookupButtons(child);
            }
        }
    }
}
