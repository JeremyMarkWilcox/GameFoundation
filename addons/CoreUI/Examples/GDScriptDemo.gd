extends Node2D
## Run GDScriptDemo.tscn with Godot .NET after building C#.
## The C# menu is composed as a child; this script does not inherit it.
@export var pause_menu: Control
@export var status_label: Label
@export var pause_action: StringName = &"ui_cancel"
@export var save_action: StringName = &"save_game"

func _ready() -> void:
	InputManager.DeviceChanged.connect(_on_device_changed)
	_refresh_status()

func _exit_tree() -> void:
	if InputManager.DeviceChanged.is_connected(_on_device_changed):
		InputManager.DeviceChanged.disconnect(_on_device_changed)

func _on_device_changed(_device: int) -> void:
	_refresh_status()

func _refresh_status() -> void:
	if status_label:
		status_label.text = "GDScript → C# globals\nSave action: record a visit. Pause: open the C# menu.\nVisits: %d | Device: %s" % [
			int(SaveManager.GetStat("gd_demo_visits", 0.0)),
			"Gamepad" if InputManager.CurrentDevice == 1 else "Keyboard / mouse"]

func _unhandled_input(event: InputEvent) -> void:
	if SceneFlowManager.IsTransitioning or MenuManager.ActiveMenu != null or get_tree().paused:
		return
	if event.is_action_pressed(pause_action) and not event.is_echo():
		if pause_menu:
			MenuManager.OpenMenu(pause_menu)
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed(save_action) and not event.is_echo():
		SaveManager.SetStat("gd_demo_visits", SaveManager.GetStat("gd_demo_visits", 0.0) + 1.0)
		SaveManager.CaptureCurrentScene()
		if SaveManager.SaveGame(-1):
			_refresh_status()
		get_viewport().set_input_as_handled()
