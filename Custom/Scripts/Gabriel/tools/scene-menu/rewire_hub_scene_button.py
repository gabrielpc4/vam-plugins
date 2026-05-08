#!/usr/bin/env python3
"""
Rewire **one** main-hub ``UIButton`` so it loads a chosen scene:

- Edits ``Saves/scene/Default.json`` only in memory, then writes it and **copies** it to
  ``Saves/scene/MainMenu.json`` so both hub files stay byte-for-byte identical.
- After resolving a scene ``.json``, if the **pack folder** (e.g. ``CG-STUDIO/Amnesia`` for
  ``.../Amnesia/Amnesia - CAMRIDE/Amnesia - CAMRIDE.json``) contains a ``.vac``, the script
  picks that ``.vac`` as the pack asset **unless** a ``.json`` with the **same basename**
  (same stem) exists beside it — then the hub loads that ``.json`` instead (button label stem
  matches that file).
- UIButton ``Text`` becomes **two-line pt-BR** (tipo, then nome) via
  ``hub_scene_labels_pt_br``.
- Runs ``inject_default_scene_thumbnails.py`` (unless ``--no-thumbnails``) so
  ``_SceneThumb_*`` overlays match LoadScene paths vs ``MainMenu_Original.json``;
  then copies ``Default.json`` → ``MainMenu.json`` again (inject only rewrites
  Default).

See: ``SCENE_MENU_AND_THUMBS.md``

Examples:
  python Custom/Scripts/Gabriel/tools/scene-menu/rewire_hub_scene_button.py --button-id TopLeftB7 \\
      --scene-json "Gato87/My Pack/Scene.json"
  python Custom/Scripts/Gabriel/tools/scene-menu/rewire_hub_scene_button.py --button-id TopLeftB7 \\
      --hub-rel "./Gato87/My Pack/Scene.json" --dry-run
  python Custom/Scripts/Gabriel/tools/scene-menu/rewire_hub_scene_button.py --list-pack-dirs --pack-under .
  python Custom/Scripts/Gabriel/tools/scene-menu/rewire_hub_scene_button.py --button-id RightTopB6 \\
      --pack-name "My Scene" --pack-under "Author/VaultPacks"
"""

from __future__ import annotations

import argparse
import json
import re
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Optional

_TOOLS_DIR = Path(__file__).resolve().parent
if str(_TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(_TOOLS_DIR))
from hub_scene_labels_pt_br import build_scene_hub_label_pt_br


def find_va_root(start: Path) -> Optional[Path]:
    for anc in [start.resolve(), *start.resolve().parents]:
        if (anc / "Saves" / "scene").is_dir():
            return anc
    return None


def list_subdirs(parent: Path) -> list[str]:
    if not parent.is_dir():
        return []
    return sorted(
        p.name for p in parent.iterdir() if p.is_dir() and not p.name.startswith(".")
    )


def nested_pack_scene_rel(pack_under: str, pack_name: str) -> Path:
    """``Saves/scene/<pack_under>/<name>/<name>/<name>.json`` (``pack_under`` may be ``.``)."""
    u = pack_under.strip().replace("\\", "/")
    u = u.strip("/")
    name = pack_name.strip()
    tail = Path(name) / name / f"{name}.json"
    if not u or u == ".":
        return tail
    return Path(u) / tail


