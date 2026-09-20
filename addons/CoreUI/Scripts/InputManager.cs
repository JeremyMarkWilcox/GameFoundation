using Godot;
using System;
using System.Linq;
using Godot.Collections; // Needed for Array

namespace CoreUI
{
    public partial class InputManager : Node
    {
        public static InputManager Instance { get; private set; }

        public enum InputDevice
        {
            KeyboardAndMouse,
            Gamepad
        }

        public InputDevice CurrentDevice { get; private set; } = InputDevice.KeyboardAndMouse;

        // Fired whenever the player swaps between controller and keyboard
        public event Action<InputDevice> OnDeviceChanged;

        private static string SettingsFilePath => SettingsStore.Path;

        public override void _Ready() => LoadKeybinds(InputMap.GetActions().Select(action => action.ToString()).ToArray());

        public override void _ExitTree() { if (Instance == this) Instance = null; }

        public override void _EnterTree()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
            ProcessMode = ProcessModeEnum.Always; // Ensure input is processed while paused
        }

        public override void _Input(InputEvent @event)
        {
            InputDevice detectedDevice = CurrentDevice;

            // Detect Gamepad input (ignoring tiny stick drifts)
            if (@event is InputEventJoypadButton || 
               (@event is InputEventJoypadMotion motion && Mathf.Abs(motion.AxisValue) > 0.5f))
            {
                detectedDevice = InputDevice.Gamepad;
            }
            // Detect Keyboard or Mouse input
            else if (@event is InputEventKey || @event is InputEventMouseButton || @event is InputEventMouseMotion)
            {
                detectedDevice = InputDevice.KeyboardAndMouse;
            }

            // Trigger updates only when the device actually swaps
            if (detectedDevice != CurrentDevice)
            {
                CurrentDevice = detectedDevice;
                ApplyDeviceSettings();
            }
        }

        private void ApplyDeviceSettings()
        {
            GD.Print($"[InputManager] Input device switched to: {CurrentDevice}");

            // Device detection is global; the active menu/player owns mouse policy.
            OnDeviceChanged?.Invoke(CurrentDevice);
        }

        /// <summary>
        /// Remaps a specific action and immediately saves it to settings.cfg.
        /// </summary>
        public void RemapAction(string actionName, InputEvent newEvent)
        {
            if (!InputMap.HasAction(actionName))
            {
                GD.PrintErr($"[InputManager] Action '{actionName}' does not exist in the InputMap.");
                return;
            }

            bool isNewEventGamepad = newEvent is InputEventJoypadButton || newEvent is InputEventJoypadMotion;

            // Get all current events for this action
            Array<InputEvent> existingEvents = InputMap.ActionGetEvents(actionName);

            // Remove the existing event of the same type (Keyboard/Mouse vs Gamepad)
            foreach (InputEvent existingEvent in existingEvents)
            {
                bool isExistingGamepad = existingEvent is InputEventJoypadButton || existingEvent is InputEventJoypadMotion;
                
                if (isExistingGamepad == isNewEventGamepad)
                {
                    InputMap.ActionEraseEvent(actionName, existingEvent);
                }
            }

            // Add the new remapped event
            InputMap.ActionAddEvent(actionName, newEvent);
            GD.Print($"[InputManager] Remapped '{actionName}' to {newEvent.AsText()}");

            SaveKeybinds(actionName, newEvent, isNewEventGamepad);
        }

        private void SaveKeybinds(string actionName, InputEvent newEvent, bool isGamepad)
        {
            ConfigFile config = SettingsStore.Read();

            // Save under a section specific to the device type
            string section = isGamepad ? "GamepadBinds" : "KeyboardBinds";
            config.SetValue(section, actionName, newEvent);

            SettingsStore.Write(config);
        }

        /// <summary>
        /// Call this once during game boot (e.g., in SettingsManager._Ready or InputManager._Ready) 
        /// to load all customized keybinds from disk.
        /// </summary>
        public void LoadKeybinds(string[] remappableActions)
        {
            if (!FileAccess.FileExists(SettingsFilePath)) return;

            ConfigFile config = new ConfigFile();
            Error err = config.Load(SettingsFilePath);
            if (err != Error.Ok) return;

            foreach (string action in remappableActions)
            {
                if (!InputMap.HasAction(action)) continue;

                // Load Keyboard Remaps
                if (config.HasSectionKey("KeyboardBinds", action))
                {
                    Variant savedKbEvent = config.GetValue("KeyboardBinds", action);
                    ApplyLoadedEvent(action, savedKbEvent.As<InputEvent>(), false);
                }

                // Load Gamepad Remaps
                if (config.HasSectionKey("GamepadBinds", action))
                {
                    Variant savedJoyEvent = config.GetValue("GamepadBinds", action);
                    ApplyLoadedEvent(action, savedJoyEvent.As<InputEvent>(), true);
                }
            }
            
            GD.Print("[InputManager] Keybinds loaded from settings.cfg");
        }

        private void ApplyLoadedEvent(string actionName, InputEvent loadedEvent, bool isGamepad)
        {
            if (loadedEvent == null) return;

            Array<InputEvent> existingEvents = InputMap.ActionGetEvents(actionName);
            foreach (InputEvent existingEvent in existingEvents)
            {
                bool isExistingGamepad = existingEvent is InputEventJoypadButton || existingEvent is InputEventJoypadMotion;
                if (isExistingGamepad == isGamepad)
                {
                    InputMap.ActionEraseEvent(actionName, existingEvent);
                }
            }

            InputMap.ActionAddEvent(actionName, loadedEvent);
        }
    }
}
