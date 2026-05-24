---
name: gabriel-scene-menu-tools
description: Specialize on Gabriel's offline scene-menu and hub patch scripts, thumbnail injection, localization helpers, and player text maps. Use when editing `Custom/Scripts/Gabriel/tools/scene-menu/**` or debugging scene-menu tooling.
---

# Gabriel Scene Menu Tools

## Session layout
- Offline Python only; `Custom/Scripts/Gabriel/FEATURE.md` places this under
  `tools/scene-menu/`.

## Common references
- Read `Custom/Scripts/Gabriel/tools/scene-menu/FEATURE.md`,
  `Custom/Scripts/Gabriel/FEATURE.md`, and
  `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md`.
- Use the scene-menu-tools feature doc's `References` section to pull
  neighboring Gabriel docs when offline menu behavior depends on runtime HUD or
  scene behavior.

## Optimization notes
- No scene-menu-tools-only runtime optimization entry exists yet.
- Still consult the optimization history when cached HUD or scene timing
  explains the generated menu output.

## Instructions
1. Read `Custom/Scripts/Gabriel/tools/scene-menu/FEATURE.md` before changing this area.
2. Read `Custom/Scripts/Gabriel/docs/RUNTIME-OPTIMIZATION-HISTORY.md` whenever
   a runtime-side cache, throttle, or deferred check could explain the tool
   contract.
3. Read linked feature docs from that file when the change crosses feature
   boundaries.
4. Keep runtime-side `.cs` changes C# 6 compatible and use `SuperController`
   APIs instead of reflection; for tool-side Python, keep request/log contracts
   aligned with the documented runtime integration points.
5. Keep hub `Default.json` and `MainMenu.json` identical whenever menu hub JSON
   changes (see `tools/scene-menu/FEATURE.md`; use
   `sync_hub_default_mainmenu.py` if they drift).
6. Update `Custom/Scripts/Gabriel/tools/scene-menu/FEATURE.md` in the same turn
   whenever behavior, owned files, load paths, inputs, settings, dependencies,
   or optimization assumptions change.
