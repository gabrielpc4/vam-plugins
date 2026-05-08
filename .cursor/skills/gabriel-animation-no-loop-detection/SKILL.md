---
name: gabriel-animation-no-loop-detection
description: Specialize on Gabriel long non-loop scene detection, deferred `Saves/scene/Default.json` loading, exception path rules, and related merge-block timing. Use when editing `Custom/Scripts/Gabriel/features/animation-no-loop-detection/**` or debugging long-animation scene end behavior.
---

# Gabriel Animation No Loop Detection

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap
  context.

## Common references
- Read
  `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the animation-no-loop feature doc's `References` section to pull
  neighboring Gabriel docs when changes cross into session-plugins, clothing,
  or spankings behavior.

## Optimization notes
- Long non-loop scene qualification is cached once per scene and per
  min-length setting.
- After the estimated clip end, completion rechecks happen every 3 seconds.

## Instructions
1. Read
   `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`
   before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   animation-end behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update
   `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md` in
   the same turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
