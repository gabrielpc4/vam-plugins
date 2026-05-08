# Gabriel Session Plugins

## Purpose
Session bundle loaded from `GabrielSessionPlugins.cslist`. The host
`GabrielSessionOrchestrator` script drives scene loads: same-folder load
pulses, full scene-settle workflow, late feature ticks, HUD binding, and the
scene-settle release action for the shared hotkey dispatcher.

The same compile unit also includes **DildoOnHands** (VR hand toy spawn) and
**VrProximityStripClothing** (VR proximity strip), so bootstrap only merges this
cslist plus the separate log clipboard bundle.

## Live Files
- `GabrielSessionPlugins.cslist`
- `src/GabrielSessionOrchestrator.cs` *(session runtime; scene settle + toggles)*
- `src/SceneSettle.cs`
- `src/PlaybackHold.cs`
- `src/InitialExposureChange.cs`
- `src/SameFolderSceneLoadCheck.cs`
- `src/SceneLoadDirNormalize.cs` *(also compiled into `GabrielHud.cslist`; path
  normalization helpers for HUD same-folder cues)*

## Load Path
- Loaded as a session plugin by `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs`.
- `GabrielSessionOrchestrator` runs in `Update` / `LateUpdate` and detects
  same-folder load pulses before delegating scene-settle control to the
  `SceneSettleRuntime` partial set in `src/SceneSettle.cs`,
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
- `SceneSettleRuntime` responsibility boundaries or the session-plugins file split
  changes.
- Action wiring or the `Space` shortcut behavior changes.
