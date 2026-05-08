---
name: gabriel-ui-hud
description: Specialize on the Gabriel HUD, world-space buttons, hotkeys, next-scene resolution, plugin toggles, and log clipboard HUD. Use when editing `Custom/Scripts/Gabriel/features/ui-hud/**` or debugging Gabriel HUD behavior.
---

# Gabriel UI HUD

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for how the HUD/orchestrator bundle is
  compiled and bootstrapped.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
