# Gabriel Passenger Possession

## Purpose
Passenger-style body possession: align the rig to a target person, prepare
ImprovedPoV, then delayed VR hand possession. **Entry** is the **right** UI-aim
beam + face A (`PassengerLaserPossess` + `MonitorModeLaserRestore`); **exit** uses palm
**Despossuir** (`PassengerRuntime.RequestStopForPalmHud`).

## Live Files
- `PassengerRuntime.cs`
- `PassengerLaserPossess.cs`
- `PassengerHandPrePossessSnapshot.cs`
- `PassengerPossessableNarrow.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- Start: `PassengerLaserPossess` → `GabrielHudButtons.RequestPassengerForSpecificPerson`.
- Stop: palm HUD via `GabrielHudButtons` / `PassengerRuntime`.

## Responsibilities
- Pick target persons, align the navigation rig, and preserve/restore head and rig state.
- Prepare `ImprovedPoV` on the target and suppress duplicate head-hide behavior where needed.
- Delay VR hand possession until a later grip/trigger confirmation step.
- Raycast along the **right** UI-aim beam and map hits to a person for laser+A start.

## Dependencies And Coupling
- Depends on `improved-pov/ImprovedPoV.cs` and cooperates with `HeadProximityHide`.
- Cooperates with `scene-camera` (`MonitorModeLaserRestore`), `NextSceneUiButton`
  (palm next-scene row), `palm-hud`, and `GabrielHudButtons` / `PassengerRuntime`.

## References
- `Reference/EasyMate-Hand-Menu-Passenger-Possession.md`
- `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`
- `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Target selection, startup alignment, hand possession timing, or laser+A rules change.
- ImprovedPoV prep or restore behavior changes.
- Any preserved-state fields or narrow-possess filters change.
