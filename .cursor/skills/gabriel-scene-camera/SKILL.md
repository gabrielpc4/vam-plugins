---
name: gabriel-scene-camera
description: Specialize on Gabriel scene-camera helpers, same-folder pose retention, monitor laser restoration, fluid-cum load hiding, and the K-hotkey patch bridge. Use when editing `Custom/Scripts/Gabriel/features/scene-camera/**` or diagnosing camera/scene-load behavior.
---

# Gabriel Scene Camera

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap context.

## Common references
- Read `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the scene-camera feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into passenger, HUD, or offline camera tools.

## Optimization notes
- The left monitor beam stays visual-only.
- The right-beam person target is cached while active and cleared when the beam
  hides.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   scene-camera behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