def resolve_hub_scene(
    scene_root: Path,
    hub_rel: str | None,
    scene_json: str | None,
    pack_name: str | None,
    pack_under: str,
) -> tuple[str, Path]:
    """
    Returns (hub ``sceneFilePath`` for JSON, abs path to scene ``.json``).
    Hub uses VaM form ``./path/under/Saves/scene.json``.
    """
    n_kw = sum(x is not None and str(x).strip() != "" for x in (hub_rel, scene_json, pack_name))
    if n_kw != 1:
        raise SystemExit(
            "Exactly one of --hub-rel, --scene-json, or --pack-name is required "
            "(unless using --list-pack-dirs only)."
        )

    if pack_name is not None:
        rel = nested_pack_scene_rel(pack_under, pack_name)
    elif scene_json is not None:
        rel = Path(scene_json.strip().replace("\\", "/").lstrip("./"))
    else:
        hr = hub_rel.strip().replace("\\", "/")
        if not hr:
            raise SystemExit("--hub-rel is empty.")
        rel = Path(hr.lstrip("./"))

    abs_json = scene_root / rel
    if not abs_json.is_file():
        raise SystemExit(f"Scene JSON not found:\n  {abs_json}")
    hub = "./" + rel.as_posix()
    return hub, abs_json


def pack_root_dir_for_scene_json(abs_json: Path) -> Path:
    """
    Pack folder that may contain a ``.vac`` (same folder as a flat ``*.json``, or one
    level up from ``Name/Name.json``), e.g. ``.../CG-STUDIO/Amnesia``.
    """
    d = abs_json.parent
    if d.name == abs_json.stem:
        return d.parent
    return d


def pick_vac_in_pack_root(abs_json: Path, scene_root: Path) -> Optional[Path]:
    """If ``pack_root_dir_for_scene_json`` contains ``*.vac``, return the best match."""
    root = pack_root_dir_for_scene_json(abs_json)
    try:
        root.relative_to(scene_root)
    except ValueError:
        return None
    if root == scene_root or not root.is_dir():
        return None
    vacs = [p for p in root.iterdir() if p.is_file() and p.suffix.lower() == ".vac"]
    if not vacs:
        return None
    vacs.sort(key=lambda p: p.name.lower())
    if len(vacs) == 1:
        return vacs[0]
    stem = abs_json.stem
    for v in vacs:
        if v.stem == stem:
            return v
    lstem = stem.lower()
    for v in vacs:
        if lstem in v.stem.lower():
            return v
    return vacs[0]


def hub_and_label_paths(
    scene_root: Path, abs_json: Path, hub_rel_for_json: str
) -> tuple[str, Path, Path]:
    """
    Returns ``(hub sceneFilePath, path for button title stem, path for JSON author sniff)``.
    If a ``.vac`` exists in the pack root, hub targets that ``.vac`` **unless**
    ``<vac-stem>.json`` exists in the same folder — then hub targets that ``.json`` and
    author sniff uses it too. Otherwise author sniff still uses the originally resolved
    ``.json`` when hub uses ``.vac``.
    """
    vac = pick_vac_in_pack_root(abs_json, scene_root)
    if vac is None:
        return hub_rel_for_json, abs_json, abs_json

    paired_json = vac.with_suffix(".json")
    if paired_json.is_file():
        try:
            paired_json.relative_to(scene_root)
        except ValueError:
            pass
        else:
            hub = "./" + paired_json.relative_to(scene_root).as_posix()
            return hub, paired_json, paired_json

    hub = "./" + vac.relative_to(scene_root).as_posix()
    return hub, vac, abs_json


_JSON_AUTHOR_HEAD_BYTES = 65536


def author_from_rel_path_under_scene(rel_json: Path) -> str | None:
    """Second path segment under ``Saves/scene`` (or sole folder if only one deep)."""
    parts = rel_json.parts
    if len(parts) >= 3:
        return parts[1]
    if len(parts) == 2:
        return parts[0]
    return None


def extract_author_from_scene_json_head(abs_json: Path) -> str | None:
    try:
        with abs_json.open("r", encoding="utf-8", errors="ignore") as f:
            head = f.read(_JSON_AUTHOR_HEAD_BYTES)
    except OSError:
        return None
    for pat in (
        r'(?i)"author"\s*:\s*"([^"]+)"',
        r'(?i)"creator"\s*:\s*"([^"]+)"',
        r'(?i)"credits"\s*:\s*"([^"]+)"',
        r'(?i)"credit"\s*:\s*"([^"]+)"',
        r'(?i)"packageAuthor"\s*:\s*"([^"]+)"',
    ):
        m = re.search(pat, head)
        if m:
            s = m.group(1).strip()
            if s:
                return s
    return None


