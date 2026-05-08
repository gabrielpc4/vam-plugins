# Gabriel Scene Camera Tools

## Purpose
Offline Python tooling used by the K-hotkey scene-camera workflow. These scripts patch scene JSON camera values based on the request payload emitted by the runtime feature.

## Live Files
- `patch_scene_initial_camera.py`
- `patch_scene_camera_manual_cli.py`

## Load Path
- Run manually from the shell or automatically from `Custom/Scripts/Gabriel/features/scene-camera/SceneCameraPatch.cs`.
- Use `last_scene_camera_patch_request.json` and `last_scene_camera_patch_log.txt` as generated sidecars, not as canonical source files.

## Responsibilities
- Patch target scene JSON files from a captured camera request payload.
- Support both auto-target and manual CLI workflows for camera patching.

## Dependencies And Coupling
- The request payload shape is owned by `SceneCameraPatch.cs`; both sides must stay synchronized.

## References
- `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md`
- `Reference/VaM-Camera-Initial-Scene-Pose.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- CLI arguments, request schema, or generated sidecar file names change.
- The K-hotkey runtime expects new or removed payload fields.
