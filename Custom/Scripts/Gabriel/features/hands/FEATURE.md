# Gabriel hands (HUD grip helpers)

## Purpose
HUD-compiled VR hand / overlap helpers shared by Gabriel HUD routing: articulated
Male2 toggles vs sphere proxy, deferred first-grip Spankings merges, overlap
full-grab auto-release without reflection.

## Live Files
- `GripHandVisibility.cs`
- `OverlapFullGrabRelease.cs`
- `SpankingsGripDeferredMerge.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- Sources sit under `features/hands/`; no separate VaM `.cslist` entry.

## Dependencies And Coupling
- Invoked from `GabrielHud` / `GabrielHotkeys`; clothing touch-fall deferral
  routes to **`GabrielSessionOrchestrator`** via `GripHandVisibility` and
  `ClothingTouchFallOffGripMerge` in
  **`clothing-interactions/ClothingTouchFallOffDeferredMerge.cs`** (session
  bundle).
- Uses palm-hud input, passenger runtime,
  animation-no-loop-detection heuristics, and
  `SpankingsGripBlockPathKeywords`.

## References
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`
- `Custom/Scripts/Gabriel/features/spankings/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever behavior, HUD wiring paths, grip
overlap rules, or load path assumptions change.
