#!/usr/bin/env python3
"""
Double ``Canvas`` ``xSize`` width on UIButton atoms that carry player ``Text``.
Targets sapuzex Perrine/Pablo menus (atoms with storables ``Text`` + ``Canvas``).

Leaves ``ySize`` untouched. Use ``--factor`` other than ``2`` to scale differently.
Writes VaM-style JSON (``\", \" \" : \"\"`` separators) and re-validates with
``json.loads`` before overwriting.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def dump_scene(data: dict) -> str:
    body = json.dumps(
        data, ensure_ascii=False, separators=(", ", " : "), indent=3
    )
    if not body.rstrip().endswith("}"):
        raise SystemExit("Serialized scene JSON malformed (no closing `}`).")
    return body + "\n"


def atom_has_choice_text(atom: dict) -> bool:
    if str(atom.get("type")) != "UIButton":
        return False
    for st in atom.get("storables") or []:
        if isinstance(st, dict) and str(st.get("id")) == "Text":
            if isinstance(st.get("text"), str):
                return True
    return False


def canvas_xsize_scaled(storable: dict, factor: float) -> bool:
    if str(storable.get("id")) != "Canvas":
        return False
    if "xSize" not in storable:
        return False
    raw = storable["xSize"]
    if not isinstance(raw, str):
        return False
    try:
        v = float(raw)
    except ValueError:
        return False
    new_v = v * factor
    storable["xSize"] = fmt_glike(new_v)
    return True


def fmt_glike(x: float) -> str:
    # Match VaM string numbers: strip trailing zeros, keep useful precision.
    s = ("%.12f" % x).rstrip("0").rstrip(".")
    return s if s else "0"


def process_scene(data: dict, factor: float) -> int:
    changed = 0
    atoms = data.get("atoms") or []
    for atom in atoms:
        if not isinstance(atom, dict):
            continue
        if not atom_has_choice_text(atom):
            continue
        for st in atom.get("storables") or []:
            if not isinstance(st, dict):
                continue
            if canvas_xsize_scaled(st, factor):
                changed += 1
    return changed


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument(
        "targets",
        nargs="*",
        type=Path,
        help=(
            "Scene JSON paths (VaM Saves). Default: sapuzex Perrine and Pablo folder "
            "plus story hub JSON."
        ),
    )
    p.add_argument(
        "--factor",
        type=float,
        default=2.0,
        help="Multiply ``Canvas.xSize`` by this (default ``2`` = double width).",
    )
    p.add_argument(
        "--dry-run",
        action="store_true",
        help="Report counts only; do not write.",
    )
    return p.parse_args(argv)


def default_sapuzex_targets(repo: Path) -> list[Path]:
    folder = repo / "Saves/scene/sapuzex/Perrine and Pablo"
    story = repo / "Saves/scene/sapuzex/Perrine and Pablo story.json"
    out: list[Path] = []
    if folder.is_dir():
        out.extend(sorted(folder.glob("*.json")))
    if story.is_file():
        out.append(story)
    return out


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    repo = Path(__file__).resolve().parents[1]
    if args.targets:
        paths = [(repo / p).resolve() if not p.is_absolute() else p for p in args.targets]
    else:
        paths = default_sapuzex_targets(repo)

    factor = args.factor
    if factor <= 0:
        raise SystemExit("--factor must be positive.")

    total = 0
    touched = 0
    for path in paths:
        if not path.is_file():
            print(f"Skip (missing): {path}", file=sys.stderr)
            continue
        raw = path.read_text(encoding="utf-8")
        data = json.loads(raw)
        if not isinstance(data, dict):
            raise SystemExit("%s root must be an object." % path)
        n = process_scene(data, factor)
        total += n
        if n == 0:
            print("%s - 0 Canvas xSize storables scaled" % path.name)
            continue
        if args.dry_run:
            msg = "%s - would scale %d Canvas.xSize (factor %s)"
            print(msg % (path.name, n, factor))
            touched += 1
            continue
        out = dump_scene(data)
        json.loads(out)
        path.write_text(out, encoding="utf-8", newline="\n")
        msg_ok = "%s - scaled %d Canvas.xSize (factor %s); JSON OK"
        print(msg_ok % (path.name, n, factor))
        touched += 1

    print("Done: %d Canvas widths changed in %d file(s)." % (total, touched))
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
