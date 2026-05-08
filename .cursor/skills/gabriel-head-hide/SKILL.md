---
name: gabriel-head-hide
description: Specialize on Gabriel VR head proximity hide, camera render hooks, and the temporary face/hair/accessory hiding pipeline. Use when editing `Custom/Scripts/Gabriel/features/head-hide/**` or debugging head-hide behavior.
---

# Gabriel Head Hide

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap context.

## Common references
- Read `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the head-hide feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into improved-pov, passenger, or scene-camera
  behavior.

## Optimization notes
- Closest head target refresh is intentionally timer-based, about 200 ms.
- This area reuses shared active-person and controller caches with other hot
  paths.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   head-hide behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
