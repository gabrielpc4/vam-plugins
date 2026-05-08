---
name: gabriel-dildo-on-hands
description: Specialize on Gabriel Dildo On Hands, right-thumbstick toy spawning, catalog cloning, and hand-local toy pose corrections. Use when editing `Custom/Scripts/Gabriel/features/dildo-on-hands/**` or debugging VR toy spawning.
---

# Gabriel Dildo On Hands

## Session layout
- `DildoOnHands.cs` is listed inside `GabrielSessionPlugins.cslist` (see
  `Custom/Scripts/Gabriel/FEATURE.md`); `DildoOnHands.cslist` is optional toy-only.

## Common references
- Read `Custom/Scripts/Gabriel/features/dildo-on-hands/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the dildo-on-hands feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into hands or shared possession behavior.

## Optimization notes
- The possession guard uses the shared per-frame person possession snapshot.
- Legacy type preference uses a single-pass scene type cache instead of nested
  scene scans.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/dildo-on-hands/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   toy-spawn behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/dildo-on-hands/FEATURE.md` in the
   same turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
