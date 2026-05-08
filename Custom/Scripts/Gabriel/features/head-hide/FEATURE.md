# Gabriel Head Hide

## Purpose
VR-only transient head hide that hides face, hair, hats, and glasses when the HMD enters a person head cylinder. It is the Gabriel-specific hide layer that cooperates with passenger mode and ImprovedPoV.

## Live Files
- `HeadProximityHide.cs`

## Load Path
- Compiled in **`GabrielSessionPlugins.cslist`** (mirrored by
  `GabrielHud.cslist`).
- **VR head proximity hide** JSON storable is registered on
  **`GabrielSessionOrchestrator`**; hotkeys and HUD still route into
  `HeadProximityHide` static hooks.

## Responsibilities
- Register and unregister camera pre/post render hooks for the relevant VR eye cameras.
- Choose the closest person whose head cylinder contains the HMD and hide the right materials/accessories for that person.
- Resolve the head-hide target once per frame per probe source and reuse that result across pre/post render callbacks.
- Restore transient hide state cleanly on possession clears and scene-settle boundaries.

## Dependencies And Coupling
- Uses `PassengerRuntime` to skip duplicate hide behavior during passenger hand possession.
- Borrowed concepts and some behavior from `ImprovedPoV` and must stay compatible with its hair/material handling.
- Caches `ImprovedPoV` passenger-suppression checks per frame so repeated render callbacks do not rescan plugin storables.

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Head-zone geometry or camera filters change.
- Material/hair restore paths or passenger skip rules change.
- Any shader-resolution or retry logic changes.
