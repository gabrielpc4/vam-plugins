---
name: gabriel-scene-camera-tools
description: Specialize on Gabriel's offline scene-camera patch scripts and their request/log contract with the K-hotkey runtime. Use when editing `Custom/Scripts/Gabriel/tools/scene-camera/**` or debugging the camera patch tooling.
---

# Gabriel Scene Camera Tools

## Session layout
- Offline Python only; `Custom/Scripts/Gabriel/FEATURE.md` places this under
  `tools/scene-camera/`.

## Common references
- Read `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the scene-camera-tools feature doc's `References` section to pull
  neighboring Gabriel docs when offline patch behavior depends on runtime
  scene-camera contracts.

## Optimization notes
- No scene-camera-tools-only runtime optimization entry exists yet.
- Still consult the optimization history when K-hotkey runtime behavior or
  cached beam/camera timing explains the tool contract.

## Instructions
1. Read `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   a runtime-side cache, throttle, or deferred check could explain the tooling
   contract.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Keep runtime-side `.cs` changes C# 6 compatible and use `SuperController`
   APIs instead of reflection; for tool-side Python, keep request/log contracts
   aligned with the documented runtime integration points.
5. Update `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
