# Gabriel Passenger Possession

## Purpose
Passenger-style body possession: align the rig to a target person, prepare
ImprovedPoV, then delayed VR hand possession. **Entry** is the **right** UI-aim
beam + face A (`PassengerLaserPossess` + `MonitorModeLaserRestore`); **exit** uses palm
**Despossuir** (`PassengerRuntime.RequestStopForPalmHud`).

## Live Files
- `PassengerRuntime.cs`
- `PassengerLaserPossess.cs`
- `PassengerHandPrePossessSnapshot.cs`
- `PassengerPossessableNarrow.cs`
- `SceneLoadPossessionCleanup.cs`

## Load Path
- Compiled in **`GabrielSessionPlugins.cslist`** (mirrored by
  `GabrielHud.cslist`).
- Start: `PassengerLaserPossess` → `PassengerRuntime.RequestPassengerForSpecificPerson`.
- Stop: palm HUD via `GabrielHud` / `PassengerRuntime`.

## Responsibilities
- Pick target persons, align the navigation rig, and preserve/restore head and rig state.
- Prepare `ImprovedPoV` on the target and suppress duplicate head-hide behavior where needed.
- Delay VR hand possession until a later grip/trigger confirmation step.
- Raycast along the **right** UI-aim beam and map the closest hit person for laser+A start.
- While active on a target Person, **eyewear** garments matched by
  `ClothingClassifier.IsPassengerSunglassesClothing` are turned **off** and
  **restored** on passenger stop using **`geometry.SetActiveClothingItem`**
  (`PassengerRuntime` +
  `PersonAtomCache.TryGetCharacterSelector`; see **`features/clothing-interactions`**
  &quot;Turning garments off&quot;—never assign **`DAZClothingItem.active`** alone).
- Laser+A confirm uses **`SuperController.GetRightSelect`** (via shared VR input), so Oculus skips the same frames as SteamVR when VaM is consuming right-hand select for **VR UI** (e.g. Edit mode menus / buttons).
- Reuse the beam target for a short interval while the right beam stays active.
- Clear stale passenger-style possession after unrelated scene loads (`SceneLoadPossessionCleanup`).
- Snap the rig with a forward offset along the head rigidbody
  (`PositionOffsetZMeters`). While active, repoint `lEye`/`rEye`
  `LookAtWithLimits` at runtime proxies along **world-horizontal** torso **yaw**
  from each socket (same eye height as the socket, not pitched toward the
  chest) so gaze stays character-forward when you turn your HMD (restored on
  stop). **`Eyes` (`EyesControl`)** is set to **LookMode.None** during passenger
  so it does not retarget the eyes; every in-scene **`AnimationPattern`**
  whose **`MoveProducer`** **receiver** is this Person&apos;s
  **`eyeTargetControl`** or **`headControl`** has that receiver **cleared**
  (same as Receiver **None** in the pattern UI) so curve playback does not
  drive those free controllers; **`MotionAnimationControl`** timeline can still
  target them if present. Links are **restored** on stop.
  Proxy positions refresh in **`PassengerRuntime.LateTick`** (after animation
  `Update`).
- While active, `headControl` tracks the HMD via
  **`FreeControllerV3.AlignTo`** on `centerCameraTarget` (same as VaM head
  possession: **`PossessForwardAxis`** / **`PossessUpAxis`**). The **Rotation**
  gizmo still behaves as **X** pitch, **Y** yaw, **Z** roll; see
  [HeadControl rotation (VaM)](#headcontrol-rotation-vam).

## HeadControl rotation (VaM)

For Person atoms, **`headControl` → Rotation** in the VaM UI behaves as:

| Axis | Role |
|------|------|
| **X** | Pitch — nod **up / down** |
| **Y** | Yaw — turn **left / right** |
| **Z** | Roll — head **tilt** (ear toward shoulder) |

Passenger runtime calls **`headControl.AlignTo(centerCameraTarget, …)`** — the
same **`FreeControllerV3.AlignTo`** path VaM uses when possessing the head:
it honors **`PossessForwardAxis`** / **`PossessUpAxis`** (nose is not always +Z).
Using **`LookRotation(HMD.forward, …)`** alone assumed Unity **`forward`** was
the look axis and could leave the mesh yaw ~90° off while the rig view stayed
correct.

## Initial HMD tilt (passenger start)

Before `headControl` is neutralized, **`ComputePassengerSignedHeadPitchVsTorso`**
records signed **nod** vs the torso horizontal plane (same construction as
in-game head pitch relative to chest, not world Euler).

The **first** navigation rig snap uses **chest-flat forward** for **horizontal**
facing (VaM **R.Y** / where the chest points), **ignoring** head twist on that
axis if the model was looking sideways. **Pitch** (up/down) matches the
person’s starting head nod in the torso frame (**`AngleAxis` around
`Cross(torsoUp, chestFlat)`**, not `Euler` on `LookRotation`, which skewed
tilt), plus optional `RotationOffsetXDegrees`. After the delta multiply,
roll is stripped with **`LookRotation(rigForward, snapUp)`** — **`snapUp`**
uses torso **up** unless it is nearly parallel to **rig forward** (e.g. supine,
looking at the ceiling), in which case **pre-snap play-space** **`navigationRig.up`**
is projected instead so Unity does not pick an arbitrary ~90° roll.
The old `eulerAngles.z = 0` hack broke pitch/yaw.

## Dependencies And Coupling
- Depends on `improved-pov/ImprovedPoV.cs` and cooperates with `HeadProximityHide`.
- Uses `ClothingClassifier` / `PersonAtomCache` to hide matching eyewear on the
  passenger target during possession.
- Cooperates with `scene-camera` (`MonitorModeLaserRestore`), `NextSceneUiButton`
  (palm next-scene row), `palm-hud`, and `GabrielHud` / `PassengerRuntime`.

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Reference/EasyMate-Hand-Menu-Passenger-Possession.md`
- `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`
- `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Target selection, startup alignment, hand possession timing, or laser+A rules change.
- Eyewear hide/restore (`SetActiveClothingItem`) or `IsPassengerSunglassesClothing`
   rules change.
- Initial torso pitch capture, head-follow **`AlignTo`** path, eye **`EyesControl`** /
  **`AnimationPattern` / `MoveProducer`** head & eye receiver quarantine, or
  **`LateTick`** proxy
  updates.
- ImprovedPoV prep or restore behavior changes.
- Any preserved-state fields or narrow-possess filters change.
