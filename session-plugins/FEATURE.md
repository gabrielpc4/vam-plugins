# SceneControlSuite Session Plugins

## Purpose
Session bundle loaded from `SessionPlugins.cslist`. The host
`SessionOrchestrator` script drives scene loads: same-folder load
pulses, full scene-settle workflow, late feature ticks, HUD binding, and the
scene-settle release action for the shared hotkey dispatcher.

The same compile unit includes **DildoOnHands**, **`ClothingClassifier`** +
**`TriggerClothingRemover`**, **`SessionOrchestrator`**, and
**`../util/SameFolderLoadCheck.cs`** (path normalization plus idle vs load-folder
matching for **`SceneSettleRuntime`** and clothing touch-fall same-folder
suppression). Bootstrap injects only
`VaMLogClipboardHud.cslist` plus **`SessionPlugins.cslist`** (VaM does not
nest `.cslist` files; multiple scripts share one compile by listing their `.cs`
paths in this list).

## Live Files
- `SessionPlugins.cslist`
- `src/SessionOrchestrator.cs` *(session runtime; scene settle +
  built-in defaults; same-folder VR rig restore timing)*
- `src/SceneSettle.cs`
- `src/SameFolderVrHmdRestore.cs` *(OpenVR/Oculus same-folder preset hop:
  capture navigation rig pose, **`playerHeightAdjust`**, and monitor cam local euler
  on the idle→loading edge before **`PassengerRuntime.NotifySceneChanged`**; if
  passenger was active, SceneControlSuite reads **`PassengerRuntime`** pre-passenger snap;
  after load **`WaitForEndOfFrame`** plus a short reassert wins over preset rig /
  monitor rotation; then **`SuperController.SetSceneLoadPosition`**.)*
- `src/PlaybackHold.cs`
- `src/InitialExposureChange.cs`
- `../util/SameFolderLoadCheck.cs` *(path normalize + same-folder checker;
  shared with `Hud.cslist`)*
- `../features/dildo-on-hands/DildoOnHands.cs`
- `../features/clothing-interactions/ClothingClassifier.cs`
- `../features/clothing-interactions/TriggerClothingRemover.cs` *(VR trigger
  strip; static tick from orchestrator `LateUpdate`)*
- `../features/clothing-interactions/ClothingTouchFallOffDeferredMerge.cs`
  *(optional standalone; touch-fall merge is implemented inline on **`SessionOrchestrator`**)*

## Load Path
- Loaded as a session plugin by `Custom/Scripts/bootstrap/Bootstrap.cs`.
- `SessionOrchestrator` runs in `Update` / `LateUpdate` and detects
  same-folder load pulses before delegating scene-settle control to the
  `SceneSettleRuntime` partial set in `src/SceneSettle.cs`,
  `src/PlaybackHold.cs`, and `src/InitialExposureChange.cs`.

## Responsibilities
- Track scene load edges and same-folder load pulses.
- On OpenVR/Oculus preset loads flagged same-folder versus SceneControlSuite&apos;s idle
  folder, **`SameFolderVrHmdRestore`** captures rig pose on that edge (passenger:
  **`PassengerRuntime.TryGetRigPoseForSameFolderRestore`** yields the pose stored
  when passenger mode started); when **`SuperController.isLoading`** next clears it
  runs after **`WaitForEndOfFrame`** and briefly re-applies across a few Unity
  frames (so SceneControlSuite wins over VaM preset rig / **`monitorCameraRotation`**
  restore), then **`SuperController.SetSceneLoadPosition`** if still idle in VR,
  skipping when passenger active or pending.
- Hold simulation/audio/exposure during scene settle through
  `SceneSettleRuntime` and same-folder load guards.
- Prime the shared per-frame Person possession snapshot before late feature
  ticks fan out to hand, palm-HUD, and toy helpers.
- Call `PassengerRuntime.LateTick` after `PassengerRuntime.Tick` in
  `LateUpdate` so passenger eye proxies update post-animation `Update`.
- Release transient head-hide state after scene settle completes.
- Register the emergency `Space` release action consumed by the HUD hotkey
  dispatcher.
- Indirectly drives initial `UserPreferences.softPhysics` via
  `SoftPhysicsScenePreference` whenever `AnimationNoLoopDetection` resolves motion
  state (long non-loop off; exception paths and other scenes on).

## Dependencies And Coupling
- Depends on `src/SceneSettle.cs`, `src/PlaybackHold.cs`,
  `src/InitialExposureChange.cs`, and `../util/SameFolderLoadCheck.cs` (Hud
  shares the latter).
- Calls `Custom/Scripts/features/head-hide/HeadProximityHide.cs` after
  the settle hold ends.
- Exposes a `JSONStorableAction` on `CoreControl` so the HUD plugin can
  trigger the emergency release without duplicating settle logic.

## References
- `Custom/Scripts/FEATURE.md` (tree-wide bootstrap / session bundle)
- `Reference/VaM-Scene-Startup-And-Settle.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Scene-settle timing, same-folder guards, or startup keybindings change.
- `SceneSettleRuntime` responsibility boundaries or the session-plugins file split
  changes.
- Action wiring or the `Space` shortcut behavior changes.
- Session-level defaults coupled to **`SoftPhysicsScenePreference`** /
  animation no-loop detection (e.g. soft physics on scene eval).
