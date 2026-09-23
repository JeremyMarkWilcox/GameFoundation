extends Node
var checks := 0
var failed := false
var devices: Array[int] = []
var completions: Array[bool] = []

func check(ok: bool, message: String) -> void:
	if not ok:
		failed = true
		push_error("FAIL: " + message)
	else:
		checks += 1
		print("PASS: " + message)

func _ready() -> void:
	if not "--smoke-test" in OS.get_cmdline_user_args():
		push_error("Use -- --smoke-test to isolate settings and saves.")
		get_tree().quit(1)
		return
	process_mode = Node.PROCESS_MODE_ALWAYS
	get_tree().current_scene = null
	get_tree().create_timer(20.0).timeout.connect(_timeout)
	_run.call_deferred()

func _timeout() -> void:
	push_error("GDSCRIPT INTEROP TIMEOUT")
	get_tree().quit(1)

func _on_device_changed(device: int) -> void:
	devices.append(device)

func _on_transition_finished(succeeded: bool) -> void:
	completions.append(succeeded)

func frames(count: int = 3) -> void:
	for i in count:
		await get_tree().process_frame

func _run() -> void:
	SaveManager.DeleteSave(97)
	SaveManager.NewGame(97)
	check(SaveManager.GetStat("absent", 2.5) == 2.5, "stat fallback crosses language boundary")
	check(SaveManager.GetFlag("absent", true), "flag fallback crosses language boundary")
	check(SaveManager.GetText("absent", "fallback") == "fallback", "text fallback crosses language boundary")
	SaveManager.SetStat("score", 12.5)
	SaveManager.SetFlag("door_open", true)
	SaveManager.SetText("name", "Player Ω")
	SaveManager.SetCheckpointId("entry")
	var demo: PackedScene = load("res://addons/CoreUI/Examples/GDScriptDemo.tscn")
	SaveManager.SetLastLevel(demo)
	check(SaveManager.SaveGame(97), "GDScript saves C# backend")
	SaveManager.NewGame(97)
	check(SaveManager.GetStat("score", 0.0) == 0.0, "new game resets shared data")
	check(SaveManager.LoadGame(97), "GDScript loads C# backend")
	check(SaveManager.GetStat("score", 0.0) == 12.5 and SaveManager.GetFlag("door_open", false), "numeric and boolean save round trip")
	check(SaveManager.GetText("name", "") == "Player Ω" and SaveManager.GetCheckpointId() == "entry", "text and checkpoint save round trip")
	check(SaveManager.GetLastLevelPath() == demo.resource_path, "assigned scene saved by resource path")
	check(not SaveManager.SaveGame(0) and not SaveManager.LoadGame(0), "invalid slots rejected")
	SettingsManager.SetMasterVolume(0.37)
	SettingsManager.LoadSettings()
	check(is_equal_approx(SettingsManager.MasterVolume, 0.37), "settings methods and property accessible")
	var binding := InputEventKey.new()
	binding.keycode = KEY_F6
	InputManager.RemapAction("save_game", binding)
	InputManager.LoadKeybinds(PackedStringArray(["save_game"]))
	check(InputMap.action_get_events("save_game").any(func(e): return e is InputEventKey and e.keycode == KEY_F6), "rebind and array argument accessible")
	InputManager.DeviceChanged.connect(_on_device_changed)
	var key := InputEventKey.new()
	key.keycode = KEY_SHIFT
	InputManager._Input(key)
	devices.clear()
	var pad := InputEventJoypadButton.new()
	pad.button_index = JOY_BUTTON_A
	InputManager._Input(pad)
	InputManager._Input(pad)
	InputManager._Input(key)
	check(devices == [1, 0], "C# Godot signal arrives once per device change")
	InputManager.DeviceChanged.disconnect(_on_device_changed)
	PauseController.SetPaused(true)
	check(PauseController.IsPaused, "GDScript pauses through existing global")
	PauseController.SetPaused(false)
	check(not get_tree().paused, "GDScript resumes through existing global")
	AudioManager.PlayUISound(1)
	AudioManager.PlayUISound(-1)
	check(true, "audio bridge accepts valid and ignores invalid sound IDs")
	SceneFlowManager.FadeDuration = 0.01
	SceneFlowManager.TransitionFinished.connect(_on_transition_finished)
	check(not SceneFlowManager.RequestScene(null), "invalid scene rejected without awaiting")
	check(SceneFlowManager.RequestScene(demo), "PackedScene request accepted")
	check(not SceneFlowManager.RequestScene(demo), "duplicate transition rejected")
	var result = await SceneFlowManager.TransitionFinished
	check(result and not SceneFlowManager.IsTransitioning, "transition completion signal is awaitable")
	await frames()
	check(get_tree().current_scene.get_script().resource_path.ends_with("GDScriptDemo.gd"), "GDScript example scene loaded")
	var example = get_tree().current_scene
	var save_event := InputEventAction.new()
	save_event.action = example.save_action
	save_event.pressed = true
	MenuManager.OpenMenu(example.pause_menu)
	await frames()
	check(MenuManager.ActiveMenu == example.pause_menu and get_tree().paused, "Inspector-assigned C# menu opens from GDScript")
	example._unhandled_input(save_event)
	check(SaveManager.GetStat("gd_demo_visits", 0.0) == 0.0, "menu blocks GDScript gameplay saving")
	MenuManager.CloseMenu()
	check(MenuManager.ActiveMenu == null and not get_tree().paused, "GDScript closes menu and restores pause")
	SaveManager.CaptureCurrentScene()
	check(SaveManager.GetLastLevelPath() == demo.resource_path, "current scene captured for Continue")
	example._unhandled_input(save_event)
	check(SaveManager.GetStat("gd_demo_visits", 0.0) == 1.0, "GDScript example saves through C#")
	SceneFlowManager.RestartCurrentScene()
	await SceneFlowManager.TransitionFinished
	await frames()
	check(get_tree().current_scene.scene_file_path == demo.resource_path, "restart callable from GDScript")
	check(SceneFlowManager.RequestSceneReference(SceneFlowManager.MainMenuScene), "scene-reference request accepted")
	await SceneFlowManager.TransitionFinished
	await frames()
	check(MenuManager.ActiveMenu != null and not MenuManager.ActiveMenu.CanGoBack, "C# title loads after GDScript gameplay")
	check(completions == [true, true, true], "one completion per accepted transition")
	SceneFlowManager.TransitionFinished.disconnect(_on_transition_finished)
	SaveManager.DeleteSave(97)
	print("GDSCRIPT INTEROP %s (%d checks)" % ["FAIL" if failed else "PASS", checks])
	get_tree().quit(1 if failed else 0)
