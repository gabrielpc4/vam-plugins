#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Extract toy atoms from a VaM scene JSON into a slim catalog HandSpawnToy
can load offline.

VaM-relative path in plugin UI
--------------------------------
Set HandSpawnToy "Toy catalog scene path" to the generated file path
(relative to the VaM install folder), same as pointing at any full *.json
scene. The plugin only reads the top-level ``atoms`` array; other scene keys
(root camera, presets, etc.) can be omitted.

How HandSpawnToy uses it at runtime (geespot.DildoOnHands.cs)
-------------------------------------------------------------
SuperController.singleton.ReadFileIntoString(relativePath) loads the blob.
TryRebuildToyCatalog parses JSON once per *catalog path* per VaM session and
stores templates in RAM; spawning reuses those strings — the full scene file
is not re-read every trigger unless you change paths or reload the session.

Output format for this script
-------------------------------
Writes ``{"atoms": [ ...matching atom objects... ]}``: each object is copied
whole from the original scene ``atoms[]`` entry so storables/colors/materials,
etc. survive.

Default types match VarietyToyAtomTypes in DildoOnHands.cs plus optional extra
atom type names (--extra-types or --extras-file).

Usage (run from VaM folder or pass absolute paths)::

    python Custom/Scripts/DildoOnHands/extract_toy_catalog.py ^
        --scene "Saves/scene/Mofme/Mofme CamGirlToys/1100_camgirltoys.json" ^
        --out "Custom/Scripts/DildoOnHands/handspawn_toy_atoms.json"

Python 3.6+ compatible (stdlib ``json`` only).
"""

import argparse
import json
import sys


DEFAULT_TYPES = frozenset(("Dildo", "ToyAH", "ToyBP", "Paddle"))


def _read_scene(path):
    # utf-8-sig strips BOM if VaM emitted one.
    with open(path, "r", encoding="utf-8-sig") as fh:
        return json.load(fh)


def _load_extras(lines_path):
    types = []
    try:
        with open(lines_path, "r", encoding="utf-8") as fh:
            for line in fh:
                t = line.strip()
                if t and not t.startswith("#"):
                    types.append(t)
    except OSError as ex:
        print("extras file: {}".format(ex), file=sys.stderr)
        sys.exit(1)

    return types


def main():
    p = argparse.ArgumentParser(
        description="Extract VaM toy atoms into a slim HandSpawnToy catalog JSON.")

    p.add_argument(
        "--scene",
        "-s",
        required=True,
        help='Source VaM scene JSON (e.g. Saves/scene/.../something.json)',
    )

    p.add_argument(
        "--out",
        "-o",
        required=True,
        help="Output JSON path ({atoms: [...] })",
    )

    p.add_argument(
        "--compact",
        action="store_true",
        help="Minified JSON (smaller file, harder to diff)",
    )

    p.add_argument(
        "--extra-types",
        default="",
        help="Comma-separated extra atom types (e.g. Vibrator,Whip)",
    )

    p.add_argument(
        "--extras-file",
        default="",
        help="One VaM atom type name per line (plugin whitelist style)",
    )

    args = p.parse_args()

    whitelist = set(DEFAULT_TYPES)
    chunk = args.extra_types.strip()

    if chunk:
        for part in chunk.split(","):
            piece = part.strip()
            if piece:
                whitelist.add(piece)

    if args.extras_file:
        for t in _load_extras(args.extras_file):
            whitelist.add(t)

    try:
        scene = _read_scene(args.scene)
    except (ValueError, OSError, UnicodeDecodeError) as ex:
        print("{}: {}".format(args.scene, ex), file=sys.stderr)
        sys.exit(1)

    atoms_in = scene.get("atoms")

    if not isinstance(atoms_in, list):
        print(
            '"atoms" missing or not a list in {}'.format(args.scene),
            file=sys.stderr,
        )
        sys.exit(1)

    picked = []

    for ent in atoms_in:
        if not isinstance(ent, dict):
            continue

        atype = ent.get("type")
        uid = ent.get("id")

        if atype not in whitelist:
            continue

        if not isinstance(uid, str) or len(uid.strip()) == 0:
            continue

        picked.append(ent)

    if not picked:
        print(
            'No atoms matched types {}.'.format(sorted(whitelist)),
            file=sys.stderr,
        )
        sys.exit(1)

    slim = {"atoms": picked}
    indent = None if args.compact else 2

    try:
        with open(args.out, "w", encoding="utf-8", newline="\n") as fh:
            json.dump(slim, fh, indent=indent, ensure_ascii=False)
            fh.write("\n")
    except OSError as ex:
        print("{}".format(ex), file=sys.stderr)
        sys.exit(1)

    print(
        'Wrote {} ({} toy atom(s)).'.format(args.out, len(picked)))

if __name__ == '__main__':
    main()
