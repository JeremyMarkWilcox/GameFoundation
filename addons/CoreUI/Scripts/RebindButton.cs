using Godot;
using Godot.Collections;

namespace CoreUI
{
    [GlobalClass]
    public partial class RebindButton : Button
    {
        [Export(PropertyHint.None, "The exact name of the action in the Input Map (e.g., 'jump')")] 
        public StringName ActionName = "save_game";
        
        private bool _isListening = false;

        public override void _Ready()
        {
            // Wait a frame to ensure InputManager has loaded its bindings
            CallDeferred(MethodName.UpdateDisplayText);
            
            Pressed += OnButtonPressed;
            VisibilityChanged += () => { if (!IsVisibleInTree()) { _isListening = false; UpdateDisplayText(); } };
            
            // Subscribe to update the text if the player swaps devices while looking at the menu
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnDeviceChanged += HandleDeviceChanged;
            }
        }

        public override void _ExitTree()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnDeviceChanged -= HandleDeviceChanged;
            }
        }

        private void OnButtonPressed()
        {
            _isListening = true;
            Text = "Press any key...";
            ReleaseFocus(); // Release focus so pressing 'Enter' or 'A' doesn't immediately re-click the button
        }

        public override void _Input(InputEvent @event)
        {
            if (!_isListening || !IsVisibleInTree()) return;
            if (@event.IsEcho()) return;
            if (@event.IsActionPressed("ui_cancel"))
            {
                _isListening = false;
                UpdateDisplayText();
                GrabFocus();
                GetViewport().SetInputAsHandled();
                return;
            }

            // Ignore analog stick drift or mouse movement during rebinding
            if (@event is InputEventMouseMotion || @event is InputEventJoypadMotion) return;

            // Accept key presses, mouse button clicks, or gamepad button presses
            if ((@event is InputEventKey keyEvent && keyEvent.Pressed) ||
                (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed) ||
                (@event is InputEventJoypadButton joyBtn && joyBtn.Pressed))
            {
                // Send the new input to the manager
                InputManager.Instance.RemapAction(ActionName, @event);
                
                _isListening = false;
                UpdateDisplayText();
                
                // Consume the input so it doesn't trigger gameplay or close the menu
                GetViewport().SetInputAsHandled();
                
                // Return UI focus to this button
                GrabFocus(); 
            }
        }

        private void HandleDeviceChanged(InputManager.InputDevice newDevice)
        {
            UpdateDisplayText();
        }

        private void UpdateDisplayText()
        {
            if (ActionName.IsEmpty || !InputMap.HasAction(ActionName))
            {
                Text = "Invalid Action";
                return;
            }

            Array<InputEvent> events = InputMap.ActionGetEvents(ActionName);
            InputEvent displayEvent = null;

            bool showGamepad = InputManager.Instance != null && InputManager.Instance.CurrentDevice == InputManager.InputDevice.Gamepad;

            // Find the correct event type (Keyboard vs Gamepad) to display
            foreach (InputEvent e in events)
            {
                bool isGamepadEvent = e is InputEventJoypadButton || e is InputEventJoypadMotion;
                if (isGamepadEvent == showGamepad)
                {
                    displayEvent = e;
                    break;
                }
            }

            if (displayEvent != null)
            {
                Text = displayEvent.AsText(); 
            }
            else
            {
                Text = "Unbound";
            }
        }
    }
}
