#!/usr/bin/env python3
"""
Keep hub menu scenes identical: ``Saves/scene/Default.json`` →
``Saves/scene/MainMenu.json`` (same bytes).

VaM VaMScripts workflows treat ``Default.json`` as the edited hub scene, then mirror
to ``MainMenu.json`` so both stay byte-for-byte the same.

Run from VaM root:
  python Custom/Scripts/tools/scene-menu/sync_hub_default_mainmenu.py
  python .../sync_hub_default_mainmenu.py --verify-only
"""

from __future__ import annotations

import argparse
import hashlib
import shutil
import sys
from pathlib import Path


def _find_va_root(start: Path) -> Path | None:
    for anc in [start.resolve(), *start.resolve().parents]:
        if (anc / "Saves" / "scene").is_dir():
            return anc
    return None


def _sha256(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        while True:
            b = f.read(1024 * 1024)
            if not b:
                break
            h.update(b)
    return h.hexdigest()


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--va-root",
        type=Path,
        default=None,
        help="VaM install root (contains Saves/scene). Default: walk from script.",
    )
    ap.add_argument(
        "--verify-only",
        action="store_true",
        help="Exit 0 if files match; 1 if they differ. Does not write.",
    )
    args = ap.parse_args()

    va = args.va_root or _find_va_root(Path(__file__).resolve())
    if va is None:
        print("Could not find VaM root.", file=sys.stderr)
        return 1
    scene = va / "Saves" / "scene"
    default_p = scene / "Default.json"
    main_p = scene / "MainMenu.json"
    if not default_p.is_file():
        print("Missing %s" % default_p, file=sys.stderr)
        return 1

    def _verify() -> int:
        if not main_p.is_file():
            print("Missing %s (not in sync with Default)." % main_p, file=sys.stderr)
            return 1
        d, m = _sha256(default_p), _sha256(main_p)
        if d == m:
            print("OK: Default.json and MainMenu.json match (%s)." % d[:16])
            return 0
        print(
            "MISMATCH: Default.json %s … vs MainMenu.json %s …"
            % (d[:16], m[:16]),
            file=sys.stderr,
        )
        return 1

    if args.verify_only:
        return _verify()

    if main_p.is_file() and _sha256(main_p) == _sha256(default_p):
        print("SKIP: Already identical.")
        return 0

    if not main_p.is_file():
        shutil.copyfile(default_p, main_p)
        print("COPIED (new): %s -> %s" % (default_p, main_p))
        return 0

    shutil.copyfile(default_p, main_p)
    print("COPIED: %s -> %s" % (default_p, main_p))
    if _sha256(main_p) != _sha256(default_p):
        print("COPY VERIFY FAILED.", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
