# Starter library — proposed plan

The owner will build and test each starter before handing it to other developers. Build one small playable example at a time, document it, and then move on. This is a planning proposal; the order is not yet agreed and these repositories have not been created.

## Suggested order

1. **Foundation:** finish documentation, verify a clean copy and an exported desktop build, then record a stable version.
2. **2D top-down:** movement, collisions, a following camera, one interaction, and a tiny test room. Validate pixel and raster settings in this starter.
3. **2D side-scroller:** reuse the shared setup; add grounded movement, jumping, platforms, and camera boundaries.
4. **3D first-person:** movement, mouse/controller look, camera capture, interaction, and a small test level.
5. **3D third-person:** add an orbit camera, camera collision, camera-relative movement, and basic animation integration.
6. **3D top-down/isometric:** define fixed versus rotating camera and direct versus click-to-move controls before implementation.

This order builds from fewer moving parts toward more camera and movement coordination. Change it if the first intended game needs another perspective. Pixel and raster are presentation presets, not separate copies of the common managers. Other genres can be added when there is a concrete need.

## Proposed local layout

Keep the existing GameFoundation repository intact. When organizing the library, place separate repositories side by side under a parent folder that is not itself a Git repository or Godot project:

```text
GameProjects/
  GameFoundation/          # Shared foundation repository
  Starter2DTopDown/        # Create when this step begins
  Starter2DSideScroller/
  Starter3DFirstPerson/
  Starter3DThirdPerson/
  Starter3DTopDown/
  Games/                  # Future independent game repositories
```

Group the repositories under the same GitHub account or organization. Use each mature starter as a GitHub template repository when ready. Template-generated projects have independent histories; foundation changes do not automatically propagate. Record the foundation source commit/version in every starter and intentionally port and test shared fixes. Avoid nested Git repositories inside GameFoundation.

Existing foundation remote: https://github.com/JeremyMarkWilcox/GameFoundation

## README required in each starter

- Purpose, perspective, maturity status, and a screenshot of the example.
- Exact Godot/.NET requirements and foundation source version.
- Import, build, run, and export steps from a clean checkout.
- Keyboard/mouse and controller controls.
- Scene layout and required Inspector assignments.
- Pixel/raster settings where relevant, plus camera and movement tuning.
- Included systems, optional plugins with versions/licenses, and known limitations.
- How to start a new game, rename it, and replace the example content.
- Validation performed and remaining checks.

Keep reusable systems in addons/CoreUI, project configuration in Configuration, gameplay in Game, and setup notes in docs. Preserve third-party licenses. Do not commit .godot, build outputs, or personal caches.

## Completion gate for each starter

The owner can play the small example with its supported inputs; menus, pause, settings, save/continue, and restart still work. A clean checkout builds, a desktop export launches, required references are assigned, and the README lets a new developer repeat setup. Only then tag the version and begin the next starter.

Reference: [GitHub template repositories](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template).
