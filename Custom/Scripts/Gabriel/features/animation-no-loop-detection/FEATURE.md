# Gabriel animation no-loop detection (Default scene load)

## Purpose
Detects non-looping timeline / scene motion playback on the main
`SuperController.motionAnimationMaster` and loads `Saves/scene/Default.json` once
after playback ends (delayed in realtime), unless the load/save dir path matches
the booty-shake exception bucket. **No E-Motion coupling** — this is standalone
scene-motion policy.

## Live Files
- `AnimationNoLoopDetection.cs`

## Load Path
- Compiled in **`GabrielSessionPlugins.cslist`** (with `GabrielHud` and
  `GabrielSessionOrchestrator`). Long non-loop / Default.json **JSON storables**
  live on **`GabrielSessionOrchestrator`**, not on `GabrielHud`.
- `GabrielSessionOrchestrator.LateUpdate` calls
  `AnimationNoLoopDetection.LateTick` and starts deferred Default.json loads when
  enabled.
- `LateTick` reuses one scene motion scan per frame for qualification and
  end-of-clip detection.

## Dependencies And Coupling
- `GabrielSessionOrchestrator` owns the user toggles for this policy (saved on the
  orchestrator plugin preset).
- Shares long non-loop scene qualification helpers with `spankings` grip-merge
  guard logic and `clothing-interactions`
  (`ClothingTouchFallOffGripMerge` in `ClothingTouchFallOffDeferredMerge.cs`).

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/spankings/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- End detection, delay seconds, target JSON path, or booty-shake exclusion rules.
- Interaction with other features that read
  `CurrentSceneUsesLongNonLoopAnimation` / grip merge blocking.
