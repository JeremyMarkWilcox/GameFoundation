# Validation record

Environment: Godot 4.7.2 .NET on Windows, .NET 8 project, NVIDIA RTX 4070 Ti / D3D12 for windowed checks.

- C# build: passed, zero errors. Four existing warnings in retained Phantom Camera C# wrappers.
- Windowed integration suite: 38 checks passed at 640×360 and 1152×648.
- Headless integration suite: 37 checks passed; the extra windowed check verifies actual cursor capture, which a headless display cannot perform.
- Visual inspection: title, Settings and Pause screenshots at both sizes; controls fit, remain centered, and have visible focus indicators.
- Git whitespace/diff check: passed.
- Google Docs: Game Foundation Documentation tab updated and read back; original two-tab hierarchy retained.

Controller A regression follow-up: reproduced the missing gamepad binding in `ui_accept`, added the bottom face button for all controller devices in Project Settings, and preserved the focused control when switching devices. The expanded suite passes 40 headless checks and 41 windowed checks. Synthetic press/release events now exercise opening Settings, Back on a second controller, returning to the selected title button, starting gameplay, and resuming while paused. The C# build passes with the same four existing warnings. Physical-controller confirmation remains outstanding.

The suite uses isolated test settings and saves. Physical-controller feel, platform exports, audio with final assets, and perspective-specific gameplay remain per-project acceptance checks. Controller events in the automated checks are synthetic.

Menu-button follow-up: explicitly mapped Escape, controller B, and controller Menu/Start to `ui_cancel` in Project Settings. Updated the gameplay hint and README. The windowed suite passes 46 checks, including Menu opening Pause, returning from nested Settings while remaining paused, resuming on a second controller, and preserving the title screen. Build passes with the same four existing warnings.

Screenshots and logs are generated in .godot and excluded from source control. Reproduce them with the smoke scene and -- --smoke-test --capture-previews; add --small-window for 640×360. Run capture without --headless.
