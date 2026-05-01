# Rewire “Looks menu page 6” (hub + previews) to a CG-STUDIO scene

This documents how the **Easy Mate hub** (`Default.json` / `MainMenu.json`) and related **Looks menu** JSON files point **Page 6** at a packaged scene under `Saves/scene/CG-STUDIO/…`, and how the **cube preview images** on `PersonLooksMenu_6.json` use a **JPG** next to the VAR.

## What gets changed

| File | What |
|------|------|
| `Saves/scene/Default.json` | `UIButton` **`RightTopB6`** → `SceneLoader` / `LoadScene` / `sceneFilePath` |
| `Saves/scene/MainMenu.json` | Same **`RightTopB6`** block |
| `Saves/scene/PersonLooksMenu_5.json` | Button **“PAGE 6”** → same `sceneFilePath` (next page from page 5) |
| `Saves/scene/PersonLooksMenu.json` | **`plugin#0_geesp0t.ResetVROrientation`** → `Additional Button Scene` (“Looks Page 6”) |
| `Saves/scene/PersonLooksMenu_6.json` | Same plugin field + all **`ImagePanelEmissive`** `Image` / `url` entries (six faces) |

## Path conventions (VaM)

- **Hub / UIButton `LoadScene`** (paths relative to **`Saves/scene/`**):  
  `"./CG-STUDIO/<Package>/<Inner>/<Scene>.json"`  
  Example: `./CG-STUDIO/A Dressing Room/A Dressing Room/A Dressing Room.json`
- **Plugin `Additional Button Scene`**: full path from VaM root:  
  `Saves/scene/CG-STUDIO/<Package>/…`
- **Preview `url` on image panels**:  
  `Saves/scene/CG-STUDIO/<Package>/<Package>.jpg`  
  (first try; the script also tries a few other common layouts—see script help.)

## Discovery rule (script)

1. List **top-level folders** under `Saves/scene/CG-STUDIO/`.
2. Keep folders whose name **contains** your query (case-insensitive), e.g. `dressing` → `A Dressing Room`.
3. If several match, the script lists them; use **`--pick N`** or a **longer `--query`**.
4. **Main scene JSON** (default):  
   `Saves/scene/CG-STUDIO/<Folder>/<Folder>/<Folder>.json`  
   i.e. same name repeated: `…/A Dressing Room/A Dressing Room/A Dressing Room.json`.
5. **Preview JPG** (default):  
   `Saves/scene/CG-STUDIO/<Folder>/<Folder>.jpg`

If your VAR layout differs, use **`--scene-json`** and **`--preview-jpg`** to override.

## One-command workflow

From repo root (or pass **`--va-root`**):

```bash
python "Custom/Scripts/Easy Mate/tools/rewire_looks_menu_page_scene.py" --query "dressing"
```

Dry run (no writes):

```bash
python "Custom/Scripts/Easy Mate/tools/rewire_looks_menu_page_scene.py" --query "dressing" --dry-run
```

## Script location

`Custom/Scripts/Easy Mate/tools/rewire_looks_menu_page_scene.py`

## What to tell the AI (Cursor)

Use a **substring** of the **CG-STUDIO folder name** (not necessarily the inner `.json` name). The script resolves paths and patches the five JSON files.

**Dry run (recommended first):**

```text
Run: python "Custom/Scripts/Easy Mate/tools/rewire_looks_menu_page_scene.py" --va-root "D:\Games\VaM" --query "dressing" --dry-run
```

**Apply (after paths look correct):**

```text
Run: python "Custom/Scripts/Easy Mate/tools/rewire_looks_menu_page_scene.py" --va-root "D:\Games\VaM" --query "dressing"
```

**Other useful invocations:**

- List packages: `--list-packages`
- Exact folder: `--package "A Dressing Room"`
- Ambiguous name: `--query "control" --pick 0`
- Non-standard layout: `--scene-json "CG-STUDIO/…/Scene.json"` and `--preview-jpg "Saves/scene/…/thumb.jpg"`

If multiple folders match `--query`, the script prints numbered choices and exits until you pass **`--pick N`** or a longer query.

## Requirements

- **Python 3.8+** on PATH.
- Scene pack under **`Saves/scene/CG-STUDIO/<Package>/`** with the expected `.json` (and ideally `.jpg`).
- If **`CG-STUDIO/`** is gitignored, the script still edits **`Saves/scene/*.json`** on disk; commit those JSON changes if you track them.

## Troubleshooting

- **“No CG-STUDIO folder”**: create `Saves/scene/CG-STUDIO` or pass a different **`--cg-subdir`** (advanced).
- **“No match for query”**: run with a substring of the **folder** name under `CG-STUDIO`, or `--list-packages`.
- **“Main scene JSON not found”**: use **`--scene-json`** with a path relative to `Saves/scene/` (e.g. `CG-STUDIO/MyPack/MyPack/MyPack.json`).
- **Preview missing**: add **`Package.jpg`** next to the inner scene folder, or pass **`--preview-jpg`**.
