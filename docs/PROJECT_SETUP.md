# New project setup

Maintain one foundation. Create each game or perspective starter intentionally, one at a time, without copying .git, .godot or personal IDE caches. Rename the application in Project Settings before saving progress; the application name determines its default user-data folder. Record the foundation version/commit in its README and GDD.

## Pixel art checklist (Godot 4.7.2)

Project Settings → Display → Window:

| Setting | Suggested starting value | Why |
| --- | --- | --- |
| Size → Viewport Width / Height | 640 / 360 | A suggested 16:9 baseline; choose the game's actual art resolution first |
| Size → Window Width / Height Override | 1280 / 720 | A 2× development window without changing world resolution |
| Size → Min Width / Height | 640 / 360 | Prevent shrinking below the base viewport and clipping content |
| Stretch → Mode | viewport | Render at the base resolution before enlarging |
| Stretch → Aspect | keep | Maintain aspect ratio with bars where needed |
| Stretch → Scale Mode | integer | Enlarge pixels by whole-number factors |

Project Settings → Rendering → Textures → Canvas Textures → Default Texture Filter: **Nearest**. Check that individual CanvasItems do not override it to Linear.

These settings correspond to `display/window/size/viewport_width`, `viewport_height`, `window_width_override`, `window_height_override`, `min_width`, `min_height`; `display/window/stretch/mode`, `aspect`, `scale_mode`; and `rendering/textures/canvas_textures/default_texture_filter`.

640×360 is a suggested baseline, not a required art resolution or a confirmed team preference. A project using 320×180, for example, should use that resolution consistently and reconsider UI font/layout sizes. The current foundation uses canvas-items/expand with no explicit base viewport size in project.godot; choose the game's viewport size when applying its preset.

For pixel assets, keep lossless imports, native sprite sizes and intentional integer visual scaling. Pixel-snapping settings are optional: test movement/camera jitter before enabling them. Pixel settings do not supply a pixel font, camera controller, tileset or gameplay.

After setup: test 1280×720 and an awkward window size, fullscreen, visible pixel edges, and all menu layouts. Use a pixel font if the game’s UI style calls for it.

## Raster 2D

Choose a base layout resolution (for example 1280×720), use `canvas_items` stretching and linear texture filtering. Choose keep versus expand deliberately, then test UI anchoring at several sizes. Mipmap/compression choices depend on individual art assets and zoom levels.

## 3D

Choose the renderer based on the real target hardware and features. Keep the foundation's working renderer until that choice is tested. Add only the camera and player scene needed for the chosen perspective. Configure lighting, physics and input in the actual game. No first-person, third-person or isometric controller is supplied yet.

## Finish setup

1. Add game-specific content outside addons/CoreUI.
2. Assign the first gameplay scene on the main menu and the title destination in Configuration/SceneFlow.tscn.
3. Keep the required autoloads; enable optional plugins only after reviewing their setup and dependencies.
4. Decide save/checkpoint behavior, controls, input bindings and accessibility requirements.
5. Build, run the smoke checks, test on a teammate's machine, then test an exported build on each supported platform.

Sources: [Godot multiple resolutions](https://docs.godotengine.org/en/4.5/tutorials/rendering/multiple_resolutions.html), [ProjectSettings](https://docs.godotengine.org/en/4.6/classes/class_projectsettings.html). Settings should be rechecked when upgrading the engine.
