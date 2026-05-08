---
name: gabriel-bootstrap
description: Specialize on the Gabriel bootstrap session entry plugin, session plugin injection order, and initial menu scene wiring. Use when editing `Custom/Scripts/Gabriel/bootstrap/**` or diagnosing Gabriel startup and PluginManager loading.
---

# Gabriel Bootstrap

## Session layout
- Read `Custom/Scripts/Gabriel/FEATURE.md` when changing `GabrielBootstrap` injection
  URLs or order.

## Common references
- Read `Custom/Scripts/Gabriel/bootstrap/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the bootstrap feature doc's `References` section to pull neighboring
  Gabriel docs when startup changes cross into session runtime behavior.

## Optimization notes
- No bootstrap-only runtime optimization entry exists yet.
- Still check the optimization history when bootstrap order or injection timing
  changes cached, delayed, or throttled session behavior downstream.

## Instructions
1. Read `Custom/Scripts/Gabriel/bootstrap/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   startup behavior could be explained by caches, throttles, or deferred checks.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Preserve the current optimization assumptions unless the change is
   intentionally retuning them, then sync the history doc and affected
   `FEATURE.md` files in the same turn.
5. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead
   of forbidden desktop-only file APIs or reflection.
6. Update `Custom/Scripts/Gabriel/bootstrap/FEATURE.md` in the same turn
   whenever behavior, owned files, load paths, inputs, settings, dependencies,
   or optimization assumptions change.
