#!/usr/bin/env python3
"""
Rewrite **BrowserGUI** ``url`` fields across every VaM scene ``*.json`` in a folder.

VaM saves in-scene web panels under storables like::

    { "id": "BrowserGUI", "url": "https://..." }

This tool walks each scene JSON (full parse), finds those objects in **depth-first**
order, and assigns your URL list with **cycling** (``urls[i % len(urls)]``).

Backup policy (per file): copy ``scene.json`` → ``scene.json.bak`` **once**, only when
that ``.bak`` does **not** already exist. Later runs keep editing ``scene.json`` but never
add extra backup files—the original snapshot stays in ``scene.json.bak``.

VaM **does not** substitute Google via scene JSON. If you still see Google, the panel’s
embedded Chromium often never left its **built‑in default page** (many prefabs ship with
Google), or navigation failed.

**RedGIFs:** by default, ``https://media.redgifs.com/<Name>.mp4`` arguments are rewritten to
``https://www.redgifs.com/watch/<slug>`` so the stock site loads in the browser. That page is
**not** edge‑to‑edge video (controls, layout), and HTML **fullscreen** APIs usually **do not**
map to “fill the VaM web panel”. For a **viewport‑filling** clip inside the panel, pass
``--video-fill-viewport`` together with **direct** ``*.mp4`` (or other http video) URLs; the
tool writes tiny HTML files under ``_vam_browser_video_fill/`` and points **BrowserGUI** at
those paths so a ``<video>`` tag can use CSS ``object-fit`` to fill the texture.

If VaM never leaves the default Google page when **BrowserGUI** points at a ``Saves/.../*.html``
path, the install may not load local HTML in the embedded browser. In that case use
``--redgifs-ifr`` to set **https** embed URLs like ``https://www.redgifs.com/ifr/<slug>``
(no local file). Optional ``--redgifs-ifr-wrapper-html`` writes a tiny wrapper page that
iframes the same URL (still a local path—only use if direct ``/ifr/`` works but you want layout).

After each write, the file is **re-read and parsed** to verify valid JSON.
"""

from __future__ import annotations

import argparse
import hashlib
import html
import json
import re
import shutil
import sys
from pathlib import Path
from typing import Any, Iterable
from urllib.parse import urlparse, urlunparse


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
    parser.add_argument(
        "--keep-redgifs-media-mp4",
        action="store_true",
        help=(
            "Do not rewrite RedGIFs CDN mp4 arguments "
            "(``https://media.redgifs.com/<Slug>.mp4``) to ``/watch/<slug>`` pages."
        ),
    )
    parser.add_argument(
        "--video-fill-viewport",
        action="store_true",
        help=(
            "Write minimal HTML pages that play direct http(s) video URLs edge-to-edge in "
            "the browser panel (see module docstring). Implies keeping mp4 URLs (no RedGIFs "
            "watch rewrite)."
        ),
    )
    parser.add_argument(
        "--video-object-fit",
        choices=("cover", "contain"),
        default="cover",
        help="How the <video> fills the panel when using --video-fill-viewport (default: cover).",
    )
    parser.add_argument(
        "--video-unmuted",
        action="store_true",
        help="Do not mute the <video> element (autoplay may be blocked; user can click to play).",
    )
    parser.add_argument(
        "--video-hide-controls",
        action="store_true",
        help="Omit the browser video control bar for a cleaner fullscreen-style look.",
    )
    parser.add_argument(
        "--redgifs-ifr",
        action="store_true",
        help=(
            "Resolve each argument to a RedGIFs slug and set BrowserGUI to "
            "https://www.redgifs.com/ifr/<slug> (embed player over HTTPS). "
            "Accepts /watch/…, /ifr/…, media…mp4, or a bare slug."
        ),
    )
    parser.add_argument(
        "--redgifs-ifr-wrapper-html",
        action="store_true",
        help=(
            "With --redgifs-ifr, write wrapper HTML under _vam_browser_redgifs_iframe/ "
            "instead of using the https /ifr/ URL directly (local path; may fail in some VaM setups)."
        ),
    )
    parser.add_argument(
        "--no-porngifs-canonical",
        action="store_true",
        help=(
            "Do not rewrite porngifs.com / www.porngifs.com http(s) URLs to "
            "https://www.porngifs.com/.../ (trailing slash). "
            "Default canonicalization avoids some VaM embedded-browser load quirks."
        ),
    )
    return parser.parse_args(argv)


