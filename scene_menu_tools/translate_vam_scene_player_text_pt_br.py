#!/usr/bin/env python3
"""
Replace player-visible strings stored in VaM Unity ``Text`` storables (storables[].id ==
``Text``, field ``text``) inside a scene JSON without truncating structure.

Keeps VaM-ish JSON spacing ``"key" : val`` via the same serializers as strip tools.

Flows:
  * ``--map-file`` JSON object: exact english string -> pt-BR (recommended).
  * ``--dry-run``: print atom id + old text; no write.
  * ``--list-texts``: only list every ``Text`` storable ``text`` (deduped paths).

Avoid translating paths, hashes, GUIDs or technical ids: only storable ids listed in
TEXT_STORABLE_IDS and only the explicit ``text`` field on those storables.

Validate output by round-tripping through json.loads before writing.
"""

from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path
from typing import Any

TOOLS_DIR = Path(__file__).resolve().parent


def dump_scene(data: dict[str, Any]) -> str:
    out = json.dumps(data, ensure_ascii=False, separators=(", ", " : "), indent=3)
    if not out.rstrip().endswith("}"):
        raise SystemExit("Refusing to write: serialized JSON does not end with `}`")
    return out + "\n"


def validate_roundtrip(serialized: str) -> dict[str, Any]:
    parsed = json.loads(serialized)
    assert isinstance(parsed, dict)
    return parsed


TEXT_STORABLE_IDS = frozenset({"Text"})


def iter_text_storable_texts(scene: dict[str, Any]) -> list[tuple[str, str]]:
    """Yield (atom_id, text_value) for every Text storable carrying ``text``."""
    out: list[tuple[str, str]] = []
    for atom in scene.get("atoms") or []:
        if not isinstance(atom, dict):
            continue
        aid = str(atom.get("id", ""))
        for st in atom.get("storables") or []:
            if not isinstance(st, dict):
                continue
            if str(st.get("id")) not in TEXT_STORABLE_IDS:
                continue
            if "text" not in st:
                continue
            t = st["text"]
            if not isinstance(t, str):
                continue
            out.append((aid, t))
    return out


def apply_map_to_text_storables(
    scene: dict[str, Any], mapping: dict[str, str]
) -> int:
    """
    Replace ``text`` when the full string is a key in ``mapping``.
    Returns number of storables updated.
    """
    changed = 0
    for atom in scene.get("atoms") or []:
        if not isinstance(atom, dict):
            continue
        for st in atom.get("storables") or []:
            if not isinstance(st, dict):
                continue
            if str(st.get("id")) not in TEXT_STORABLE_IDS:
                continue
            if "text" not in st:
                continue
            old = st["text"]
            if not isinstance(old, str):
                continue
            new = mapping.get(old)
            if new is None or new == old:
                continue
            st["text"] = new
            changed += 1
    return changed


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("scene_json", type=Path, help="VaM scene JSON path")
    p.add_argument(
        "--map-file",
        type=Path,
        default=None,
        help="UTF-8 JSON object: exact source string -> pt-BR translation",
    )
    p.add_argument("--dry-run", action="store_true", help="No file write")
    p.add_argument(
        "--list-texts",
        action="store_true",
        help="Print atom id + each Text ``text``, then exit (no write)",
    )
    p.add_argument(
        "--backup",
        action="store_true",
        help="Copy input to same path + ``.bak`` before overwrite",
    )
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    path = args.scene_json.resolve()
    if not path.is_file():
        print(f"Not a file: {path}", file=sys.stderr)
        return 2

    raw = path.read_text(encoding="utf-8")
    scene = json.loads(raw)
    if not isinstance(scene, dict):
        raise SystemExit("Root JSON must be an object")

    if args.list_texts:
        for aid, txt in iter_text_storable_texts(scene):
            print(f"[{aid}]\t{repr(txt)}")
        return 0

    mapping: dict[str, str]
    if args.map_file:
        mp = Path(args.map_file).resolve()
        blob = mp.read_text(encoding="utf-8")
        mapping = json.loads(blob)
        if not isinstance(mapping, dict):
            raise SystemExit("--map-file root must be a JSON object")
        # normalize keys/str values
        mapping = {
            str(k): str(v)
            for k, v in mapping.items()
            if isinstance(k, (str, int, float)) and isinstance(v, (str, int, float))
        }
    else:
        print("--map-file is required unless using --list-texts", file=sys.stderr)
        return 2

    before = iter_text_storable_texts(scene)
    n = apply_map_to_text_storables(scene, mapping)
    after = iter_text_storable_texts(scene)

    if args.dry_run:
        print(f"Would replace {n} Text storable(s).")
        for aid, txt in before:
            if txt in mapping and mapping.get(txt) != txt:
                print(f"  [{aid}] {repr(txt)} -> {repr(mapping[txt])}")
        return 0

    if n == 0:
        print("No matching strings changed; refusing to rewrite file.")
        return 1

    serialized = dump_scene(scene)
    validate_roundtrip(serialized)

    if args.backup:
        bak = path.with_suffix(path.suffix + ".bak")
        shutil.copy2(path, bak)
        print(f"Backup: {bak}")

    path.write_text(serialized, encoding="utf-8", newline="\n")
    print(f"Wrote {path} ({n} Text storables updated)")
    unused = sorted(set(mapping.keys()) - set(t for _, t in before))
    if unused:
        print("Unused map keys:", ", ".join(repr(u) for u in unused), file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
