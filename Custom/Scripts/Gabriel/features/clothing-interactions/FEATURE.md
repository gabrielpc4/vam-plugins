# Gabriel Clothing Interactions

## Purpose
All Gabriel-owned clothing-touch, strip, and proximity-strip helpers — plus the
TouchFallOff **person plugin**. Deferred grip merge lives in
`ClothingTouchFallOffDeferredMerge.cs` (session bundle).

## Live Files
- `Clothing.cslist` *(session compile: `ClothingClassifier.cs` +
  `TriggerClothingRemover.cs`; loaded by bootstrap — not merged into
  `GabrielSessionPlugins.cslist` because VaM does not nest cslists)*
- `ClothingTouchFallOff.cs` *(person-only `MVRScript`)*
- `ClothingTouchFallOffDeferredMerge.cs`
  *(`ClothingTouchFallOffPluginPath`, `ClothingTouchFallOffGripMerge` —
  `GabrielSessionPlugins.cslist`)*
- `ClothingClassifier.cs` *(unified `Keywords`, `TorsoBand`, `Text`, strip
  pick and `ClassifyTorsoBand` — shared)*
- `TriggerClothingRemover.cs` *(VR grab trigger removes nearest torso-band
  garment when Male2 hands are active)*

HUD grip/orbit/overlap companions live under parallel `features/hands/` —
see **`Custom/Scripts/Gabriel/features/hands/FEATURE.md`**.

## Load Path
- `Clothing.cslist` is merged as its own session plugin by `GabrielBootstrap`
  (after log clipboard and `GabrielSessionPlugins.cslist`).
- **ClothingTouchFallOff** person merge path is
  `ClothingTouchFallOffPluginPath.PersonPlugin` in
  `ClothingTouchFallOffDeferredMerge.cs`; **MergeClothingTouchFallOffOnAllPersonsOnly**
  runs on **`GabrielSessionOrchestrator`** (grip-deferred queue via
  `ClothingTouchFallOffGripMerge` in the same file).
- HUD merges **only** when other features need it; touch-fall is **not** a HUD
  button and is not configured as a plugin path constant on `GabrielHud`.

## Responsibilities
- Enable clothing fall-off on nearby garments when hands contact a person.
- Strip clothing bands from scene persons when Male2 VR hands grab near the torso.
- Defer ClothingTouchFallOff merge on grip when long non-loop motion rules qualify
  (see **`features/hands/GripHandVisibility`** → `GabrielSessionOrchestrator`).
- Proximity-strip session/person glue for VR-assisted band removal paths.

## Dependencies And Coupling
- Grip merge queues (Spankings / clothing touch-fall) talk to
  **`GabrielSessionOrchestrator`** and **`AnimationNoLoopDetection`**.
- Shares behavior notes with **`features/hands`** and **`animation-no-loop-detection`**.
- `ClothingTouchFallOff` person-plugin merge is orchestrated from
  **`GabrielSessionOrchestrator`**, not from HUD-owned plugin path constants.

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