def normalize_redgifs_media_mp4_to_watch(url: str) -> str:
    """
    Map ``https://media.redgifs.com/MySlug.mp4`` → ``https://www.redgifs.com/watch/myslug``.
    Other URLs are returned unchanged.
    """

    trimmed = url.strip()
    parsed = urlparse(trimmed)
    hostname = (parsed.hostname or "").lower()

    if hostname != "media.redgifs.com":
        return trimmed

    path = parsed.path or ""
    if not path.lower().endswith(".mp4"):
        return trimmed

    filename = path.rsplit("/", 1)[-1]
    if len(filename) <= 4:
        return trimmed

    slug_lower = filename[:-4].lower()

    return "https://www.redgifs.com/watch/" + slug_lower


def normalize_porngifs_browser_url(url: str) -> str:
    """
    Force https://www.porngifs.com/.../ for porngifs hosts.

    VaM's embedded Chromium sometimes fails initial navigation for the apex host or for
    extensionless numeric paths unless the canonical www host and trailing slash are used;
    manual paste can still work because redirects differ from scripted first load.
    """

    trimmed = url.strip()
    lowered = trimmed.lower()

    if not (lowered.startswith("http://") or lowered.startswith("https://")):
        return trimmed

    parsed = urlparse(trimmed)
    host = (parsed.hostname or "").lower()

    if host != "porngifs.com" and host != "www.porngifs.com":
        return trimmed

    path = parsed.path or ""
    if path and not path.endswith("/"):
        path = path + "/"

    scheme = parsed.scheme if parsed.scheme else "https"
    if scheme != "http" and scheme != "https":
        scheme = "https"

    return urlunparse(
        (scheme, "www.porngifs.com", path, parsed.params, parsed.query, parsed.fragment)
    )


def extract_redgifs_slug(token: str) -> str:
    """
    Normalize user input (watch URL, ifr URL, media mp4, or bare slug) to a lowercase slug.
    """

    trimmed = token.strip()

    if not trimmed:
        raise SystemExit("Empty RedGIFs slug or URL.")

    lowered = trimmed.lower()
    if lowered.startswith("http://") or lowered.startswith("https://"):
        parsed = urlparse(trimmed)
        host = (parsed.hostname or "").lower()
        resource_path = parsed.path or ""
        path_lower = resource_path.lower()

        if "/ifr/" in path_lower:
            fragment = resource_path[path_lower.index("/ifr/") + len("/ifr/") :]
            slug = fragment.strip("/").split("/")[0]

        elif "/watch/" in path_lower:
            fragment = resource_path[path_lower.index("/watch/") + len("/watch/") :]
            slug = fragment.strip("/").split("/")[0]

        elif host == "media.redgifs.com":
            filename = resource_path.rsplit("/", 1)[-1]
            if not filename.lower().endswith(".mp4"):
                raise SystemExit(
                    f"Expected media.redgifs.com … .mp4 for slug inference, got: {token!r}"
                )

            slug = filename[:-4]

        else:
            raise SystemExit(f"Unrecognized RedGIFs URL (need /watch/, /ifr/, or media mp4): {token!r}")

        slug = slug.split("?")[0].split("#")[0]

    else:
        slug = trimmed

    slug = slug.strip().strip("/")
    slug_normalized = slug.lower()

    if not re.match(r"^[a-z0-9_-]+$", slug_normalized):
        raise SystemExit(f"Invalid RedGIFs slug after parsing: {token!r} -> {slug_normalized!r}")

    return slug_normalized


def redgifs_ifr_https_url(slug: str) -> str:
    return "https://www.redgifs.com/ifr/" + slug


def build_redgifs_iframe_wrapper_html(ifr_https_url: str) -> str:
    """Minimal full-viewport iframe embed (RedGIFs /ifr/ page inside local wrapper)."""

    safe_src = html.escape(ifr_https_url.strip(), quote=True)

    document_lines = [
        "<!DOCTYPE html>",
        '<html lang="en">',
        "<head>",
        '<meta charset="utf-8"/>',
        '<meta name="viewport" content="width=device-width, initial-scale=1"/>',
        "<title>RedGIFs</title>",
        "<style>",
        "html, body { margin:0; padding:0; width:100%; height:100%; background:#000; overflow:hidden; }",
        ".wrap { position:relative; width:100%; height:100%; }",
        "iframe {",
        "  position:absolute;",
        "  top:0;",
        "  left:0;",
        "  width:100%;",
        "  height:100%;",
        "  border:0;",
        "}",
        "</style>",
        "</head>",
        "<body>",
        '<div class="wrap">',
        f'<iframe src="{safe_src}" scrolling="no" allowfullscreen></iframe>',
        "</div>",
        "</body>",
        "</html>",
        "",
    ]

    return "\n".join(document_lines)


