# SceneControlSuite Clothing Interactions

## Purpose
All SceneControlSuite-owned clothing-touch, strip, and proximity-strip helpers — plus the
TouchFallOff **person plugin**. Grip-triggered orchestrator merge (see
session-plugins) attaches the plugin only after the first articulated-hand enable
when the scene qualifies.

## Live Files
- `ClothingTouchFallOff.cs` *(person-only `MVRScript`)*
- `ClothingTouchFallOff.cslist` *(includes ``../../util/PersonAtomCache.cs`` so
  fall-off proximity can reuse cached free-controller reads)*
- `ClothingTouchFallOffDeferredMerge.cs` *(standalone script; inlined merge path
  is implemented on **`SessionOrchestrator`**, not referenced from session
  `*.cslist` today)*
- `ClothingClassifier.cs` *(unified `Keywords`, `TorsoBand`, `Text`, strip
  pick and `ClassifyTorsoBand`; also listed in `SessionPlugins.cslist` with
  `TriggerClothingRemover`)*
- `TriggerClothingRemover.cs` *(static VR trigger strip ticked from
  **`SessionOrchestrator.LateUpdate`**; session/HUD `*.cslist`)*

HUD grip/orbit/overlap companions live under parallel `features/hands/` —
see **`Custom/Scripts/features/hands/FEATURE.md`**.

## Load Path
- `ClothingClassifier.cs` and `TriggerClothingRemover.cs` compile inside
  **`SessionPlugins.cslist`** (same CoreControl plugin as orchestrator +
  HUD). Bootstrap only injects that `.cslist` plus log clipboard — no separate
  **`.cslist`** file for strip (`Clothing.cslist` removed).
- **ClothingTouchFallOff**: `SessionOrchestrator` merges the person
  plugin (`MergeClothingTouchFallOffOnAllPersonsOnly`) on first VR grip switch
  to Male2 hands when the scene includes a female with active clothing,
  **`AnimationNoLoopDetection`** permits, and the user has not already consumed
  that merge during the **same VaM load-folder batch** as other JSONs in that
  folder (cross-folder resets the batch).
- HUD merges **only** when other features need it; touch-fall is **not** a HUD
  button and is not configured as a plugin path constant on `Hud`.

## Turning garments off (SceneControlSuite convention)
- Turn matching garments **off** (and restore **on**) with
  **`DAZCharacterSelector.SetActiveClothingItem`** on that Person atom&apos;s
  **`geometry`** storable. VaM updates **`DAZClothingItem.active`**, the clothing
  Unity **`GameObject`**, clothing-selector UI storables (`val` parity), and
  runs **`SyncAnatomy`**.
- Do **not** assign only **`DAZClothingItem.active`** and assume the garment
  mesh disappears—in practice renders often linger.
- In-repo callers: **`TriggerClothingRemover`** (VR trigger strip path);
  **`PassengerRuntime`** (passenger eyewear on start → restore on stop after
  **`ClothingClassifier.IsPassengerSunglassesClothing`** classifies slots).

## Responsibilities
- `ClothingClassifier.IsPassengerSunglassesClothing` selects passenger-hide
  eyewear (blob **`sunglasses`** or **`exclusiveRegion.Glasses`**);
  **`PassengerRuntime`** disables and restores matching slots via **`geometry.SetActiveClothingItem`**
  (see **`features/passenger-possession`**).
- Enable clothing fall-off on nearby garments when hands contact a person.
- Memoize garments whose fall-off is already enabled until clothing slots or
  active counts change.
- Strip clothing bands from scene persons when Male2 VR hands grab near the
  torso, without doing a scene-wide idle precheck every frame.
- Retry ClothingTouchFallOff merge on subsequent Male2 enables only after a VaM
  load-folder navigation change (`AnimationNoLoopDetection` can defer the first
  attempt; **`features/hands/GripHandVisibility`** → orchestrator handles
  eligibility and same-folder suppression).
- Proximity-strip session/person glue for VR-assisted band removal paths.

## Dependencies And Coupling
- Grip merge queues (Spankings / clothing touch-fall) talk to
  **`SessionOrchestrator`** and **`AnimationNoLoopDetection`**.
- `TriggerClothingRemover` reuses
  **`PersonAtomCache.PersonHasAnyActiveClothingOnGeometry`** for both idle
  presence checks and nearest-strip eligibility.
- Shares behavior notes with **`features/hands`** and **`animation-no-loop-detection`**.
- `ClothingTouchFallOff` person-plugin merge is orchestrated from
  **`SessionOrchestrator`**, not from HUD-owned plugin path constants.

## References
- `Custom/Scripts/FEATURE.md`
- `Custom/Scripts/features/hands/FEATURE.md`
- `Custom/Scripts/features/ui-hud/FEATURE.md`
- `Custom/Scripts/session-plugins/FEATURE.md`
- `Custom/Scripts/features/animation-no-loop-detection/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Any cslist load path changes.
- Touch fall-off thresholds or garment scan rules change.
- Band classification / proximity-strip behavior changes.
- Passenger eyewear match rules (**`IsPassengerSunglassesClothing`**), how hides
  are applied via **`DAZCharacterSelector.SetActiveClothingItem`**, or the
  **Turning garments off** section accuracy change.
