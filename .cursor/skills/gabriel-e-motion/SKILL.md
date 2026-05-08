---
name: gabriel-e-motion
description: Specialize on Gabriel E-Motion path-keyword logic and deferred path-rule merges (`EmotionPathRuleMerge`). Use when editing `Custom/Scripts/Gabriel/features/e-motion/**` or debugging Lite auto-merge triggers. For post-animation Default.json policy, use `features/animation-no-loop-detection/`.
---

# Gabriel E-Motion Hooks

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap context.

## Common references
- Read `Custom/Scripts/Gabriel/features/e-motion/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the E-Motion feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into animation-no-loop or deferred merge
  behavior.

## Optimization notes
- No dedicated E-Motion runtime optimization entry exists yet.
- Still consult the optimization history when path-rule changes affect deferred
  merges or post-animation behavior.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/e-motion/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   E-Motion behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/e-motion/FEATURE.md` in the same turn
   whenever behavior, owned files, load paths, inputs, settings, dependencies,
   or optimization assumptions change.
