#!/usr/bin/env python3
"""
Remove Expression Randomizer plugin entries and their plugin storables from every Person in a VaM scene JSON.

Matches:
  - PluginManager.plugins values whose path string contains "ExpressionRandomizer" (case-insensitive)
  - Any storable whose "id" string contains "ExpressionRandomizer" (case-insensitive)

Examples:
  python strip_scene_expression_randomizer.py "Saves/scene/My/Scene.json"
  python strip_scene_expression_randomizer.py scene.json --dry-run
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


def is_expr_rand_plugin_path(value) -> bool:
    if not isinstance(value, str):
        return False
    return "expressionrandomizer" in value.lower()


def is_expr_rand_storable_id(storable_id) -> bool:
    if not isinstance(storable_id, str):
        return False
    return "expressionrandomizer" in storable_id.lower()


def process_person(person: dict, apply_changes: bool) -> tuple[int, int]:
    """
    Returns (plugins_removed, storables_removed).
    Mutates Person only when apply_changes is True.
    """
    pr = 0
    sr = 0
    plugin_manager = None
    for st in person.get("storables", []):
        if st.get("id") == "PluginManager":
            plugin_manager = st
            break

    plugins = plugin_manager.get("plugins") if plugin_manager else None
    if isinstance(plugins, dict):
        keys_to_remove = [
            k
            for k, v in plugins.items()
            if is_expr_rand_plugin_path(v)
        ]
        pr = len(keys_to_remove)
        if apply_changes:
            for k in keys_to_remove:
                del plugins[k]

    for st in person.get("storables", []):
        sid = st.get("id")
        if is_expr_rand_storable_id(sid):
            sr += 1

    if sr and apply_changes:
        person["storables"] = [
            st
            for st in person.get("storables", [])
            if not is_expr_rand_storable_id(st.get("id"))
        ]

    return pr, sr


def main(argv: list[str]) -> int:
    p = argparse.ArgumentParser(
        description="Strip Expression Randomizer plugins and linked plugin storables from Person atoms.",
    )
    p.add_argument(
        "scene_json",
        type=Path,
        help="Path to scene .json (relative to cwd or absolute).",
    )
    p.add_argument(
        "--dry-run",
        action="store_true",
        help="Print counts only; do not write the file.",
    )
    args = p.parse_args(argv)

    path = args.scene_json
    if not path.is_file():
        p.error("Not a file: %s" % path)

    data = load_scene(path)
    atoms = data.get("atoms")
    if not isinstance(atoms, list):
        p.error("Invalid scene: missing atoms array")

    apply_changes = not args.dry_run

    persons = 0
    plugins_total = 0
    storables_total = 0
    for atom in atoms:
        if atom.get("type") != "Person":
            continue
        persons += 1
        pr, sr = process_person(atom, apply_changes)
        plugins_total += pr
        storables_total += sr

    label = "would remove" if args.dry_run else "removed"
    print(
        'Persons: %d  plugin slots %s: %d  storables %s: %d  file: "%s"'
        % (
            persons,
            label,
            plugins_total,
            label,
            storables_total,
            path,
        )
    )

    if args.dry_run:
        print("Dry-run: no write.")
        return 0

    path.write_text(dump_scene(data), encoding="utf-8")
    print("Wrote scene JSON.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
