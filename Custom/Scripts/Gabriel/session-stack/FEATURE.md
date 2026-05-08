# Gabriel Session Stack

## Purpose
Runtime helper for scene loads. It decides when the full scene-settle workflow
should run, skips that workflow for same-folder load pulses, and exposes a
scene-settle release action for the shared HUD hotkey dispatcher.

## Live Files
- `GabrielSessionStack.cslist`
- `src/GabrielSessionStack.cs`
- `src/SceneSettle.cs`
- `src/PlaybackHold.cs`
- `src/InitialExposureChange.cs`
- `src/SameFolderSceneLoadCheck.cs`
- `src/SceneLoadDirNormalize.cs` *(also compiled into `GabrielHud.cslist`; path
  normalization helpers for HUD same-folder cues)*

## Load Path
- Loaded as a session plugin by `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs`.
- Detects same-folder load pulses before delegating scene-settle control to
  the `SceneSettleRuntime` partial set in `src/SceneSettle.cs`,
  `src/PlaybackHold.cs`, and `src/InitialExposureChange.cs`.

## Responsibilities
- Track scene load edges and same-folder load pulses.
- Hold simulation/audio/exposure during scene settle through
  `SceneSettleRuntime` and same-folder load guards.
- Release transient head-hide state after scene settle completes.
- Register the emergency `Space` release action consumed by the HUD hotkey
  dispatcher.

## Dependencies And Coupling
- Depends on `src/SceneSettle.cs`, `src/PlaybackHold.cs`,
  `src/InitialExposureChange.cs`, `src/SameFolderSceneLoadCheck.cs`,
  and `src/SceneLoadDirNormalize.cs` (Hud shares the latter helpers).
- Calls `Custom/Scripts/Gabriel/features/head-hide/HeadProximityHide.cs` after
  the settle hold ends.
- Exposes a `JSONStorableAction` on `CoreControl` so the HUD plugin can
  trigger the emergency release without duplicating settle logic.

## References
- `Reference/VaM-Scene-Startup-And-Settle.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Scene-settle timing, same-folder guards, or startup keybindings change.
- `SceneSettleRuntime` responsibility boundaries or the session-stack file split
  changes.
- Action wiring or the `Space` shortcut behavior changes.
