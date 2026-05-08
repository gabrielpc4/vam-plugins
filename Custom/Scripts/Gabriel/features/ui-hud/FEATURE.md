# Gabriel UI HUD

## Purpose
Main session HUD and hotkey hub. This area owns the world-space menu, log copy HUD,
scene-advance UIButton resolution (`NextSceneUiButton`), and most
cross-feature runtime orchestration.

## Live Files
- `GabrielHud.cs`
- `GabrielHud.cslist`
- `GabrielHudButtons.cs`
- `NextSceneUiButton.cs`
- `GabrielHotkeys.cs`
- `VaMLogClipboardHud.cs`
- `VaMLogClipboardHud.cslist`

## Load Path
- `GabrielBootstrap` loads both `VaMLogClipboardHud.cslist` and `GabrielHud.cslist` as session plugins.
- `GabrielHud.cslist` compiles the bulk of the HUD-adjacent feature helpers from other feature folders.
- `VaMLogClipboardHud.cslist` stays isolated so log copy buttons can still load if the main HUD compile fails.

## Responsibilities
- Build and refresh the world-space Gabriel menu on `mainHUD`.
- Own the keyboard hotkeys in `GabrielHotkeys` (`Space`, `Ctrl+Shift+S`, `K`,
  `O`, `F`), plugin toggles, scene-change routing, palm **Próxima cena** via
  `NextSceneUiButton`, plus thin glue into `AnimationNoLoopDetection`, path-rule
  E-Motion merges, Spankings/Clothing grip deferrals, and fluid/camera helpers.
- `GabrielHudButtons` owns external plugin family paths for E-Motion, Spankings, and ClothingTouchFallOff.
- The hotkey dispatcher also invokes the session-plugins `Space` release action
  through `CoreControl`.
- Expose user toggles for remote grip link blocking, head hide, long non-loop
  animation-end default loads, same-folder camera retain, monitor lasers, and
  fluid-cum visibility.

## Dependencies And Coupling
- Calls into other feature folders compiled via `GabrielHud.cslist` — see adjacent
  `FEATURE.md` under `palm-hud`, `animation-no-loop-detection`,
  `passenger-possession`,
  `scene-camera`, `clothing-interactions`, `hands`, `head-hide`,
  `session-plugins`,
  `e-motion`, `spankings`, plus `GabrielHudButtons`-owned plugin toggles.

## References
- `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md`
- `Custom/Scripts/Gabriel/features/hands/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`
- `Reference/EasyMate-next-scene-hand-hud-handoff.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- HUD rows, hotkeys, or default toggles change.
- Plugin family paths or next-scene resolution behavior changes.
- Cross-feature callback wiring changes.