def resolve_label_author(abs_scene: Path, rel_under_scene: Path) -> str | None:
    a = extract_author_from_scene_json_head(abs_scene)
    if a:
        return a
    return author_from_rel_path_under_scene(rel_under_scene)


def replace_scene_after_button(text: str, button_id: str, new_hub_path: str) -> str:
    m = re.search(
        rf'("id"\s*:\s*"{re.escape(button_id)}"[\s\S]{{0,12000}}?)'
        r'("sceneFilePath"\s*:\s*)("[^"]*")',
        text,
    )
    if not m:
        raise ValueError(f'Could not find sceneFilePath after button id "{button_id}"')
    return text[: m.start(3)] + json.dumps(new_hub_path) + text[m.end(3) :]


def replace_text_storable_for_button(text: str, button_id: str, new_text: str) -> str:
    m = re.search(
        rf'("id"\s*:\s*"{re.escape(button_id)}"[\s\S]{{0,16000}}?)'
        r'("id"\s*:\s*"Text"[\s\S]*?"text"\s*:\s*)("[^"]*")',
        text,
    )
    if not m:
        raise ValueError(f'Could not find UIButton Text storables "text" after button id "{button_id}"')
    return text[: m.start(3)] + json.dumps(new_text) + text[m.end(3) :]


