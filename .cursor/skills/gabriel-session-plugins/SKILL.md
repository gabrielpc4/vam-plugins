---
name: gabriel-session-plugins
description: Specialize on Gabriel session plugins (scene settle, same-folder pulses, HUD Space release wiring, person-plugin merges). Use when editing `Custom/Scripts/Gabriel/session-plugins/**` or debugging Gabriel session-load behavior.
---

# Gabriel Session Plugins

## Session layout
- Read `Custom/Scripts/Gabriel/FEATURE.md` when changing
  `GabrielSessionPlugins.cslist` membership or orchestrator boundaries.

## Common references
- Read `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the session-plugins feature doc's `References` section to pull adjacent
  Gabriel docs when orchestrator changes cross feature boundaries.

## Optimization notes
- This area owns shared per-frame person snapshot priming through the
  orchestrator.
- Long non-loop animation timing and deferred `Default.json` loading also route
  through this session area.

## Instructions
1. Read `Custom/Scripts/Gabriel/session-plugins/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   session behavior could be explained by caches, throttles, or deferred checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/session-plugins/FEATURE.md` in the same turn
   whenever behavior, owned files, load paths, inputs, settings, dependencies,
   or optimization assumptions change.
