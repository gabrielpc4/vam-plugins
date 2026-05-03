#!/usr/bin/env python3
"""
Disable or remove geometry clothing slots on every Person atom in a VaM scene JSON.

Examples:
  python strip_scene_person_clothing.py "Saves/scene/My/Scene.json" "Alphakini Bra Sim"
  python strip_scene_person_clothing.py scene.json --cloth "Item A" --cloth "Item B"
  python strip_scene_person_clothing.py scene.json "A" "B" --remove-from-list --dry-run
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

def load_scene(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")
    return json.loads(text)


def dump_scene(data: dict) -> str:
    out = json.dumps(data, ensure_ascii=False, separators=(", ", " : "), indent=3)
    if not out.rstrip().endswith("}"):
        raise SystemExit("Refusing to write: serialized JSON does not end with `}`")
    return out + "\n"


def collect_clothing_names(args: argparse.Namespace) -> list[str]:
    names: list[str] = []
    names.extend(args.clothing_positional)
    if args.cloth:
        names.extend(args.cloth)
    seen = set()
    out: list[str] = []
    for n in names:
        s = n.strip()
        if not s or s in seen:
            continue
        seen.add(s)
        out.append(s)
    return out


def process_person(
    person: dict,
    targets: set[str],
    remove_from_list: bool,
    apply_changes: bool,
) -> tuple[int, int]:
    """
    Returns (items_matched, geometry_blocks_updated).
    If apply_changes is False, counts only without mutating.
    """
    matched = 0
    touched = 0
    for st in person.get("storables", []):
        if st.get("id") != "geometry":
            continue
        clothing = st.get("clothing")
        if not isinstance(clothing, list):
            continue
        if remove_from_list:
            removals = sum(
                1
                for item in clothing
                if isinstance(item, dict)
                and item.get("id") in targets
            )
            matched += removals
            if removals and apply_changes:
                st["clothing"] = [
                    item
                    for item in clothing
                    if not (
                        isinstance(item, dict)
                        and item.get("id") in targets
                    )
                ]
            if removals:
                touched += 1
        else:
            for item in clothing:
                if not isinstance(item, dict):
                    continue
                cid = item.get("id")
                if cid in targets:
                    matched += 1
                    if apply_changes:
                        item["enabled"] = "false"
            if matched > 0:
                touched += 1
        break
    return matched, touched


def main(argv: list[str]) -> int:
    p = argparse.ArgumentParser(
        description="Disable (--default) or remove clothing entries under Person geometry storables.",
    )
    p.add_argument(
        "scene_json",
        type=Path,
        help="Path to scene .json (relative to cwd or absolute).",
    )
    p.add_argument(
        "clothing_positional",
        nargs="*",
        help='Clothing "id" values as printed in VaM geometry (extras: use --cloth).',
    )
    p.add_argument(
        "--cloth",
        action="append",
        default=[],
        metavar="NAME",
        help="Additional clothing id (repeatable).",
    )
    p.add_argument(
        "--remove-from-list",
        action="store_true",
        help="Drop matching clothing dicts entirely instead of setting enabled=false.",
    )
    p.add_argument(
        "--dry-run",
        action="store_true",
        help="Print counts only; do not write the file.",
    )
    args = p.parse_args(argv)

    names = collect_clothing_names(args)
    if not names:
        p.error("Pass at least one clothing name (positional or --cloth).")

    path = args.scene_json
    if not path.is_file():
        p.error("Not a file: %s" % path)

    targets = set(names)
    data = load_scene(path)
    atoms = data.get("atoms")
    if not isinstance(atoms, list):
        p.error("Invalid scene: missing atoms array")

    apply_changes = not args.dry_run

    total_match = 0
    geometry_blocks = 0
    persons = 0
    for atom in atoms:
        if atom.get("type") != "Person":
            continue
        persons += 1
        m, g = process_person(atom, targets, args.remove_from_list, apply_changes)
        total_match += m
        geometry_blocks += g

    label = "matched" if args.dry_run else "updated"
    print(
        'Persons: %d  clothing slots %s: %d  geometry blocks touched: %d  file: "%s"'
        % (persons, label, total_match, geometry_blocks, path)
    )
    if total_match == 0:
        print("No matching clothing ids found (check exact spelling vs geometry id).")

    if args.dry_run:
        print("Dry-run: no write.")
        return 0

    path.write_text(dump_scene(data), encoding="utf-8")
    print("Wrote scene JSON.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
