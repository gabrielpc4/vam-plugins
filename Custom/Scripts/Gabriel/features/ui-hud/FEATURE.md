# Gabriel UI HUD

## Purpose
Main session HUD and hotkey hub. This area owns the world-space menu, log copy HUD, next-scene button firing, and most cross-feature runtime orchestration.

## Live Files
- `GabrielHud.cs`
- `GabrielHud.cslist`
- `GabrielHudButtons.cs`
- `VaMLogClipboardHud.cs`
- `VaMLogClipboardHud.cslist`

## Load Path
- `GabrielBootstrap` loads both `VaMLogClipboardHud.cslist` and `GabrielHud.cslist` as session plugins.
- `GabrielHud.cslist` compiles the bulk of the HUD-adjacent feature helpers from other feature folders.
- `VaMLogClipboardHud.cslist` stays isolated so log copy buttons can still load if the main HUD compile fails.

## Responsibilities
- Build and refresh the world-space Gabriel menu on `mainHUD`.
- Own hotkeys, next-scene UIButton resolution, plugin-family toggles, and most scene-change callbacks.
- Expose user toggles for remote grip link blocking, head hide, mocap-end default loads, same-folder camera retain, monitor lasers, and fluid-cum visibility.

## Dependencies And Coupling
- Calls into `palm-hud`, `passenger-possession`, `scene-camera`, `clothing-interactions`, `head-hide`, `e-motion`, and `spankings` helpers.
- `GabrielHudButtons` owns external plugin family paths for E-Motion, Spankings, and ClothingTouchFallOff.

## References
- `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md`
- `Reference/EasyMate-next-scene-hand-hud-handoff.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- HUD rows, hotkeys, or default toggles change.
- Plugin family paths or next-scene resolution behavior changes.
- Cross-feature callback wiring changes.
