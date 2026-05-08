---
name: gabriel-hands
description: Specialize on Gabriel hand visibility, overlap full-grab auto-release, and deferred first-grip merge helpers. Use when editing `Custom/Scripts/Gabriel/features/hands/**` or debugging grip, overlap, or hand-visibility behavior.
---

# Gabriel Hands

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap
  context.

## Common references
- Read `Custom/Scripts/Gabriel/features/hands/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the hands feature doc's `References` section to pull neighboring Gabriel
  docs when changes cross into palm-hud, clothing, or spankings behavior.

## Optimization notes
- Hand visibility uses the shared per-frame person possession snapshot.
- Deferred grip behavior crosses into clothing, spankings, and
  animation-no-loop timing.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/hands/FEATURE.md` before changing this
   area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   hand behavior could be explained by caches, throttles, or deferred checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/hands/FEATURE.md` in the same turn
   whenever behavior, owned files, load paths, inputs, settings, dependencies,
   or optimization assumptions change.
