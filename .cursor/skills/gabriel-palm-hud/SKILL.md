---
name: gabriel-palm-hud
description: Specialize on the Gabriel VR palm HUD, HMD-relative euler pose checks, controller input abstraction, and optional VR gestures. Use when editing `Custom/Scripts/Gabriel/features/palm-hud/**` or debugging palm HUD behavior.
---

# Gabriel Palm HUD

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for session bundle / bootstrap context.

## Common references
- Read `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the palm-HUD feature doc's `References` section to pull neighboring
  Gabriel docs when changes cross into hands, passenger, or head-hide behavior.

## Optimization notes
- Palm-HUD possession gating depends on the shared per-frame person possession
  snapshot.
- Cross-check hands and head-hide timing before changing palm-HUD input gates.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   palm-HUD behavior could be explained by caches, throttles, or deferred
   checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md` in the same
   turn whenever behavior, owned files, load paths, inputs, settings,
   dependencies, or optimization assumptions change.
