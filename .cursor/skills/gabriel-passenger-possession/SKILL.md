---
name: gabriel-passenger-possession
description: Specialize on the Gabriel passenger possession runtime, head/body alignment, hand pre-possess snapshots, and possessable narrowing. Use when editing `Custom/Scripts/Gabriel/features/passenger-possession/**` or debugging passenger mode.
---

# Gabriel Passenger Possession

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap context.

## Common references
- Read `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the passenger-possession feature doc's `References` section to pull
  neighboring Gabriel docs when changes cross into scene-camera, head-hide, or
  palm-HUD behavior.

## Optimization notes
- Right-beam closest-person resolution can stay cached for about 1 second while
  the beam stays active.
- Passenger behavior often depends on scene-camera beam timing and head-hide
  suppression behavior.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   passenger behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update
   `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
