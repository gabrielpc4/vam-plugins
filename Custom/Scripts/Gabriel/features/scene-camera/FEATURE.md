# Gabriel Scene Camera

## Purpose
Camera and scene-load quality-of-life helpers attached to the HUD runtime, plus the bridge into the offline scene-camera patch scripts.

## Live Files
- `SceneCameraPatch.cs`
- `SameFolderCameraRetain.cs`
- `MonitorModeLaserRestore.cs`
- `FluidCumHideDuringSceneLoad.cs`
- `DefaultMonitorCameraFov.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- `SceneCameraPatch` writes request data into `Custom/Scripts/Gabriel/tools/scene-camera/` and can auto-run the Python patcher.

## Responsibilities
- Persist/reapply camera pose across same-folder loads.
- Restore desktop/monitor aim cylinders for the expected input gestures.
- Tick `PassengerLaserPossess` when beams run so **right beam + face A**
  can start passenger on a lit hit person.
- Hide DillDoe fluid mesh until scene load settles.
- Nudge standalone monitor-camera FOV from VaM defaults to Gabriel preference.
- Capture camera/rig snapshots from the K hotkey and route them into the offline patch scripts.

## Dependencies And Coupling
- Shares scene-settle assumptions with `session-plugins` and
  `SceneSettleRuntime`.
- `MonitorModeLaserRestore` calls into `passenger-possession/PassengerLaserPossess.cs` for shared beam hit tests.
- Depends on `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md` for the offline script side.

## References
- `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md`
- `Reference/VaM-Camera-Initial-Scene-Pose.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- K-hotkey request payload or Python script paths change.
- Same-folder retain logic or monitor laser triggers change.
- Fluid-cum reveal timing or load guards change.
