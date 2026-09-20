# Game Foundation

A small Godot 4.7.2 .NET / C# starting framework for team projects. Desktop is the validated target. .NET 8 SDK is required (Android configuration targets .NET 9 but is not validated here).

## Start here

1. Import `project.godot` into the **.NET** build of Godot 4.7.2.
2. Click **Build**, then **F5**. The title scene includes Settings as a sibling menu.
3. Try New Game, Escape / controller Menu (Start, hamburger) / B, Settings, Restart, and Return to Title. Menu opens Pause during gameplay; pressing it again resumes. In Settings it returns to the previous menu.
4. In gameplay, **F5 / controller Y** saves. Return to Title and choose Continue. Settings includes a Save-action rebind button; Escape/B cancels listening.

The solution currently retains its original `Main Menu.sln` filename. This does not affect the project name or behavior.

## One maintained foundation

Keep one general template. Create a copy when beginning a game or the next focused perspective starter, then choose its presentation settings and add only the player, camera and plugins it needs. Do not maintain parallel Pixel/Raster/3D copies just for presentation settings.

See `docs/PROJECT_SETUP.md` for the Pixel, Raster and 3D settings guide. The documented 640×360 Pixel resolution is a suggested starting point, not a confirmed personal preference. Perspective-specific controllers and camera rigs are future work. The owner will build and test small playable examples one at a time before handing mature starters to other developers.

## Assign references in the Inspector

- **Main menu:** assign its gameplay `PackedScene`, sibling `SettingsMenu`, and button nodes.
- **Pause menu:** assign its Settings menu and buttons in the gameplay scene.
- **Settings:** its controls are direct exported node fields. Renaming/rearranging UI no longer requires editing script paths.
- **Return to Title:** open `Configuration/SceneFlow.tscn`, expand `MainMenuScene`, and select a scene file. The custom `SceneReference` resource is lazy to avoid title → game → pause → title circular PackedScene dependencies.
- **Audio:** optional UI sound streams can be assigned in `addons/CoreUI/Scenes/Audio.tscn`. Empty slots are silent by design.
- **Input:** actions are defined in Project Settings → Input Map. Scripts expose `StringName` action fields. These remain names because Input Map actions are named engine identifiers.
- **Theme:** edit `addons/CoreUI/Resources/FoundationTheme.tres` or assign a replacement Theme on your menu roots.

Fields are the team convention for Inspector configuration. Godot also supports exported properties; properties are not universally unsafe.

## Ownership and dependencies

`addons/CoreUI/Scripts` and `Scenes` contain reusable code. They do not depend on example controllers. `Examples` demonstrates composition; `Configuration` holds this project's title destination. Create a `Game` folder for each real project’s scenes and scripts.

Required autoloads are preconfigured: MenuManager, SceneFlowManager, SettingsManager, AudioManager, PauseController, InputManager, SaveManager. The previously installed Phantom Camera 0.11.0.3 files and license are retained, but its editor plugin and autoload are disabled. The foundation does not require it. See `docs/OPTIONAL_SYSTEMS.md` before enabling or adding plugins.

Use `MenuManager.OpenMenu(assignedMenu)` for normal UI composition. Menu IDs and UIEventBus string opening remain an optional compatibility API; duplicate IDs produce a diagnostic. The active menu owns focus and input. Device detection does not change gameplay mouse mode; menus temporarily choose cursor visibility and restore the previous mode when closed. Title menus cannot be dismissed with Back. PauseController's manual pause is combined with menu pause requirements.

## Persistence

Preferences and keybindings share `user://settings.cfg` using read/modify/write. Bindings reload at startup. One binding per device family (keyboard/mouse or gamepad) is supported for each action. Duplicate/conflicting bindings, analog-axis rebinding UI, and a reset-to-default UI are not included.

SaveManager supports numbered slots, schema version 1, temporary-file replacement and a previous-save backup. Continue loads the saved scene. The demo saves its scene; its optional Player field can also record and restore a Node2D/Node3D position when assigned. New Game resets memory; it does not delete the previous save until the next successful save. Restart reloads the current scene and restores its last saved position when available. Each game must decide its actual checkpoint and autosave policy.

Save data strings (JSON keys, paths stored on disk), log messages, labels, and engine property names are intentional strings. Editable scene, node and menu dependencies are Inspector references.

## Verification and maintenance

See [the proposed starter roadmap](docs/STARTER_ROADMAP.md) for the build order to discuss, repository hierarchy, and README requirements for future developers.

See `docs/SMOKE_TESTS.md` for automated and manual checks, `docs/GDD_TEMPLATE.md` for new-game planning, and `docs/FRAMEWORK_STATUS.md` for scope.

Make common fixes in this foundation first, run the checks, then bring the relevant files into each active project. Do not replace a game's entire customized UI/configuration when updating shared scripts. Create each new copy intentionally when that game or starter becomes the active task. Keep a foundation version or source commit in its README and GDD.
