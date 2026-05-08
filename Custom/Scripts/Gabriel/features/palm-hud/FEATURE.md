# Gabriel Palm HUD

## Purpose
VR-only hand HUD layer. It decides when the right-hand palm UI appears and how
A/B/menu inputs are interpreted for **Despossuir**, **Próxima cena**, and related
rows. Passenger **start** is not from the palm (see `passenger-possession` +
`scene-camera` **right** UI-aim laser + face A).

## Live Files
- `VrEulerPossessHandHud.cs`
- `VrEulerPossessPoseCheck.cs`
- `VrInput.cs`
- `LEGACY-NOTES.md`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- `GabrielHud` ticks the palm HUD. **Próxima cena** uses
  `GabrielHudNextSceneButton`; **Despossuir** uses `PassengerRuntime`.

## Responsibilities
- Show/hide the right-hand palm HUD from HMD-relative euler windows.
- Map Quest/OpenVR face buttons, menu, select, and grip-trigger abstractions through `VrInput`.
- Show **Despossuir** and **Próxima cena** when applicable; no gender submenu.
- Keep the right-hand back-of-hand pose window aligned with the current palm HUD behavior.

## Dependencies And Coupling
- Calls into **`GabrielHudNextSceneButton`**, **`PassengerRuntime`**, and legacy
  palm-adjacent paths in **`GabrielHudButtons`** where still needed.
- `LEGACY-NOTES.md` contains the deeper handoff details that should stay aligned with this feature doc.

## References
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/palm-hud/LEGACY-NOTES.md`
- `Reference/EasyMate-next-scene-hand-hud-handoff.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Palm HUD pose windows or button mappings change.
- Hand-pose visibility rules or which rows appear change.
- `LEGACY-NOTES.md` becomes inconsistent with current runtime behavior.
