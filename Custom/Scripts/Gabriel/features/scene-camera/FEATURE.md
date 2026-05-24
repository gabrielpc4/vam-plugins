# Gabriel Scene Camera

## Purpose
Camera and scene-load quality-of-life helpers attached to the HUD runtime, plus the bridge into the offline scene-camera patch scripts.

## Live Files
- `SceneCameraPatch.cs`
- `MonitorModeLaserRestore.cs`
- `FluidCumHideDuringSceneLoad.cs`
- `DefaultMonitorCameraFov.cs`

## Load Path
- Compiled in **`GabrielSessionPlugins.cslist`** (mirrored by
  `GabrielHud.cslist`).
- `SceneCameraPatch` writes request data into `Custom/Scripts/Gabriel/tools/scene-camera/` and can auto-run the Python patcher.

## Responsibilities
- Restore desktop/monitor aim cylinders for the expected input gestures.
- Tick `PassengerLaserPossess` when beams run so **right beam + face A**
  can start passenger on a lit hit person (face A confirm uses
  `SuperController.GetRightSelect` via `VrInput`, same as OpenVR UI suppression).
- Keep the left beam visual-only; only the right beam resolves person hits.
- Refresh the right-beam closest-person target at a relaxed cadence while the
  beam stays active, and invalidate it as soon as the beam hides.
- Hide DillDoe fluid mesh until scene load settles.
- Nudge standalone monitor-camera FOV from VaM defaults to Gabriel preference.
- Capture camera/rig snapshots from the C hotkey and route them into the
  offline patch scripts.
- **`[CameraRig]`** rotation uses center-eye world euler with **euler Z (roll)
  stripped to 0** before patching.
- Python strips root **`playerNavCollider`** when present so VaM does not overwrite rig rotation from physical-floor tracking.

## Dependencies And Coupling
- Shares scene-settle assumptions with `session-plugins` and `SceneSettleRuntime`.
- `MonitorModeLaserRestore` calls into `passenger-possession/PassengerLaserPossess.cs` for shared beam hit tests.
- Beam hit tests use a shared non-alloc closest-person scan so monitor lasers do
  not allocate every frame while aiming.
- Depends on `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md` for the offline script side.

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md`
- `Reference/VaM-Camera-Initial-Scene-Pose.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Camera-patch (C-hotkey) request payload or Python script paths change.
- Monitor laser triggers change.
- Fluid-cum reveal timing or load guards change.
