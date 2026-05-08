---
name: gabriel-ui-hud
description: Specialize on the Gabriel HUD, world-space buttons, hotkeys, next-scene resolution, plugin toggles, and log clipboard HUD. Use when editing `Custom/Scripts/Gabriel/features/ui-hud/**` or debugging Gabriel HUD behavior.
---

# Gabriel UI HUD

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for how the HUD/orchestrator bundle is
  compiled and bootstrapped.

## Common references
- Read `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the UI HUD feature doc's `References` section to pull neighboring Gabriel
  docs when HUD changes cross into other feature areas.

## Optimization notes
- Next-scene trigger resolution is cached, and missing trigger lookups retry on
  a timer instead of every request.
- HUD behavior often crosses into session-plugins and scene-camera timing.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   HUD behavior could be explained by caches, throttles, or deferred checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md` in the same turn
   whenever behavior, owned files, load paths, inputs, settings, dependencies,
   or optimization assumptions change.
