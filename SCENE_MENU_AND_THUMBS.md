# Scene menu, main hub tiles, and thumbnails

This VaM repo keeps a **frozen reference** hub (`MainMenu_Original.json`) and a **working** hub (`Default.json`). When a tile’s `LoadScene` target differs from the reference, tooling can add a preview **`ImagePanelEmissive`** (`_SceneThumb_<ButtonId>`) next to that button.

## Goal: “Put scene *Lose name of the scene* on button `TopLeftB7`”

**Intended automation (e.g. an LLM agent):**

1. **Read this file** so you know the two commands below and the file roles.
2. **Find the scene** under `Saves/scene/`: the user may describe it loosely. Search by folder name, `.json` basename, or keywords; open or verify the `.json` path. Note the path **relative to** `Saves/scene/` (forward slashes).
3. **Rewire the hub** — edit ``Default.json`` (button `LoadScene` + **`Text`** label), then **`MainMenu.json` is replaced with a copy of `Default.json`** so both files always match.

   ```bash
   python scene_menu_tools/rewire_hub_scene_button.py --button-id "TopLeftB7" --scene-json "REL/PATH/Scene.json"
   ```

   Use **`--dry-run`** first if you want to print actions without writing. VaM root is found automatically (folder containing `Saves/scene`), or pass **`--va-root`**.

   Other scene forms (if you already have a hub-style path or a nested pack layout) are documented under **`rewire_hub_scene_button.py`** below.

4. **Regenerate thumbnail panels** — this creates or refreshes **`_SceneThumb_*`** `ImagePanelEmissive` atoms **only** for buttons whose `LoadScene` **differs** from **`MainMenu_Original.json`** (same button `id`):

   ```bash
   python scene_menu_tools/inject_default_scene_thumbnails.py
   ```

   Preview images come from the scene folder (co-located `.jpg`/`.jpeg` or newest in folder); see that script’s section for rules.

5. **Commit** if the user uses git.

## Key files

| File | Role |
|------|------|
| `Saves/scene/MainMenu_Original.json` | **Baseline**: reference `sceneFilePath` per hub `UIButton` `id`. The injector compares `Default.json` to this. |
| `Saves/scene/Default.json` | Working main hub: `UIButton` atoms; `_SceneThumb_<ButtonId>` panels sit **immediately before** the matching button. |
| `Saves/scene/MainMenu.json` | **Copy of** `Default.json` after **`rewire_hub_scene_button.py`** runs (same bytes). |
| `scene_menu_tools/` | Run helpers from **VaM / repo root**. Use this folder name — not `tools` — because `.gitignore` has `Tools/` (VaM export) and Windows paths are case-insensitive. |

## Scripts (project root `scene_menu_tools/`)

### `rewire_hub_scene_button.py`

**Purpose:** Update **`Default.json`** for **one** hub `UIButton` **`id`** (`sceneFilePath` + **`Text`**). Then **copy `Default.json` → `MainMenu.json`** so the two hub scenes stay identical. **`--button-id` is required.**

Button **`Text`** format:

- **Line 1:** scene name = basename of the hub load target (see pack rules below).
- **Pack ``.vac`` vs paired ``.json``:** if the scene ``.json`` lives under a pack folder (e.g. ``CG-STUDIO/Amnesia/...``) and that pack folder contains a chosen ``*.vac`` (same stem match / tie-break as in the script), the hub normally loads that ``.vac``. **If** ``<vac-stem>.json`` exists **in the same pack folder**, the hub loads that ``.json`` instead (VaM can load either; JSON wins when both names exist).
- **`Type:`** only if the button `id` starts with **`Left`** (`Dance`) or **`Front`** (`Story`). **`Right*`** buttons omit the type line.
- **`Author:`** if a string is found in the first 64KiB of the scene JSON (`author`, `creator`, `credits`, etc.), else if the path under `Saves/scene` has at least two segments, the **second** path segment (e.g. `_DANCE/AuthorName/.../scene.json` → `AuthorName`). Omitted if it would duplicate the title.

Exactly **one** scene target:

| Flag | Meaning |
|------|---------|
| **`--scene-json REL`** | Path under `Saves/scene`; hub value becomes `./REL` |
| **`--hub-rel "./…"`** | Literal hub string; file must exist under `Saves/scene` |
| **`--pack-name`** + **`--pack-under`** | Resolves `Saves/scene/<pack-under>/NAME/NAME/NAME.json` (`--pack-under .` = top level of `Saves/scene`) |
| **`--list-pack-dirs`** + **`--pack-under`** | List subfolders (discovery only) |

```bash
python scene_menu_tools/rewire_hub_scene_button.py --list-pack-dirs --pack-under .
python scene_menu_tools/rewire_hub_scene_button.py --button-id "TopLeftB7" --scene-json "Author/Pack/Scene.json" --dry-run
python scene_menu_tools/rewire_hub_scene_button.py --button-id "TopLeftB7" --scene-json "Author/Pack/Scene.json"
```

### `inject_default_scene_thumbnails.py`

- Inserts/refreshes **`_SceneThumb_*`** `ImagePanelEmissive` atoms **only** where **`LoadScene` ≠ `MainMenu_Original.json`** for that `id`.
- Removes `_SceneThumb_*` when the path matches the baseline again, or when there is no usable JPEG.
- **Preview image**: prefers `<same folder as load target (``.json`` or ``.vac``)>/<same basename>.jpg` (or `.jpeg`), else **newest** `.jpg`/`.jpeg` in that folder. If still none, **no** thumbnail atom.
- **Pose**: matches button `position`/`rotation`, offset **1 cm** back along local −Z (constants in the script).

```bash
python scene_menu_tools/inject_default_scene_thumbnails.py
python scene_menu_tools/inject_default_scene_thumbnails.py --dry-run
```

Writes `Saves/scene/Default.json.bak` when not dry-run.

### `offset_non_thumb_image_panels_back.py`

Moves **non-thumbnail** `ImagePanelEmissive` atoms (not `id` starting with `_SceneThumb_`) by **`OFFSET_M`** along local **−Z**. Default **`OFFSET_M = 0`**. Edit the file to nudge banners, etc.

```bash
python scene_menu_tools/offset_non_thumb_image_panels_back.py --dry-run
```

## Removed / obsolete

- **`default_scene_thumb_overrides.json`** and per-scene URL overrides in the injector — **removed**.

## Legacy path

Previous location: `Custom/Scripts/Easy Mate/tools/`. Use **`scene_menu_tools/`** at repo root here.
