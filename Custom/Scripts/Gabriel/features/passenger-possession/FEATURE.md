# Gabriel Passenger Possession

## Purpose
Passenger-style body possession runtime used by the Gabriel palm HUD and possess flows. It aligns the rig to a target person, prepares ImprovedPoV, and hands off to delayed VR hand possession.

## Live Files
- `PassengerRuntime.cs`
- `PassengerHandPrePossessSnapshot.cs`
- `PassengerPossessableNarrow.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- Entered from `GabrielHudButtons` palm HUD / possess routines rather than from standalone scene buttons.

## Responsibilities
- Pick target persons, align the navigation rig, and preserve/restore head and rig state.
- Prepare `ImprovedPoV` on the target and suppress duplicate head-hide behavior where needed.
- Delay VR hand possession until a later grip/trigger confirmation step.

## Dependencies And Coupling
- Depends on `Custom/Scripts/Gabriel/features/improved-pov/ImprovedPoV.cs` and cooperates with `HeadProximityHide`.
- Driven primarily by the `palm-hud` and `ui-hud` layers.

## References
- `Reference/EasyMate-Hand-Menu-Passenger-Possession.md`
- `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`
- `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Target selection, startup alignment, or hand possession timing changes.
- ImprovedPoV prep or restore behavior changes.
- Any preserved-state fields or narrow-possess filters change.
