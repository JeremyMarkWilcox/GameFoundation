# Foundation acceptance checks

## Automated engine check

Build with `dotnet build` or Godot's Build button. Run the .NET Godot executable with:

```text
--headless --path <project-directory> --scene res://addons/CoreUI/Tests/FoundationSmokeTest.tscn -- --smoke-test
```

Expect `FOUNDATION SMOKE PASS` and exit code 0. The test covers title focus/back protection, keyboard events in Settings, preservation/reload of settings and binds, Start, pause → Settings → pause, restart, pause after restart, gameplay saving, returning to title, Settings after return, Continue, and missing slots.

The `--smoke-test` argument isolates writes to `foundation_test_settings.cfg` and `foundation_test_save_*.json`; normal player settings and saves are untouched. Do not use this flag for normal play.

## Manual release checklist

- Fresh import/build on a teammate's machine and an exported desktop build.
- Mouse, keyboard-only, and a physical controller: navigate every button, back out, and switch devices mid-menu.
- Nested/overlay menus leave focus on the top menu and restore the prior focused control when returning.
- Rebind Save; change audio/fullscreen; quit and relaunch; verify both survive. Cancel a pending rebind.
- In a game that assigns the optional Player field: move, save, move elsewhere, return to title and Continue; position returns to the saved location.
- New Game starts fresh; Restart follows the documented saved-position behavior.
- Resize, maximize, and toggle fullscreen: menus stay centered and readable. A project configured for Pixel keeps integer scaling and crisp sprite edges.
- Once perspective-specific player/camera components are added, verify movement, collision, camera behavior and diagonal speed in that real project.
- Repeated scene requests during a fade cause a single transition. Missing destinations report an error without dismissing the current menu.
- Assign audio streams and confirm hover/click/open/close sounds play while paused.

Headless checks do not certify physical gamepad feel, monitor-specific fullscreen behavior, final artwork, or exported-platform compatibility.