def materialize_redgifs_iframe_wrappers(
    scene_folder: Path,
    slugs: list[str],
    dry_run: bool,
) -> list[str]:
    """Writes one HTML per distinct slug; returns VaM-root relative paths (same order as slugs)."""

    fill_root = scene_folder / "_vam_browser_redgifs_iframe"
    mapping_cache: dict[str, str] = {}
    resolved_paths: list[str] = []

    repo_root = _REPO.resolve()

    for slug in slugs:
        if slug not in mapping_cache:
            target_https = redgifs_ifr_https_url(slug)
            digest = hashlib.sha256(slug.encode("utf-8")).hexdigest()[:16]
            filename = f"redgifs_ifr_{digest}.html"
            html_path = fill_root / filename
            relative_install = html_path.resolve().relative_to(repo_root).as_posix()
            document = build_redgifs_iframe_wrapper_html(target_https)

            if not dry_run:
                fill_root.mkdir(parents=True, exist_ok=True)
                html_path.write_text(document, encoding="utf-8")

            action_word = "Would write" if dry_run else "Wrote"
            print(
                f"{action_word} RedGIFs iframe wrapper {relative_install!r} (slug {slug!r})",
                file=sys.stderr,
            )
            mapping_cache[slug] = relative_install

        resolved_paths.append(mapping_cache[slug])

    return resolved_paths


def looks_like_direct_http_video_url(url: str) -> bool:
    """True when the URL looks like a direct video asset over http(s)."""

    trimmed = url.strip()
    lowered = trimmed.lower()

    if not (lowered.startswith("http://") or lowered.startswith("https://")):
        return False

    parsed = urlparse(trimmed)
    resource_path = (parsed.path or "").lower()

    return resource_path.endswith((".mp4", ".webm", ".m4v", ".ogg"))


def build_viewport_fill_html(
    video_src_url: str,
    object_fit: str,
    muted: bool,
    show_controls: bool,
) -> str:
    """Single-page player: video stretched to the browser viewport via CSS."""

    safe_src = html.escape(video_src_url.strip(), quote=True)
    muted_attr = " muted" if muted else ""
    controls_attr = " controls" if show_controls else ""

    document_lines = [
        "<!DOCTYPE html>",
        '<html lang="en">',
        "<head>",
        '<meta charset="utf-8"/>',
        '<meta name="viewport" content="width=device-width, initial-scale=1"/>',
        "<title>VaM video fill</title>",
        "<style>",
        "html, body { margin:0; padding:0; width:100%; height:100%; background:#000; overflow:hidden; }",
        "video {",
        "  position: fixed;",
        "  top: 0;",
        "  left: 0;",
        "  right: 0;",
        "  bottom: 0;",
        "  width: 100%;",
        "  height: 100%;",
        f"  object-fit: {object_fit};",
        "}",
        "</style>",
        "</head>",
        "<body>",
        f'<video src="{safe_src}" autoplay loop playsinline{muted_attr}{controls_attr}></video>',
        "<script>",
        "(function () {",
        "  var video = document.querySelector('video');",
        "  if (!video) { return; }",
        "  video.addEventListener('click', function () {",
        "    if (video.paused) { video.play(); }",
        "  });",
        "})();",
        "</script>",
        "</body>",
        "</html>",
        "",
    ]

    return "\n".join(document_lines)


def materialize_viewport_fill_pages(
    scene_folder: Path,
    video_urls: list[str],
    object_fit: str,
    video_muted: bool,
    video_controls: bool,
    dry_run: bool,
) -> list[str]:
    """
    Writes HTML under scene_folder/_vam_browser_video_fill/ unless dry_run.
    Returns one VaM‑root relative posix path per input URL (same order, cycling-ready).
    """

    fill_root = scene_folder / "_vam_browser_video_fill"
    mapping_cache: dict[str, str] = {}
    resolved_paths: list[str] = []

    repo_root = _REPO.resolve()

    for raw_url in video_urls:
        trimmed = raw_url.strip()

        if trimmed not in mapping_cache:
            digest = hashlib.sha256(trimmed.encode("utf-8")).hexdigest()[:16]
            filename = f"viewport_{digest}.html"
            html_path = fill_root / filename
            relative_install = html_path.resolve().relative_to(repo_root).as_posix()
            document = build_viewport_fill_html(
                trimmed,
                object_fit=object_fit,
                muted=video_muted,
                show_controls=video_controls,
            )

            if not dry_run:
                fill_root.mkdir(parents=True, exist_ok=True)
                html_path.write_text(document, encoding="utf-8")

            action_word = "Would write" if dry_run else "Wrote"
            print(
                f"{action_word} viewport-fill page {relative_install!r} (video {trimmed!r})",
                file=sys.stderr,
            )
            mapping_cache[trimmed] = relative_install

        resolved_paths.append(mapping_cache[trimmed])

    return resolved_paths


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


