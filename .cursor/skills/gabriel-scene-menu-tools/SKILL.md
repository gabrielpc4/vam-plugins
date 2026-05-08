---
name: gabriel-scene-menu-tools
description: Specialize on Gabriel's offline scene-menu and hub patch scripts, thumbnail injection, localization helpers, and player text maps. Use when editing `Custom/Scripts/Gabriel/tools/scene-menu/**` or debugging scene-menu tooling.
---

# Gabriel Scene Menu Tools

## Session layout
- Offline Python only; `Custom/Scripts/Gabriel/FEATURE.md` places this under
  `tools/scene-menu/`.

## Instructions
1. Read `Custom/Scripts/Gabriel/tools/scene-menu/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/tools/scene-menu/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
