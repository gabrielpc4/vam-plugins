# Gabriel Scene Menu Tools

## Purpose
Offline Python utilities for hub/menu scene rewires, thumbnail injection, localization, and scene cleanup. They are not runtime plugins and should only touch scene JSON when the user explicitly asks for scene-menu edits.

## Live Files
- `hub_scene_labels_pt_br.py`
- `inject_default_scene_thumbnails.py`
- `localize_hub_button_labels.py`
- `offset_non_thumb_image_panels_back.py`
- `patch_gato87_hey_mama_scene.py`
- `restore_json_baks_and_translate_text.py`
- `rewire_hub_scene_button.py`
- `sapuzex_double_uibutton_text_canvas_width.py`
- `strip_scene_expression_randomizer.py`
- `strip_scene_person_clothing.py`
- `translate_vam_scene_player_text_pt_br.py`
- `vam_scene_browser_urls.py`
- `player_text_maps/**`
- `LOOKS-MENU-SCENE-REWIRE.md`

## Load Path
- Manual Python tooling only; nothing in this folder is auto-loaded by VaM.
- Run scripts from the VaM root using the path under `Custom/Scripts/Gabriel/tools/scene-menu/`.

## Responsibilities
- Patch hub/menu scene JSON files, labels, thumbnails, and player-facing translated text.
- Keep the moved script docstrings/examples aligned with the Gabriel folder path instead of the removed `scene_menu_tools` root.
- Avoid expanding scene rewrites unless the user explicitly asks for them.

## Dependencies And Coupling
- This tool area is offline-only, but it edits some of the same menu scenes that bootstrap depends on.
- `LOOKS-MENU-SCENE-REWIRE.md` is a historical note that should not drift away from the current script set.

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/bootstrap/FEATURE.md`
- `Custom/Scripts/Gabriel/tools/scene-menu/LOOKS-MENU-SCENE-REWIRE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Scripts are added, removed, renamed, or moved.
- Any script docstring/example still references the removed `scene_menu_tools` root.
- Scene-edit scope changes and the docs need to reflect the new policy.
