# SceneControlSuite runtime optimization history

## Purpose

This file records performance-motivated SceneControlSuite runtime changes so future
regressions can be mapped back to the most likely optimization.

This is not a full changelog. It only tracks the optimization work introduced
by the May 2026 SceneControlSuite runtime audit and follow-up tuning.

## Commits

- `e179fee` - `perf(gabriel): reduce repeated runtime scans`
- `719c074` - `perf(gabriel): throttle person targeting and animation polls`

## How to use this file

If something starts feeling stale, delayed, missing, or only updates after a
short wait, check the matching area below first.

Most of these optimizations fall into one of these buckets:

- Cached person and controller lookups shared by multiple features.
- Throttled target refresh timers instead of continuous recalculation.
- Deferred rechecks instead of per-frame polling.

## Shared person snapshots and controller caches

### Main files

- `Custom/Scripts/util/PersonAtomCache.cs`
- `Custom/Scripts/session-plugins/src/SessionOrchestrator.cs`
- `Custom/Scripts/features/hands/GripHandVisibility.cs`
- `Custom/Scripts/features/palm-hud/VrEulerPossessHandHud.cs`
- `Custom/Scripts/features/dildo-on-hands/DildoOnHands.cs`
- `Custom/Scripts/features/clothing-interactions/TriggerClothingRemover.cs`
- `Custom/Scripts/features/clothing-interactions/ClothingTouchFallOff.cs`
- `Custom/Scripts/features/head-hide/HeadProximityHide.cs`

### What changed

- Added a shared per-frame active-person possession snapshot.
- Added shared cached `FreeControllerV3` lookup helpers for hot paths.
- The session orchestrator primes the per-frame snapshot early in
  `LateUpdate()`.
- Several features now reuse the shared active-person list instead of walking
  `SuperController.GetAtoms()` again.

### If something looks wrong, suspect this area

- A feature reacts one frame late to a possession change.
- A controller lookup seems stale only during the frame an atom changes state.
- A person-targeting feature behaves oddly after unusual atom reload or
  replacement flows.

## Beam target resolution and passenger laser start

### Main files

- `Custom/Scripts/features/passenger-possession/PassengerLaserPossess.cs`
- `Custom/Scripts/features/scene-camera/MonitorModeLaserRestore.cs`

### What changed

- Replaced `Physics.RaycastAll()` plus sort with a reusable
  `Physics.RaycastNonAlloc()` buffer on the normal path.
- Left monitor beam stays visual-only and no longer resolves person hits.
- Right beam closest-person resolution now refreshes at most once every
  1 second while the beam stays active.
- Right-beam target cache clears when the beam hides.

### If something looks wrong, suspect this area

- Passenger laser keeps the person that was under the beam up to about
  1 second ago.
- Quickly sweeping the beam between nearby people does not switch the target
  immediately.
- A bug only happens while the right beam stays continuously visible.

## Head proximity hide target resolution

### Main files

- `Custom/Scripts/features/head-hide/HeadProximityHide.cs`

### What changed

- Head-hide target resolution now reuses the shared active-person list and
  cached head/chest controller lookups.
- The closest head target refreshes on a short timer, about every 200 ms,
  instead of recalculating on every render callback.
- `ImprovedPoV` suppression checks remain cached so repeated render callbacks
  do not rescan plugin storables every time.

### If something looks wrong, suspect this area

- Head hide swaps between nearby people with a short delay.
- Hide or unhide reacts up to about 200 ms later than before.
- A problem only appears during fast HMD movement near multiple heads.

### Intentional limit

This path was not stretched to a 1 second refresh. That was judged too likely
to make hide behavior visibly stale.

## Animation no-loop detection and exception path rules

### Main files

- `Custom/Scripts/features/animation-no-loop-detection/AnimationNoLoopDetection.cs`

### What changed

- Long non-loop scene qualification is computed once per scene and per
  min-length setting.
- The maximum clip length is cached instead of rescanning every tick.
- After playback first advances, the feature waits until the estimated clip end
  before checking completion.
- If the clip is still not done, it rechecks every 3 seconds instead of every
  frame.
- Internal `BootyShake` naming was renamed to `Exception`.
- The actual scene-path token match stayed the same: `"booty shake"`.

### If something looks wrong, suspect this area

- `Saves/scene/Default.json` loads later than before at animation end.
- Grip-merge blocking around long non-loop scenes behaves differently near the
  end.
- A scene with unusual playback jumps, pauses, or timing never reaches the
  expected delayed completion check.

## UI next-scene trigger resolution cache

### Main files

- `Custom/Scripts/features/ui-hud/NextSceneUiButton.cs`

### What changed

- Expensive trigger lookup is cached.
- Failed lookups retry on a timer instead of every request.
- Cache invalidates on host bind, host release, and atom UID changes.

### If something looks wrong, suspect this area

- A next-scene button added dynamically is not found immediately.
- Trigger discovery only succeeds after the retry window or after scene atom
  changes.

## Clothing interaction scan reductions

### Main files

- `Custom/Scripts/features/clothing-interactions/TriggerClothingRemover.cs`
- `Custom/Scripts/features/clothing-interactions/ClothingTouchFallOff.cs`

### What changed

- Strip selection now walks the shared active-person list instead of doing a
  fresh atom scan.
- Garments that already had fall-off enabled are memoized and skipped on later
  periodic scans until clothing changes invalidate the cache.
- The VR strip path no longer does a separate every-frame
  "does any person have clothing?" scene precheck.

### If something looks wrong, suspect this area

- Strip/remove only starts seeing a person after the current frame refresh.
- Clothing fall-off enable does not rescan until clothing slot or activity
  changes invalidate the memoized state.

## Dildo On Hands scan reductions

### Main files

- `Custom/Scripts/features/dildo-on-hands/DildoOnHands.cs`
- `Custom/Scripts/features/dildo-on-hands/DildoOnHands.cslist`

### What changed

- Possession guard now reuses the shared per-frame possession snapshot.
- Legacy type preference now builds one set of scene types instead of nesting
  scene scans.
- The cslist was updated so the shared cache helper is compiled with the
  feature.

### If something looks wrong, suspect this area

- Toy spawn guard behaves differently during the exact frame possession changes.
- Legacy type preference behaves differently if atom types are changed or cloned
  mid-flow.

## Improved PoV render-path reductions

### Main files

- `Custom/Scripts/features/improved-pov/ImprovedPoV.cs`

### What changed

- Render hooks now register only while the effect is active.
- Small render-path LINQ and lambda overhead was replaced with explicit loops.
- Active hair matching and refresh now use loop-based helpers instead of LINQ.

### If something looks wrong, suspect this area

- First-person visual effects are missing because render hook active state is
  wrong.
- Hair visibility cache does not refresh when effect activation changes in an
  unexpected order.

## Intentional non-change

No single global "closest person" cache was added for all SceneControlSuite systems.

Different systems mean different things by "closest":

- Closest beam hit along a ray.
- Nearest torso to a hand for stripping.
- Closest head zone containing the HMD.

Sharing one answer across all those systems would risk wrong behavior more than
it would save.

## Quick suspicion guide

- Stale beam target or delayed passenger laser switch:
  suspect `719c074`.
- Head-hide lag or short-delay person swaps:
  suspect `719c074`.
- Long animation end detection or delayed Default scene load:
  suspect `719c074`.
- One-frame possession or controller-cache issues:
  suspect `e179fee`.
- Next-scene trigger discovery timing:
  suspect `e179fee`.
- Improved PoV effect hook enable or disable timing:
  suspect `e179fee`.
