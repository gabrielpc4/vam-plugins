---
name: gabriel-improved-pov
description: Specialize on Gabriel's vendored Improved PoV plugin, first-person possession rendering, and the repo-specific integration points that depend on it. Use when editing `Custom/Scripts/Gabriel/features/improved-pov/**` or debugging Improved PoV behavior.
---

# Gabriel Improved PoV

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for how person plugins relate to the
  session bundle.

## Common references
- Read `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the Improved PoV feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into head-hide or passenger behavior.

## Optimization notes
- Render hooks register only while the effect is active.
- Hair refresh and active-hair matching use explicit loop-based helpers instead
  of render-path LINQ.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   Improved PoV behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
