#!/usr/bin/env python3
"""
Populate free hub UIButton slots with SapuzEx scenes missing from Default/MainMenu.

SapuzEx packs are usually **multi-scene**: the hub should load the **first**
scene of each story, not a mid-pack beat. Common layout:

- ``sapuzex/<Pack> intro.json`` at the **root** chains into
  ``sapuzex/<Pack>/...`` (not every pack has this).
- Otherwise use ``<Pack>/<Pack>.json``, ``<Pack>/intro.json``,
  ``<Pack>/Menu.json``, or a root ``<Pack> story.json``-style entry.
- Packs with **no** clear entry (only deep sub-folders) are skipped.

Also drops ``**/subscenes/**``, ``math lesson/scenes/**``, ``**/demonic
summoning/subscene/**``, ``cooking lesson/beach memories/**``, and ``Copia``
clones.

One candidate per **pack** (pack key normalizes root ``* intro`` / ``* story``
names to the base pack id so the hub does not list two starts for the same
line e.g. Perrine story vs Perrine folder).

Writes ``Default.json`` once with all rewires + pt-BR labels (same helpers as
``rewire_hub_scene_button.py``), runs ``inject_default_scene_thumbnails.py``
unless ``--no-thumbnails``, then copies Default → MainMenu.

See ``rewire_hub_scene_button.py``.
"""

from __future__ import annotations

import argparse
import re
import shutil
import subprocess
import sys
from pathlib import Path

_TOOLS_DIR = Path(__file__).resolve().parent
if str(_TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(_TOOLS_DIR))

import rewire_hub_scene_button as rew  # noqa: E402 pylint: disable=C0415
from hub_scene_labels_pt_br import build_scene_hub_label_pt_br  # noqa: E402 pylint: disable=C0415


# Prefer these first; then RightTopB1-B8 / RightBottomB1-B8 for overflow.
_SLOT_ORDER_DEFAULT: list[str] = [
    "FrontTopB6",
    "FrontTopB7",
    "FrontTopB8",
    "FrontBottomB6",
    "FrontBottomB7",
    "FrontBottomB8",
] + ["RightTopB%s" % i for i in range(1, 9)] + ["RightBottomB%s" % i for i in range(1, 9)]


def _gather_existing_sapuzex_paths(hub_scene_text: str) -> set[str]:
    keys: set[str] = set()
    for m in re.finditer(r'"sceneFilePath"\s*:\s*"([^"]+)"', hub_scene_text):
        p = m.group(1).replace("\\", "/")
        low = p.lower()
        if "sapuzex" not in low:
            continue
        norm = p[2:] if p.startswith("./") else p
        keys.add(norm.replace("\\", "/").lower())
    return keys


def _excluded_posix(rl: str) -> bool:
    if "/subscenes/" in rl:
        return True
    if "/math lesson/scenes/" in rl:
        return True
    if " - copia." in rl:
        return True
    if "/demonic summoning/subscene/" in rl:
        return True
    if "/cooking lesson/beach memories/" in rl:
        return True
    return False


def pack_key_for_rel(rel_posix: str) -> str:
    """
    One logical pack id per hub path so root ``Tribal ritual intro.json`` maps
    to the same pack as ``tribal ritual/...`` and ``Perrine … story.json`` maps
    to ``perrine and pablo``.
    """
    norm = rel_posix.replace("\\", "/").strip("/").lower()
    parts = [p for p in norm.split("/") if p]
    if len(parts) < 2 or parts[0] != "sapuzex":
        return norm
    fname = parts[-1]
    stem = Path(fname).stem.lower()
    if len(parts) == 2:
        if stem.endswith(" intro"):
            return stem[: -len(" intro")].strip()
        if stem.endswith(" story"):
            return stem[: -len(" story")].strip()
        return stem
    return parts[1]


def _shallow_entry_sort_key(p: Path) -> tuple:
    """Prefer intro/menu, then lowest explicit scene/part number, else name."""
    s = p.stem.lower()
    if "intro" in s:
        return (0, 0, s)
    if s == "menu":
        return (1, 0, s)
    m = re.search(r"\bmess\s+up\s+(\d+)\s*$", s)
    if m:
        return (2, int(m.group(1)), s)
    m2 = re.search(r"\b(scene|part|step)\s+(\d+)\s*$", s)
    if m2:
        return (3, int(m2.group(2)), s)
    mt = re.search(r"(\d+)\s*$", s)
    if mt:
        return (4, int(mt.group(1)), s)
    return (90, 0, s)