def extract_uibutton_text_from_raw_hub(text: str, button_id: str) -> str | None:
    """Current ``Text`` storables ``text`` JSON string for ``button_id``, or None."""
    m = re.search(
        rf'("id"\s*:\s*"{re.escape(button_id)}"[\s\S]{{0,16000}}?)'
        r'("id"\s*:\s*"Text"[\s\S]*?"text"\s*:\s*)("[^"]*")',
        text,
    )
    if not m:
        return None
    try:
        return json.loads(m.group(3))
    except json.JSONDecodeError:
        return None


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--va-root",
        type=Path,
        default=None,
        help="VaM install root (folder containing Saves/scene). Default: walk up from this script.",
    )
    ap.add_argument(
        "--list-pack-dirs",
        action="store_true",
        dest="list_pack_dirs",
        help="List immediate subfolders of Saves/scene/<pack-under> and exit. Requires --pack-under.",
    )
    ap.add_argument(
        "--button-id",
        default="",
        metavar="ID",
        help="UIButton id whose LoadScene to replace (required unless --list-pack-dirs).",
    )
    scene = ap.add_mutually_exclusive_group(required=False)
    scene.add_argument(
        "--hub-rel",
        default=None,
        metavar="PATH",
        help="Exact hub value, e.g. ./Author/Folder/Scene.json (relative to Saves/scene). File must exist.",
    )
    scene.add_argument(
        "--scene-json",
        default=None,
        metavar="REL_PATH",
        help="Path relative to Saves/scene to the .json (hub value becomes ./REL_PATH).",
    )
    scene.add_argument(
        "--pack-name",
        default=None,
        metavar="NAME",
        help="Scene path <pack-under>/NAME/NAME/NAME.json under Saves/scene. Requires --pack-under.",
    )
    ap.add_argument(
        "--pack-under",
        default="",
        metavar="REL",
        help="Folder under Saves/scene for --pack-name / --list-pack-dirs (use . for top level).",
    )
    ap.add_argument("--dry-run", action="store_true", help="Print actions; do not write files.")
    ap.add_argument(
        "--no-thumbnails",
        action="store_true",
        help="Skip inject_default_scene_thumbnails.py after editing Default.json.",
    )
    args = ap.parse_args()

    script_here = Path(__file__).resolve()
    va_root = args.va_root or find_va_root(script_here)
    if va_root is None:
        raise SystemExit("Could not find VaM root (no Saves/scene in parents). Pass --va-root.")

    scene_root = va_root / "Saves" / "scene"
    pu = args.pack_under.strip().replace("\\", "/")

    if args.list_pack_dirs:
        if not pu:
            raise SystemExit(
                "--pack-under is required with --list-pack-dirs (use . for Saves/scene top level)."
            )
        base = scene_root if pu == "." else scene_root / pu.lstrip("./")
        dirs = list_subdirs(base)
        print(f"{base}:\n" + "\n".join(dirs) if dirs else "(empty)")
        return 0

    if args.pack_name is not None and str(args.pack_name).strip():
        if not pu:
            raise SystemExit(
                "--pack-under is required with --pack-name (use . for NAME/NAME/NAME.json under Saves/scene)."
            )

    hub_rel_json, abs_scene = resolve_hub_scene(
        scene_root,
        args.hub_rel,
        args.scene_json,
        args.pack_name,
        args.pack_under,
    )

    bid = args.button_id.strip()
    if not bid:
        raise SystemExit("--button-id cannot be empty.")

    hub_rel, title_src, author_json_src = hub_and_label_paths(scene_root, abs_scene, hub_rel_json)
    rel_for_author = title_src.relative_to(scene_root)
    scene_title = title_src.stem
    author = resolve_label_author(author_json_src, rel_for_author)

    default_path = scene_root / "Default.json"
    main_menu_path = scene_root / "MainMenu.json"

    if not default_path.is_file():
        raise SystemExit(f"Missing hub scene: {default_path}")

    raw = default_path.read_text(encoding="utf-8")
    old_txt = extract_uibutton_text_from_raw_hub(raw, bid)
    label = build_scene_hub_label_pt_br(
        scene_title, bid, hub_rel, old_txt, ignore_frozen_prior=True
    )

    print(f"VaM root:        {va_root}")
    print(f"Button id:       {bid}")
    print(f"Scene JSON:      {abs_scene}")
    if hub_rel != hub_rel_json:
        if title_src.suffix.lower() == ".json":
            print(f"Pack hub uses paired JSON (same name as .vac): {title_src}")
        else:
            print(f"Pack .vac override: {title_src}")
    print(f"Hub sceneFilePath: {hub_rel}")
    print(f"Button label:\n{label}\n---")
    if author and author.strip():
        print(f"(Author from scene, not on label: {author.strip()})")

    try:
        out = replace_scene_after_button(raw, bid, hub_rel)
        out = replace_text_storable_for_button(out, bid, label)
    except ValueError as e:
        raise SystemExit(f"{default_path.name}: {e}") from e

    if args.dry_run:
        if out != raw:
            print(f"WOULD WRITE: {default_path}")
        else:
            print("NO CHANGE to Default.json (button wiring + label already match).")
        if not args.no_thumbnails:
            print("WOULD RUN: inject_default_scene_thumbnails.py")
        else:
            print("(Skipping thumbnail inject: --no-thumbnails)")
        print(f"WOULD COPY:  {default_path} -> {main_menu_path}")
        return 0

    if out != raw:
        default_path.write_text(out, encoding="utf-8", newline="\n")
        print(f"WROTE: {default_path}")
    else:
        print(f"NO CHANGE: {default_path} (button fields)")

    if not args.no_thumbnails:
        inj = _TOOLS_DIR / "inject_default_scene_thumbnails.py"
        r = subprocess.run(
            [sys.executable, str(inj)],
            cwd=str(va_root),
        )
        if r.returncode != 0:
            print(
                "WARNING: inject_default_scene_thumbnails.py exited with",
                r.returncode,
                file=sys.stderr,
            )
        else:
            print(f"RAN: {inj.name}")

    shutil.copyfile(default_path, main_menu_path)
    print(f"COPIED: {default_path} -> {main_menu_path}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
