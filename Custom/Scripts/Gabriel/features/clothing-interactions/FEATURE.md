# Gabriel Clothing Interactions

## Purpose
All Gabriel-owned clothing-touch, strip, and proximity-strip helpers — plus the
TouchFallOff **person plugin** (`ClothingTouchFallOffGripMerge` lives in that
same file).

## Live Files
- `ClothingTouchFallOff.cs` *(person plugin; includes `ClothingTouchFallOffGripMerge`)*
- `ClothingTouchFallOff.cslist`
- `ClothingKeywords.cs` *(strip band keywords and garment text heuristics — shared)*
- `VrProximityStripClothing.cs`
- `VrProximityStripClothingPlugin.cs`
- `VrProximityStripClothing.cslist`

HUD grip/orbit/overlap companions live under parallel `features/hands/` —
see **`Custom/Scripts/Gabriel/features/hands/FEATURE.md`**.

## Load Path
- `GabrielBootstrap` loads `VrProximityStripClothing.cslist` as a session plugin.
- `GabrielSessionPlugins` and HUD routines merge `ClothingTouchFallOff.cslist`
  onto Person atoms.

## Responsibilities
- Enable clothing fall-off on nearby garments when hands contact a person.
- Strip clothing bands from scene persons when Male2 VR hands grab near the torso.
- Defer ClothingTouchFallOff merge on grip when long non-loop motion rules qualify
  (see **`features/hands/GripHandVisibility`** callbacks into HUD).
- Proximity-strip session/person glue for VR-assisted band removal paths.

## Dependencies And Coupling
- Grip merge queues use `GabrielHud` helpers and **`AnimationNoLoopDetection`**.
- Shares behavior notes with **`features/hands`** and **`animation-no-loop-detection`**.
- `ClothingTouchFallOff` is a managed person-plugin path inside `session-plugins`.

## References
- `Custom/Scripts/Gabriel/features/hands/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Any cslist load path changes.
- Touch fall-off thresholds or garment scan rules change.
- Band classification / proximity-strip behavior changes.
