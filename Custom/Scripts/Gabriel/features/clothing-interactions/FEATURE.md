# Gabriel Clothing Interactions

## Purpose
All Gabriel-owned clothing-touch, strip, and proximity-strip helpers — plus the
TouchFallOff **person plugin**. Deferred grip merge lives in
`ClothingTouchFallOffDeferredMerge.cs` (session bundle).

## Live Files
- `ClothingTouchFallOff.cs` *(person-only `MVRScript`)*
- `ClothingTouchFallOffDeferredMerge.cs`
  *(`ClothingTouchFallOffPluginPath`, `ClothingTouchFallOffGripMerge` —
  `GabrielSessionPlugins.cslist`)*
- `ClothingClassifier.cs` *(unified `Keywords`, `TorsoBand`, `Text`, strip
  pick and `ClassifyTorsoBand`; also listed in `GabrielSessionPlugins.cslist` with
  `TriggerClothingRemover`)*
- `TriggerClothingRemover.cs` *(VR grab trigger removes nearest torso-band
  garment when Male2 hands are active; session bundle via
  `GabrielSessionPlugins.cslist`)*

HUD grip/orbit/overlap companions live under parallel `features/hands/` —
see **`Custom/Scripts/Gabriel/features/hands/FEATURE.md`**.

## Load Path
- `ClothingClassifier.cs` and `TriggerClothingRemover.cs` compile inside
  **`GabrielSessionPlugins.cslist`** (same CoreControl plugin as orchestrator +
  HUD). Bootstrap only injects that `.cslist` plus log clipboard — no separate
  **`.cslist`** file for strip (`Clothing.cslist` removed).
- **ClothingTouchFallOff** person merge path is
  `ClothingTouchFallOffPluginPath.PersonPlugin` in
  `ClothingTouchFallOffDeferredMerge.cs`; **MergeClothingTouchFallOffOnAllPersonsOnly**
  runs on **`GabrielSessionOrchestrator`** (grip-deferred queue via
  `ClothingTouchFallOffGripMerge` in the same file).
- HUD merges **only** when other features need it; touch-fall is **not** a HUD
  button and is not configured as a plugin path constant on `GabrielHud`.

## Responsibilities
- Enable clothing fall-off on nearby garments when hands contact a person.
- Memoize garments whose fall-off is already enabled until clothing slots or
  active counts change.
- Strip clothing bands from scene persons when Male2 VR hands grab near the
  torso, without doing a scene-wide idle precheck every frame.
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
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/features/hands/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Any cslist load path changes.
- Touch fall-off thresholds or garment scan rules change.
- Band classification / proximity-strip behavior changes.
