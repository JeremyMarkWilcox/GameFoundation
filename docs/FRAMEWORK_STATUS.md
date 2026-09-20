# Framework status

## Implemented
- Direct Inspector assignments for menu destinations, UI controls, gameplay scenes, player/camera references and input action names.
- Menu stack with active-menu input ownership, keyboard/gamepad focus, previous focus restoration and root Back protection.
- Shared scene transitions with duplicate-request protection, scene validation and pause cleanup.
- Persistent volume/fullscreen and device-specific keybindings loaded at boot.
- Inspector-assignable optional UI sound playback and shared Theme.
- Versioned JSON slot saves, temporary-file replacement, previous-file backup and Continue.
- Demonstration scenes and automated engine regression checks.

## Demonstrated
- Title → gameplay → pause → settings → restart → title → Continue.
- Save rebinding and persistence alongside display/audio settings.
- One general demo; optional Player reference for position persistence.
- Pixel, Raster and 3D settings are documented, not separate maintained templates.

## Deliberately deferred
Localization, separate music/effects volume controls, music crossfades, async loading/progress screens, save-slot selection UI, schema migrations beyond version 1, advanced controller glyphs/hotplug UX, accessibility settings, and genre-specific gameplay systems.

The core should grow when a real project demonstrates repeated need. Pixel and raster are rendering/art-workflow choices, not genre restrictions. Future first-person, third-person and isometric 3D variants can share the same foundation.
