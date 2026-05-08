# Looks Menu Page 6 Rewire Note

This file is now a current-state note for the migrated Gabriel scene-menu
tooling.

## Status

The old dedicated wrapper script
`Custom/Scripts/Easy Mate/tools/rewire_looks_menu_page_scene.py`
is not part of the Gabriel tree anymore.

The scene-menu tools that survived the cutover now live under:

- `Custom/Scripts/Gabriel/tools/scene-menu/rewire_hub_scene_button.py`
- `Custom/Scripts/Gabriel/tools/scene-menu/inject_default_scene_thumbnails.py`
- `Custom/Scripts/Gabriel/tools/scene-menu/localize_hub_button_labels.py`
- `Custom/Scripts/Gabriel/tools/scene-menu/player_text_maps/**`

## What this means

- There is no single current Gabriel script that recreates the full old
  page-6 wrapper workflow end-to-end.
- `rewire_hub_scene_button.py` is the closest current hub-button rewire tool
  for `Default.json` and `MainMenu.json`.
- `inject_default_scene_thumbnails.py` is the current thumbnail sync tool.
- If you need `PersonLooksMenu*.json` rewrites beyond the existing cutover,
  treat that as a separate explicit scene-menu task.

## Current policy

- Offline scene/menu rewrites should only run when the user explicitly asks for
  them.
- Keep all examples and docs in this folder aligned with the Gabriel path
  `Custom/Scripts/Gabriel/tools/scene-menu/`.
- Do not revive the removed Easy Mate wrapper name in new docs or scripts
  unless a real replacement script is added back to the repo.