def ensure_original_backup(scene_path: Path, dry_run: bool) -> Path | None:
    """
    Returns the path of ``scene.json.bak`` if a new copy was made, or None if
    it already existed (original backup kept as-is).
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
    urls = list(args.urls)

    if args.video_fill_viewport and args.redgifs_ifr:
        raise SystemExit("Choose only one of --video-fill-viewport or --redgifs-ifr.")

    if args.redgifs_ifr_wrapper_html and not args.redgifs_ifr:
        raise SystemExit("--redgifs-ifr-wrapper-html requires --redgifs-ifr.")

    skip_redgifs_watch_rewrite = (
        args.keep_redgifs_media_mp4 or args.video_fill_viewport or args.redgifs_ifr
    )

    if args.redgifs_ifr:
        slug_list = [extract_redgifs_slug(token) for token in urls]

        if args.redgifs_ifr_wrapper_html:
            urls = materialize_redgifs_iframe_wrappers(folder, slug_list, args.dry_run)
        else:
            urls = [redgifs_ifr_https_url(slug) for slug in slug_list]
            for slug, browser_url in zip(slug_list, urls):
                print(
                    f"RedGIFs BrowserGUI URL: {browser_url!r} (slug {slug!r})",
                    file=sys.stderr,
                )

    elif args.video_fill_viewport:
        for candidate_url in urls:
            if not looks_like_direct_http_video_url(candidate_url):
                raise SystemExit(
                    "--video-fill-viewport needs direct http(s) video URLs "
                    "(path ending in .mp4, .webm, .m4v, or .ogg). "
                    f"Offending argument: {candidate_url!r}"
                )

        urls = materialize_viewport_fill_pages(
            folder,
            urls,
            object_fit=args.video_object_fit,
            video_muted=not args.video_unmuted,
            video_controls=not args.video_hide_controls,
            dry_run=args.dry_run,
        )

    elif not skip_redgifs_watch_rewrite:
        mapped: list[str] = []
        for original_url in urls:
            normalized_url = normalize_redgifs_media_mp4_to_watch(original_url)

            if normalized_url != original_url.strip():
                print(
                    f"Using RedGIFs watch URL: {normalized_url!r} (from {original_url!r})",
                    file=sys.stderr,
                )

            mapped.append(normalized_url)

        urls = mapped

    if not args.no_porngifs_canonical:
        canonical_urls: list[str] = []
        for candidate in urls:
            rewritten = normalize_porngifs_browser_url(candidate)

            if rewritten != candidate.strip():
                print(
                    f"Canonical porngifs URL: {rewritten!r} (from {candidate!r})",
                    file=sys.stderr,
                )

            canonical_urls.append(rewritten)

        urls = canonical_urls

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
    skipped_backup_count = 0

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

        backup_written_path = ensure_original_backup(scene_path, args.dry_run)

        if backup_written_path is None and not args.dry_run:
            skipped_backup_count += 1

        if args.dry_run:
            print(
                f"[dry-run] would update {changed} BrowserGUI url(s) in "
                f"{relative_repo(scene_path)}",
                flush=True,
            )

            if backup_written_path is not None:
                print(f"          backup -> {relative_repo(backup_written_path)}", flush=True)

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

        label_backup = "existing .bak unchanged"
        if backup_written_path is not None:
            label_backup = relative_repo(backup_written_path)

        print(
            f"OK {relative_repo(scene_path)} — {changed} panel(s); backup: {label_backup}",
            flush=True,
        )
        total_files_touched += 1
        total_panels_changed += changed

    print(
        f"Done. Files modified: {total_files_touched}; BrowserGUI url updates: {total_panels_changed}.",
        flush=True,
    )

    if skipped_backup_count > 0 and not args.dry_run:
        print(
            f"Original backup (*.json.bak) already existed for {skipped_backup_count} file(s); "
            "those scenes were updated without creating another backup.",
            file=sys.stderr,
        )

    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
