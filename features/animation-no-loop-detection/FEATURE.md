# SceneControlSuite animation no-loop detection (Default scene load)

## Purpose
Detects non-looping timeline / scene motion playback on the main
`SuperController.motionAnimationMaster` and loads `Saves/scene/Default.json` once
after playback ends (delayed in realtime), unless the load/save dir path matches
the exception bucket. **No E-Motion coupling** — this is standalone
scene-motion policy.

On the first motion-state evaluation after each scene change, notifies
`SoftPhysicsScenePreference` so **`UserPreferences.softPhysics`** tracks the same
long non-loop vs exception policy (implementation lives under
**`features/soft-physics-preference/`**).

## Live Files
- `AnimationNoLoopDetection.cs`

## Load Path
- Compiled in **`SessionPlugins.cslist`** (with `Hud` and
  `SessionOrchestrator`). Long non-loop / Default.json **JSON storables**
  live on **`SessionOrchestrator`**, not on `Hud`.
- `SessionOrchestrator.LateUpdate` calls
  `AnimationNoLoopDetection.LateTick` and starts deferred Default.json loads when
  enabled.
- `LateTick` qualifies the scene once per scene/min-length setting, then waits
  until the estimated clip end before checking completion. If the clip is still
  not done, it rechecks every 3 seconds.

## Dependencies And Coupling
- `SessionOrchestrator` owns the user toggles for this policy (saved on the
  orchestrator plugin preset).
- After each recomputed motion state, calls **`SoftPhysicsScenePreference`**
  (prefs write; see **`features/soft-physics-preference/`**).
- Shares long non-loop scene qualification helpers with `spankings` grip-merge
  guard logic and `clothing-interactions`
  (`ClothingTouchFallOffGripMerge` in `ClothingTouchFallOffDeferredMerge.cs`).

## References
- `Custom/Scripts/FEATURE.md`
- `Custom/Scripts/session-plugins/FEATURE.md`
- `Custom/Scripts/features/ui-hud/FEATURE.md`
- `Custom/Scripts/features/spankings/FEATURE.md`
- `Custom/Scripts/features/clothing-interactions/FEATURE.md`
- `Custom/Scripts/features/soft-physics-preference/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- End detection, delay seconds, target JSON path, or exception exclusion rules.
- Coupling flags passed to **`SoftPhysicsScenePreference`** (semantics must stay
  aligned).
- Interaction with other features that read
  `CurrentSceneUsesLongNonLoopAnimation` / grip merge blocking.
