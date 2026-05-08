# Gabriel Session Stack

## Purpose
Runtime helper for scene loads. It decides when the full scene-settle workflow
should run, skips that workflow for same-folder load pulses, and exposes the
session keyboard shortcuts.

## Live Files
- `GabrielSessionStack.cslist`
- `src/GabrielSessionStack.cs`
- `src/SceneSettle.cs`
- `src/PlaybackHold.cs`
- `src/InitialExposureChange.cs`
- `src/SameFolderSceneLoadCheck.cs`
- `src/SessionKeyboardShortcuts.cs`

## Load Path
- Loaded as a session plugin by `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs`.
- Detects same-folder load pulses before delegating scene-settle control to
  the `OnSceneStartup` partial set in `src/SceneSettle.cs`,
  `src/PlaybackHold.cs`, and `src/InitialExposureChange.cs`.

## Responsibilities
- Track scene load edges and same-folder load pulses.
- Hold simulation/audio/exposure during scene settle through `OnSceneStartup` and same-folder load guards.
- Release transient head-hide state after scene settle completes.
- Expose emergency/session shortcuts through `SessionKeyboardShortcuts`.

## Dependencies And Coupling
- Depends on `src/SceneSettle.cs`, `src/PlaybackHold.cs`,
  `src/InitialExposureChange.cs`, `src/SameFolderSceneLoadCheck.cs`, and
  `src/SessionKeyboardShortcuts.cs`.
- Calls `Custom/Scripts/Gabriel/features/head-hide/HeadProximityHide.cs` after
  the settle hold ends.

## References
- `Reference/VaM-Scene-Startup-And-Settle.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Scene-settle timing, same-folder guards, or startup keybindings change.
- `OnSceneStartup` responsibility boundaries or the session-stack file split
  changes.
- Plugin panel copy or the `Space`/`P` shortcut behavior changes.
