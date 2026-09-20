# Required globals and optional systems

## Shared autoloads

| Global | Responsibility |
| --- | --- |
| MenuManager | Menu stack, focus, Back, menu cursor state |
| SceneFlowManager | Configured destinations, fades, transition cleanup |
| SettingsManager | Persistent volume/fullscreen preferences |
| InputManager | Device detection and persistent action bindings |
| AudioManager | Optional assigned UI sound playback |
| PauseController | Manual pause combined with menu pause |
| SaveManager | Generic versioned slot persistence |

These services are perspective-independent. InputManager reports the input device; the active player/camera chooses gameplay mouse capture and input meaning. Menus restore the gameplay mouse mode when dismissed. Game-specific managers become globals only if they need to survive scene changes or coordinate the entire game.

## Optional modules

Player controllers, cameras, interaction detectors, animation and HUDs should normally be scene components. Inventory, quests, turn management, persistent parties and networking may need project-level services, but are not installed preemptively.

Keep a future feature in the GDD as planned until implemented and validated. Develop it in a real game first, then extract a mature reusable kit if multiple games need it. Include optional scenes without instantiating them or registering autoloads by default.

## Plugin register

| Plugin | Status | Dependency / decision |
| --- | --- | --- |
| Phantom Camera 0.11.0.3 | Existing files retained; plugin and autoload disabled | Not required by the foundation. Evaluate for a camera-heavy game before enabling. License remains in its folder. |

For each new plugin, document: official source, purpose, supported Godot/.NET versions, license, required autoloads, install/enable steps, smallest working example, measured startup/export impact, removal steps and tested platforms. Research those details at the point of adoption; do not download a speculative collection of plugins into the base.

For the retained Phantom Camera version, enable the editor plugin in Project Settings → Plugins and verify its PhantomCameraManager autoload is created by the plugin. Do not add a duplicate manually. Disable the plugin before removing its folder; verify its autoload is removed and remove references from any game scenes that use it.

The base still compiles the retained Phantom Camera C# wrappers and may report their existing warnings. They are not active runtime services. A future decision to remove the package entirely can be made separately.
