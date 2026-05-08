---
name: gabriel-spankings
description: Specialize on Gabriel Spankings auto-merge block rules and path-keyword behavior. Use when editing `Custom/Scripts/Gabriel/features/spankings/**` or debugging first-grip Spankings merges.
---

# Gabriel Spankings Hooks

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap context.

## Common references
- Read `Custom/Scripts/Gabriel/features/spankings/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the spankings feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into hands, ui-hud, or animation-no-loop
  behavior.

## Optimization notes
- No Spankings-only runtime optimization entry exists yet.
- Grip-merge timing still depends on hands deferral and animation-no-loop
  timing rules documented in the optimization history.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/spankings/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   Spankings behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/spankings/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
