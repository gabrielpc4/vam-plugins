# Gabriel Head Hide

## Purpose
VR-only transient head hide that hides face, hair, hats, and glasses when the HMD enters a person head cylinder. It is the Gabriel-specific hide layer that cooperates with passenger mode and ImprovedPoV.

## Live Files
- `HeadProximityHide.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- Enabled/disabled from the `GabrielHud` `VR head proximity hide` storable.

## Responsibilities
- Register and unregister camera pre/post render hooks for the relevant VR eye cameras.
- Choose the closest person whose head cylinder contains the HMD and hide the right materials/accessories for that person.
- Restore transient hide state cleanly on possession clears and scene-settle boundaries.

## Dependencies And Coupling
- Uses `PassengerRuntime` to skip duplicate hide behavior during passenger hand possession.
- Borrowed concepts and some behavior from `ImprovedPoV` and must stay compatible with its hair/material handling.

## References
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Head-zone geometry or camera filters change.
- Material/hair restore paths or passenger skip rules change.
- Any shader-resolution or retry logic changes.
