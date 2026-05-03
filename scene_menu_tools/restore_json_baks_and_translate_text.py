#!/usr/bin/env python3
"""
Copy ``scene.json.bak`` over ``scene.json`` (restore English/state at backup time),
then run ``translate_vam_scene_player_text_pt_br.py`` with a map file.

Deletes only the overwritten ``.json`` content; preserves ``.bak`` on disk.
"""

from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
from pathlib import Path

_REPO = Path(__file__).resolve().parents[1]
_TRANSLATOR = Path(__file__).resolve().parent / "translate_vam_scene_player_text_pt_br.py"


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument(
        "directory",
        type=Path,
        help="VaM folder with ``*.json.bak`` next to scene ``*.json``",
    )
    p.add_argument(
        "--map-file",
        type=Path,
        required=True,
        help="JSON EN->PT map for ``translate_*``.",
    )
    p.add_argument(
        "--story-json",
        type=Path,
        default=None,
        help="Optional scene path; restores from ``same path + .bak`` first.",
    )
    return p.parse_args(argv)


def restore_sidecar_baks(folder: Path) -> int:
    """Overwrite ``*.json`` from sibling ``*.json.bak``. Returns count."""
    n = 0
    for bak in sorted(folder.glob("*.json.bak")):
        name = bak.name
        if not name.endswith(".json.bak"):
            continue
        dest = bak.parent / name[: -len(".bak")]
        shutil.copy2(bak, dest)
        n += 1
        print(f"Restored {dest.relative_to(_REPO)} from .bak", flush=True)
    return n


def restore_story(sidecar_story: Path) -> bool:
    """``story.json.bak`` -> ``story.json``."""
    bak = sidecar_story.with_suffix(sidecar_story.suffix + ".bak")
    if not bak.is_file():
        return False
    shutil.copy2(bak, sidecar_story)
    print(f"Restored {sidecar_story.relative_to(_REPO)} from .bak", flush=True)
    return True


def translate(scene: Path, map_path: Path) -> None:
    r = subprocess.run(
        [
            sys.executable,
            str(_TRANSLATOR),
            str(scene.resolve()),
            "--map-file",
            str(map_path.resolve()),
        ],
        cwd=str(_REPO),
        capture_output=False,
    )
    if r.returncode not in (0, 1):
        raise SystemExit(f"translator exit {r.returncode} for {scene}")


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    folder = (args.directory if args.directory.is_absolute() else (_REPO / args.directory)).resolve()
    mf = (
        args.map_file
        if args.map_file.is_absolute()
        else (_REPO / args.map_file)
    ).resolve()
    if not folder.is_dir():
        raise SystemExit(f"Not a dir: {folder}")
    if not mf.is_file():
        raise SystemExit(f"Missing map: {mf}")

    restore_sidecar_baks(folder)

    story = args.story_json
    if story is not None:
        sp = (story if story.is_absolute() else (_REPO / story)).resolve()
        restore_story(sp)
        translate(sp, mf)

    for jp in sorted(folder.glob("*.json")):
        translate(jp, mf)

    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
