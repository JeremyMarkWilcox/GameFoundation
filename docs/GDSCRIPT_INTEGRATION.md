# C# and GDScript integration

The foundation stays in C#. Gameplay and scene components can use either language. Use Godot .NET and build the C# project before opening/running the examples; the standard non-.NET editor cannot run this foundation.

## Working example

Run `addons/CoreUI/Examples/GDScriptDemo.tscn` after building. Its GDScript root owns an Inspector-assigned C# PauseMenu and status Label. The inherited Settings menu is assigned inside the scene. The Save action increments a visit counter through the shared save backend; Pause opens the existing menu. Return to Title and Continue reuse the saved scene.

The example uses your normal active save slot when run normally. Use the automated test below for isolated saves. It does not replace the project's main scene or add an autoload.

## Calling the shared globals

Use the autoload names directly from GDScript. C# method/property/signal names retain PascalCase. In C#, existing singleton access remains unchanged.

```gdscript
@export var pause_menu: Control
@export var destination: PackedScene

func open_pause() -> void:
    MenuManager.OpenMenu(pause_menu)

func save_progress() -> bool:
    SaveManager.SetStat("coins", 12.0)
    SaveManager.SetFlag("gate_open", true)
    SaveManager.SetText("player_name", "Sam")
    SaveManager.SetCheckpointId("entrance")
    SaveManager.CaptureCurrentScene()
    return SaveManager.SaveGame(-1) # -1 uses the active slot.
```

```csharp
SaveManager.Instance.SetStat("coins", 12.0f);
SaveManager.Instance.SetFlag("gate_open", true);
SaveManager.Instance.SetText("player_name", "Sam");
SaveManager.Instance.SetCheckpointId("entrance");
SaveManager.Instance.CaptureCurrentScene();
bool saved = SaveManager.Instance.SaveGame();
```

Node, scene and resource dependencies belong in exported Inspector fields. Save keys, checkpoint IDs and Input Map action names remain intentional identifiers.

| Global | Shared interface |
| --- | --- |
| MenuManager | OpenMenu(assigned C# menu), CloseMenu(), CloseAllMenus(), ActiveMenu. OpenMenuById() remains available for deliberately registered IDs. |
| PauseController | SetPaused(bool), TogglePaused(), IsPaused. Use MenuManager to open a pause menu; manual pause alone has no UI. |
| SettingsManager | SetMasterVolume(float), SetFullscreen(bool), LoadSettings(), SaveSettings(), MasterVolume, Fullscreen. |
| InputManager | CurrentDevice, RemapAction(action, InputEvent), LoadKeybinds(PackedStringArray), DeviceChanged signal. |
| AudioManager | PlayUISound(int): Hover=0, Click=1, Cancel=2, Error=3, Open=4, Close=5. Invalid IDs are ignored; unassigned streams are silent. |
| SaveManager | NewGame(slot), SaveGame(slot), LoadGame(slot), DeleteSave(slot), CurrentSlot and value accessors below. |
| SceneFlowManager | RequestScene(PackedScene), RequestSceneReference(assigned SceneReference), TransitionFinished signal, IsTransitioning, RestartCurrentScene(), ReturnToMainMenu(). |

## Save data

Use GetStat(key, fallback), GetFlag(key, fallback), GetText(key, fallback) and the matching Set methods. GetCheckpointId()/SetCheckpointId() access the checkpoint. CaptureCurrentScene() records the current scene for Continue; SetLastLevel(assigned PackedScene) records another destination, and GetLastLevelPath() reads it. Scenes must be saved resources to persist a useful destination.

Setters change in-memory data; SaveGame writes it. NewGame resets in-memory data. LoadGame reads the file. These methods use the same data and JSON schema as C# CurrentData, so existing saves stay compatible. CurrentData and its .NET dictionaries are C# implementation details, not the GDScript interface. C# OpenMenus and static Instance properties likewise are not the cross-language contract. Use ActiveMenu and the actual autoload node from GDScript.

Pass positive slot numbers to NewGame/DeleteSave. SaveGame/LoadGame also accept -1 for the current slot. The examples use explicit arguments at the language boundary.

## Notifications and scene transitions

```gdscript
func _ready() -> void:
    InputManager.DeviceChanged.connect(_on_device_changed)

func _exit_tree() -> void:
    if InputManager.DeviceChanged.is_connected(_on_device_changed):
        InputManager.DeviceChanged.disconnect(_on_device_changed)

func _on_device_changed(device: int) -> void:
    # 0 = keyboard/mouse, 1 = gamepad.
    pass

func travel() -> void:
    if SceneFlowManager.RequestScene(destination):
        var succeeded: bool = await SceneFlowManager.TransitionFinished
        if not succeeded:
            push_warning("Scene transition failed")
```

Use a persistent node for a completion listener: a component in the departing scene is freed during travel. RequestScene/RequestSceneReference return false for a busy manager or invalid destination; do not await a rejected request. An accepted transition emits TransitionFinished(bool) after the transition finishes and IsTransitioning is reset, including a false result if execution fails. The signal is shared by all transitions, not a per-request token.

C# can use `InputManager.Instance.DeviceChanged += Handler;` with an int handler, or its existing OnDeviceChanged event. Subscribe to only one to avoid handling a change twice. Existing ChangeScene overloads and ChangeSceneAsync remain available for C#; use the distinctly named Request methods from GDScript.

UIEventBus remains an internal C# convenience. GDScript calls the equivalent MenuManager and AudioManager methods directly; no second global event bus is needed.

## Custom menu behavior

GDScript cannot inherit C# BaseMenu (or vice versa). Keep BaseMenu/PauseMenu on its existing Control and place GDScript behavior on a separate parent or child node with exported references. Connect ordinary button or visibility signals there. Pass the actual C# menu instance to OpenMenu; a plain Control without a BaseMenu script is not a menu.

Do not replace BaseMenu's script with GDScript. Keep gameplay input behind menu/pause/transition checks, as the example demonstrates. Do not reserve a C# custom type as a GDScript type annotation; use engine types such as Node and Control for assigned references.

## Validation

Build, then run:

```text
--headless --path <project-directory> --scene res://addons/CoreUI/Tests/GDScriptInteropTest.tscn -- --smoke-test
```

Expect GDSCRIPT INTEROP PASS and exit code 0. This exercises all seven globals, save round trips, signal delivery, assigned menus, both scene request forms, restart and a real GDScript scene. Run the existing windowed FoundationSmokeTest as well. The flag isolates settings and saves; slot 97 is reserved and cleaned by this test. Do not run suites simultaneously against the same project data.

Physical controller checks and exported-platform validation remain release requirements.

References: [Godot cross-language scripting](https://docs.godotengine.org/en/stable/tutorials/scripting/cross_language_scripting.html), [C# signals](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html).
