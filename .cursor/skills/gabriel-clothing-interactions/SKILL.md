---
name: gabriel-clothing-interactions
description: Specialize on Gabriel clothing-touch fall-off, proximity strip, and ClothingTouchFallOff person-plugin wiring. Grip visibility, overlap release, and Spankings deferral HUD sources live under `Custom/Scripts/Gabriel/features/hands/`. Use when editing `Custom/Scripts/Gabriel/features/clothing-interactions/**` or debugging those behaviors when they touch grip merge paths.
---

# Gabriel Clothing Interactions

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for `GabrielSessionPlugins.cslist`
  clothing rows vs person-only `ClothingTouchFallOff.cs`.

## Common references
- Read `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the clothing-interactions feature doc's `References` section to pull
  neighboring Gabriel docs when changes cross into hands, spankings, or
  animation-no-loop behavior.

## Optimization notes
- Strip targeting uses shared active-person snapshots instead of fresh scene
  scans.
- Clothing fall-off enable memoizes garments until slot or activity changes
  invalidate the state.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   clothing behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update
   `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md` in the
   same turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