def pick_first_scene_for_folder(
    sap: Path,
    folder_name: str,
    scene_root: Path,
) -> Path | None:
    """
    First hub-eligible scene for pack folder ``folder_name`` (SapuzEx root
    ``sap``), following typical SapuzEx entry order.
    """
    # 1) Root ``<Pack> intro.json`` (often starts the multi-scene flow).
    # 2) Root ``<Pack>.json`` (single entry at root for that pack name).
    # 3) ``<Pack>/intro.json``
    # 4) ``<Pack>/<Pack>.json``
    # 5) ``<Pack>/Menu.json`` / ``menu.json``
    # 6) Root ``<Pack> story.json`` (e.g. Perrine line).
    # 7) Root ``*.json`` whose stem starts with ``<Pack> `` (prefix story file).
    # 8) Shallow ``<Pack>/*.json`` only (no subfolders), sorted.
    d = folder_name
    candidates: list[Path] = [
        sap / ("%s intro.json" % d),
        sap / ("%s.json" % d),
        sap / d / "intro.json",
        sap / d / ("%s.json" % d),
        sap / d / "Menu.json",
        sap / d / "menu.json",
        sap / ("%s story.json" % d),
    ]
    for p in candidates:
        if not p.is_file():
            continue
        rel = p.relative_to(scene_root).as_posix()
        if _excluded_posix(rel.lower()):
            continue
        return p

    fld_l = d.lower()
    for p in sorted(sap.glob("*.json"), key=lambda x: x.name.lower()):
        stem_l = p.stem.lower()
        if stem_l.startswith(fld_l + " "):
            rel = p.relative_to(scene_root).as_posix()
            if not _excluded_posix(rel.lower()):
                return p

    pack_dir = sap / d
    if not pack_dir.is_dir():
        return None
    shallow = sorted(
        [q for q in pack_dir.glob("*.json") if q.is_file()],
        key=_shallow_entry_sort_key,
    )
    for p in shallow:
        rel = p.relative_to(scene_root).as_posix()
        if not _excluded_posix(rel.lower()):
            return p
    return None


