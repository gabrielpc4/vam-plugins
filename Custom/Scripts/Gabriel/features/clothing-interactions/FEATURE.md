# Gabriel Clothing Interactions

## Purpose
All Gabriel-owned clothing-touch, strip, and VR-hand clothing helpers. This area mixes one session plugin, one per-person plugin, and two helper modules compiled into the HUD stack.

## Live Files
- `ClothingTouchFallOff.cs`
- `ClothingTouchFallOff.cslist`
- `VrProximityStripClothing.cs`
- `VrProximityStripClothingPlugin.cs`
- `VrProximityStripClothing.cslist`
- `GripHandVisibility.cs`
- `OverlapFullGrabRelease.cs`
- `ClothingTouchFallOffGripMerge.cs`

## Load Path
- `GabrielBootstrap` loads `VrProximityStripClothing.cslist` as a session plugin.
- `GabrielSessionStack` and HUD routines merge `ClothingTouchFallOff.cslist` onto Person atoms.
- `GripHandVisibility`, `OverlapFullGrabRelease`, and `ClothingTouchFallOffGripMerge`
  are compiled into `GabrielHud.cslist`.

## Responsibilities
- Enable clothing fall-off on nearby garments when hands contact a person.
- Strip clothing bands from scene persons when Male2 VR hands grab near the torso.
- Toggle Male2 vs sphere/none VR hand models and trigger first-grip merge side effects.
- Release stuck overlap full-grabs through public `SuperController` / `FreeControllerV3` paths.

## Dependencies And Coupling
- `GripHandVisibility` wires Spankings and Clothing deferred merges owned by `ui-hud`
  helpers and shares long non-loop motion gates with `mocap-end-default-scene`.
- `ClothingTouchFallOff` is a managed person plugin path inside `session-stack`.

## References
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/session-stack/FEATURE.md`
- `Custom/Scripts/Gabriel/features/mocap-end-default-scene/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Any cslist load path changes.
- Grip-trigger merge side effects or Male2 hand assumptions change.
- Band classification, proximity thresholds, or fall-off heuristics change.
