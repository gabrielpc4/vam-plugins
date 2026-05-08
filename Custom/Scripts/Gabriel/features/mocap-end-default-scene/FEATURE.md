# Gabriel mocap-end default scene

## Purpose
Detects non-looping timeline / scene motion playback on the main
`SuperController.motionAnimationMaster` and loads `Saves/scene/Default.json` once
after playback ends (delayed in realtime), unless the load/save dir path matches
the booty-shake exception bucket. **No E-Motion coupling** — this is standalone
scene-motion policy.

## Live Files
- `NonLoopMocapMainEnd.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- Late-ticked from `GabrielHud` with JSON toggles on that plugin; deferred load
  coroutine runs on the same plugin host.

## Dependencies And Coupling
- `GabrielHud` owns the user toggles (saved on the HUD preset) and starts the
  delayed load coroutine after `NonLoopMocapMainEnd.LateTick` detects end.
- Shares long non-loop scene qualification helpers with `spankings` grip-merge
  guard logic and `clothing-interactions` (`ClothingTouchFallOffGripMerge`
  nested in `Hands/ClothingTouchFallOff.cs`).

## References
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/spankings/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- End detection, delay seconds, target JSON path, or booty-shake exclusion rules.
- Interaction with other features that read `CurrentSceneUsesLongNonLoopMocap` / grip
  merge blocking.
