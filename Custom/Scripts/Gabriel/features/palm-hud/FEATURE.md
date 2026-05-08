# Gabriel Palm HUD

## Purpose
VR-only hand HUD layer. It decides when the right-hand palm UI appears, how A/B/menu inputs are interpreted, and whether optional gesture-based possess/unpossess flows are active.

## Live Files
- `VrEulerPossessHandHud.cs`
- `VrEulerPossessPoseCheck.cs`
- `VrInput.cs`
- `LEGACY-NOTES.md`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- `GabrielHud` ticks the palm HUD and `GabrielHudButtons` supplies the possess/unpossess actions.

## Responsibilities
- Show/hide the right-hand palm HUD from HMD-relative euler windows.
- Map Quest/OpenVR face buttons, menu, select, and grip-trigger abstractions through `VrInput`.
- Drive the gender-choice step and the next-scene row while possession is active or pending.
- Keep the right-hand back-of-hand pose window aligned with the current palm HUD behavior.

## Dependencies And Coupling
- Calls into `GabrielHudButtons` for actual actions and `PassengerRuntime` for possession flow.
- `LEGACY-NOTES.md` contains the deeper handoff details that should stay aligned with this feature doc.

## References
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/palm-hud/LEGACY-NOTES.md`
- `Reference/EasyMate-next-scene-hand-hud-handoff.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Palm HUD pose windows or button mappings change.
- Gender-step behavior or hand-pose visibility rules change.
- `LEGACY-NOTES.md` becomes inconsistent with current runtime behavior.
