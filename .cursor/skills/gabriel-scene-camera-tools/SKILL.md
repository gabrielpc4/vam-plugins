---
name: gabriel-scene-camera-tools
description: Specialize on Gabriel's offline scene-camera patch scripts and their request/log contract with the K-hotkey runtime. Use when editing `Custom/Scripts/Gabriel/tools/scene-camera/**` or debugging the camera patch tooling.
---

# Gabriel Scene Camera Tools

## Session layout
- Offline Python only; `Custom/Scripts/Gabriel/FEATURE.md` places this under
  `tools/scene-camera/`.

## Instructions
1. Read `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
