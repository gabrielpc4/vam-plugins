# Gabriel Clothing Interactions

## Purpose
All Gabriel-owned clothing-touch, strip, and proximity-strip helpers — plus the
TouchFallOff **person plugin** (`ClothingTouchFallOffGripMerge` lives in that
same file).

## Live Files
- `Clothing.cslist` *(session compile: `ClothingKeywords.cs` +
  `TriggerClothingRemover.cs`; loaded by bootstrap — not merged into
  `GabrielSessionPlugins.cslist` because VaM does not nest cslists)*
- `ClothingTouchFallOff.cs` *(person-only plugin; includes
  `ClothingTouchFallOffGripMerge`; loaded from this `.cs` path, not a cslist)*
- `ClothingKeywords.cs` *(keywords, torso band `ClothingTorsoBand` /
  `ClothingTorsoBandPicker`, and garment text heuristics — shared)*
- `TriggerClothingRemover.cs` *(VR grab trigger removes nearest torso-band
  garment when Male2 hands are active)*

HUD grip/orbit/overlap companions live under parallel `features/hands/` —
see **`Custom/Scripts/Gabriel/features/hands/FEATURE.md`**.

## Load Path
- `Clothing.cslist` is merged as its own session plugin by `GabrielBootstrap`
  (after log clipboard and `GabrielSessionPlugins.cslist`).
- HUD merges **only** `ClothingTouchFallOff.cs` onto Person atoms (single-file
  plugin path; avoids instantiating `ClothingTouchFallOff` on CoreControl, which
  would happen if it were listed beside `TriggerClothingRemover` in the same
  session cslist).

## Responsibilities
- Enable clothing fall-off on nearby garments when hands contact a person.
- Strip clothing bands from scene persons when Male2 VR hands grab near the torso.
- Defer ClothingTouchFallOff merge on grip when long non-loop motion rules qualify
  (see **`features/hands/GripHandVisibility`** callbacks into HUD).
- Proximity-strip session/person glue for VR-assisted band removal paths.

## Dependencies And Coupling
- Grip merge queues use `GabrielHud` helpers and **`AnimationNoLoopDetection`**.
- Shares behavior notes with **`features/hands`** and **`animation-no-loop-detection`**.
- `ClothingTouchFallOff` is a managed person-plugin path via `GabrielHud`, not the
  session bundle.

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
