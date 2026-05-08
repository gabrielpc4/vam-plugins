#!/usr/bin/env python3
"""
Rewrite **BrowserGUI** ``url`` fields across every VaM scene ``*.json`` in a folder.

VaM saves in-scene web panels under storables like::

    { "id": "BrowserGUI", "url": "https://..." }

This tool walks each scene JSON (full parse), finds those objects in **depth-first**
order, and assigns your URL list with **cycling** (``urls[i % len(urls)]``).

**Backup:** only the **original** file is preserved: the first time you edit a scene,
``scene.json`` is copied to ``scene.json.bak``. If ``scene.json.bak`` already exists,
it is **not** overwritten and no other backup files are created.

After each write, the file is **re-read and parsed** to verify valid JSON.
"""

from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path
from typing import Any, Iterable


_REPO = Path(__file__).resolve().parents[1]


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "scene_folder",
        type=Path,
        help="Directory containing VaM scene *.json files (non-recursive by default).",
    )
    parser.add_argument(
        "urls",
        nargs="+",
        help="One or more URLs assigned to BrowserGUI panels in order (cycled if needed).",
    )
    parser.add_argument(
        "--recursive",
        action="store_true",
        help="Include *.json in subfolders.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print planned changes; do not write files.",
    )
    parser.add_argument(
        "--only-if-url-starts-with",
        metavar="PREFIX",
        default=None,
        help=(
            "Only change BrowserGUI entries whose current url value starts with PREFIX "
            "(e.g. http). Empty/missing url is treated as unchanged unless PREFIX is \"\"."
        ),
    )
    return parser.parse_args(argv)


def iter_browser_gui_nodes(root: Any) -> Iterable[dict[str, Any]]:
    """Depth-first yield of dict nodes that look like BrowserGUI storables."""

    if isinstance(root, dict):
        if root.get("id") == "BrowserGUI":
            yield root

        for child_value in root.values():
            yield from iter_browser_gui_nodes(child_value)

    elif isinstance(root, list):
        for item in root:
            yield from iter_browser_gui_nodes(item)


def should_touch_url(current: Any, prefix: str | None) -> bool:
    if prefix is None:
        return True

    if current is None:
        current_text = ""
    else:
        current_text = str(current)

    return current_text.startswith(prefix)


def assign_browser_urls(data: Any, urls: list[str], url_prefix_filter: str | None) -> int:
    """Mutates ``data``. Returns count of BrowserGUI url fields updated."""

    nodes = list(iter_browser_gui_nodes(data))
    if not nodes:
        return 0

    url_count = len(urls)
    changed = 0

    for index, node in enumerate(nodes):
        if "url" not in node:
            current = ""
        else:
            current = node.get("url")

        if not should_touch_url(current, url_prefix_filter):
            continue

        new_url = urls[index % url_count]
        node["url"] = new_url
        changed += 1

    return changed


def ensure_original_backup_once(scene_path: Path, dry_run: bool) -> Path | None:
    """
    Copy ``scene.json`` → ``scene.json.bak`` only when ``.bak`` is missing.
    Returns the backup path if a new copy was or would be made; otherwise None.
    """

    sidecar_bak = scene_path.with_suffix(scene_path.suffix + ".bak")

    if sidecar_bak.is_file():
        return None

    if not dry_run:
        shutil.copy2(scene_path, sidecar_bak)

    return sidecar_bak


def load_scene_json(scene_path: Path) -> Any:
    text = scene_path.read_text(encoding="utf-8")
    return json.loads(text)


def validate_json_file(scene_path: Path) -> None:
    load_scene_json(scene_path)


def collect_json_files(folder: Path, recursive: bool) -> list[Path]:
    if not folder.is_dir():
        raise SystemExit(f"Not a directory: {folder}")

    if recursive:
        paths = sorted(folder.rglob("*.json"))
    else:
        paths = sorted(folder.glob("*.json"))

    filtered: list[Path] = []
    for path in paths:
        name = path.name
        if name.endswith(".bak"):
            continue

        filtered.append(path)

    return filtered


def relative_repo(path: Path) -> str:
    try:
        return str(path.resolve().relative_to(_REPO))
    except ValueError:
        return str(path.resolve())


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    folder = args.scene_folder.resolve()
    urls = args.urls

    prefix_filter: str | None
    if args.only_if_url_starts_with is None:
        prefix_filter = None
    else:
        prefix_filter = args.only_if_url_starts_with

    scene_paths = collect_json_files(folder, args.recursive)

    if not scene_paths:
        print(f"No *.json files under {relative_repo(folder)}", file=sys.stderr)
        return 1

    total_files_touched = 0
    total_panels_changed = 0

    for scene_path in scene_paths:
        try:
            data = load_scene_json(scene_path)
        except json.JSONDecodeError as error:
            print(
                f"SKIP invalid JSON {relative_repo(scene_path)}: {error}",
                file=sys.stderr,
            )
            continue

        snapshot = json.dumps(data, sort_keys=True)
        changed = assign_browser_urls(data, urls, prefix_filter)

        if changed == 0:
            continue

        after_snapshot = json.dumps(data, sort_keys=True)
        if after_snapshot == snapshot:
            continue

        backup_path = ensure_original_backup_once(scene_path, args.dry_run)

        if args.dry_run:
            print(
                f"[dry-run] would update {changed} BrowserGUI url(s) in "
                f"{relative_repo(scene_path)}",
                flush=True,
            )

            if backup_path is not None:
                print(f"          would create original backup -> {relative_repo(backup_path)}", flush=True)

            total_files_touched += 1
            total_panels_changed += changed
            continue

        dumped = json.dumps(data, indent=3, ensure_ascii=False) + "\n"
        scene_path.write_text(dumped, encoding="utf-8")

        try:
            validate_json_file(scene_path)
        except json.JSONDecodeError as error:
            print(
                f"ERROR wrote invalid JSON {relative_repo(scene_path)}: {error}",
                file=sys.stderr,
            )
            return 2

        if backup_path is not None:
            backup_note = relative_repo(backup_path)
        else:
            backup_note = "unchanged (original already in .bak)"

        print(
            f"OK {relative_repo(scene_path)} — {changed} panel(s); backup: {backup_note}",
            flush=True,
        )
        total_files_touched += 1
        total_panels_changed += changed

    print(
        f"Done. Files modified: {total_files_touched}; BrowserGUI url updates: {total_panels_changed}.",
        flush=True,
    )

    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