def collect_pack_first_candidates(scene_root: Path, existing: set[str]) -> list[str]:
    """One first-scene path per SapuzEx pack, excluding packs already on hub."""
    sap = scene_root / "sapuzex"
    if not sap.is_dir():
        return []

    claimed: set[str] = set()
    for ek in existing:
        claimed.add(pack_key_for_rel(ek))

    selected: list[tuple[str, str]] = []
    chosen_lower: set[str] = set()

    dirs = sorted(
        [p for p in sap.iterdir() if p.is_dir() and not p.name.startswith(".")],
        key=lambda p: p.name.lower(),
    )
    for ddir in dirs:
        pk = ddir.name.lower()
        if pk in claimed:
            continue
        entry = pick_first_scene_for_folder(sap, ddir.name, scene_root)
        if entry is None:
            continue
        rel = entry.relative_to(scene_root).as_posix()
        rl = rel.lower()
        if rl in existing or _excluded_posix(rl) or rl in chosen_lower:
            continue
        key = pack_key_for_rel(rel)
        if key in claimed:
            continue
        claimed.add(key)
        chosen_lower.add(rl)
        selected.append((key, rel))

    for f in sorted(sap.glob("*.json"), key=lambda p: p.name.lower()):
        rl = f.relative_to(scene_root).as_posix().lower()
        if rl in existing or _excluded_posix(rl) or rl in chosen_lower:
            continue
        stem = f.stem
        stem_l = stem.lower()
        if stem_l.endswith(" intro"):
            base = stem[: -len(" intro")].strip()
            if (sap / base).is_dir():
                continue
        if (sap / stem).is_dir():
            continue
        key = pack_key_for_rel(f.relative_to(scene_root).as_posix())
        if key in claimed:
            continue
        rel = f.relative_to(scene_root).as_posix()
        claimed.add(key)
        chosen_lower.add(rl)
        selected.append((key, rel))

    selected.sort(key=lambda t: t[0])
    return [rel for _, rel in selected]


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--va-root",
        type=Path,
        default=None,
        help="VaM root (folder with Saves/scene). Default: walk up from script.",
    )
    ap.add_argument("--dry-run", action="store_true", help="Print plan only.")
    ap.add_argument(
        "--no-thumbnails",
        action="store_true",
        help="Skip inject_default_scene_thumbnails.py.",
    )
    ap.add_argument(
        "--slots",
        default="",
        help="Comma-separated button ids overriding default Front + Right slots.",
    )
    args = ap.parse_args()

    va_root = args.va_root or rew.find_va_root(Path(__file__).resolve())
    if va_root is None:
        print("Could not locate VaM root.", file=sys.stderr)
        return 1
    scene_root = va_root / "Saves" / "scene"

    slots = (
        [s.strip() for s in args.slots.split(",") if s.strip()]
        if args.slots.strip()
        else list(_SLOT_ORDER_DEFAULT)
    )

    default_path = scene_root / "Default.json"
    main_menu_path = scene_root / "MainMenu.json"
    if not default_path.is_file():
        raise SystemExit("Missing %s" % default_path)

    raw = default_path.read_text(encoding="utf-8")
    existing = _gather_existing_sapuzex_paths(raw)
    cand = collect_pack_first_candidates(scene_root, existing)

    plan: list[tuple[str, str]] = []
    n = min(len(slots), len(cand))
    for i in range(n):
        plan.append((slots[i], cand[i]))

    print("VaM root:", va_root)
    print("SapuzEx paths already wired (normalized): %d" % len(existing))
    print("SapuzEx pack-first candidates (not on hub):", len(cand))
    print("Fill count (min slots / candidates):", n)

    rows: list[tuple[str, str, str, str, str]] = []
    for bid, scene_rel in plan:
        hub_rel_json, abs_scene = rew.resolve_hub_scene(
            scene_root, None, scene_rel.strip(), None, ""
        )
        hub_rel, title_src, author_json_src = rew.hub_and_label_paths(
            scene_root, abs_scene, hub_rel_json
        )
        rel_for_author = title_src.relative_to(scene_root)
        scene_title = title_src.stem
        au = rew.resolve_label_author(author_json_src, rel_for_author)
        author_s = au.strip() if au and au.strip() else ""

        old_txt = rew.extract_uibutton_text_from_raw_hub(raw, bid)
        label = build_scene_hub_label_pt_br(
            scene_title,
            bid,
            hub_rel,
            old_txt,
            ignore_frozen_prior=True,
        )
        rows.append((bid, scene_rel, hub_rel, label, author_s))

    print("\nAssignments:")
    for bid, sr, hh, lbl, author_s in rows:
        print("%s -> %s" % (bid, sr))
        print("       hubPath: %s" % hh)
        if author_s:
            print("       author: %s" % author_s)

    print("\nButton labels:")
    for bid, sr, hh, lbl, author_s in rows:
        print("--- %s (%s)" % (bid, sr))
        print(lbl)

    if args.dry_run:
        print("\nDry-run; skipped write, thumbnails, MainMenu copy.")
        return 0

    try:
        for bid, sr, hh, lbl, author_s in rows:
            raw = rew.replace_scene_after_button(raw, bid, hh)
            raw = rew.replace_text_storable_for_button(raw, bid, lbl)
    except ValueError as e:
        raise SystemExit("%s %s" % (default_path.name, e))

    default_path.write_text(raw, encoding="utf-8", newline="\n")
    print("\nWROTE:", default_path)

    if not args.no_thumbnails:
        inj = _TOOLS_DIR / "inject_default_scene_thumbnails.py"
        r = subprocess.run([sys.executable, str(inj)], cwd=str(va_root))
        if r.returncode != 0:
            print(
                "WARNING: inject_default_scene_thumbnails.py exited with",
                r.returncode,
                file=sys.stderr,
            )
        else:
            print("RAN:", inj.name)
    shutil.copyfile(default_path, main_menu_path)
    print("COPIED:", default_path, "->", main_menu_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
