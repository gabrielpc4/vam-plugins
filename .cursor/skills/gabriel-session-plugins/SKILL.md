---
name: gabriel-session-plugins
description: Specialize on Gabriel session plugins (scene settle, same-folder pulses, HUD Space release wiring, person-plugin merges). Use when editing `Custom/Scripts/Gabriel/session-plugins/**` or debugging Gabriel session-load behavior.
---

# Gabriel Session Plugins

## Session layout
- Read `Custom/Scripts/Gabriel/FEATURE.md` when changing
  `GabrielSessionPlugins.cslist` membership or orchestrator boundaries.

## Instructions
1. Read `Custom/Scripts/Gabriel/session-plugins/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/session-plugins/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
