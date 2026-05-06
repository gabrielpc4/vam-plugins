# -*- coding: utf-8 -*-
"""
Patch VaM scene JSON (initial camera / rig / monitor peel) from a request file
written by EasyMate (EasyMateKSceneCameraPatch). See
Reference/VaM-Camera-Initial-Scene-Pose.md.

Usage:
  python patch_scene_initial_camera.py <scene_json_abs> <request_json_abs>

Creates <scene>.json.backup once (if missing), then patches the target scene
JSON using string-preserving replacements (suited to very large files).
"""
from __future__ import print_function

import io
import json
import os
import re
import shutil
import sys
import time


LOG_NAME = "last_scene_camera_patch_log.txt"


def _log(msg, log_fp=None):
    line = "[%s] %s\n" % (time.strftime("%Y-%m-%d %H:%M:%S"), msg)
    sys.stderr.write(line)
    if log_fp is not None:
        log_fp.write(line)
        log_fp.flush()


def fmt_num(v):
    try:
        f = float(v)
    except (TypeError, ValueError):
        return "0"
    if abs(f) < 1e-8:
        return "0"
    s = "%.15g" % f
    if s in ("-0", "-0.0"):
        return "0"
    return s


def atom_end(s, open_idx):
    n = len(s)
    if open_idx >= n or s[open_idx] != "{":
        return -1
    depth = 0
    i = open_idx
    in_str = False
    esc = False
    while i < n:
        ch = s[i]
        if in_str:
            if esc:
                esc = False
            elif ch == "\\":
                esc = True
            elif ch == '"':
                in_str = False
        else:
            if ch == '"':
                in_str = True
            elif ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    return i
        i += 1
    return -1


def find_atom_bounds(content, uid):
    needle_spaced = '"id" : "%s"' % uid
    needle_compact = '"id":"%s"' % uid
    needle_va_colon_space = '"id": "%s"' % uid
    pos = content.find(needle_spaced)
    if pos < 0:
        pos = content.find(needle_compact)
    if pos < 0:
        pos = content.find(needle_va_colon_space)
    if pos < 0:
        return None
    sub_start = content.rfind("{", 0, pos)
    if sub_start < 0:
        return None
    end = atom_end(content, sub_start)
    if end < 0:
        return None
    return sub_start, end + 1


def patch_xyz_block(text, key, x, y, z):
    pat = r'("%s"\s*:\s*\{\s*)"x"\s*:\s*"[^"]*"(\s*,\s*"y"\s*:\s*)"[^"]*"(\s*,\s*"z"\s*:\s*)"[^"]*"(\s*\})' % (
        re.escape(key),
    )

    def repl(m):
        return (
            m.group(1)
            + '"x" : "%s"' % fmt_num(x)
            + m.group(2)
            + '"%s"' % fmt_num(y)
            + m.group(3)
            + '"%s"' % fmt_num(z)
            + m.group(4)
        )

    new_t, n = re.subn(pat, repl, text, count=1, flags=re.DOTALL)
    if n != 1:
        raise ValueError("patch %s: expected 1 substitution, got %d" % (key, n))
    return new_t


def patch_player_height(content, value):
    pat = r'("playerHeightAdjust"\s*:\s*)"[^"]*"'
    new_c, n = re.subn(pat, r'\1"%s"' % fmt_num(value), content, count=1)
    if n != 1:
        raise ValueError("playerHeightAdjust: expected 1 substitution, got %d" % n)
    return new_c


def patch_monitor_rotation(content, x, y, z):
    pat = r'("monitorCameraRotation"\s*:\s*\{\s*)"x"\s*:\s*"[^"]*"(\s*,\s*"y"\s*:\s*)"[^"]*"(\s*,\s*"z"\s*:\s*)"[^"]*"(\s*\})'

    def repl(m):
        return (
            m.group(1)
            + '"x" : "%s"' % fmt_num(x)
            + m.group(2)
            + '"%s"' % fmt_num(y)
            + m.group(3)
            + '"%s"' % fmt_num(z)
            + m.group(4)
        )

    new_c, n = re.subn(pat, repl, content, count=1, flags=re.DOTALL)
    if n != 1:
        raise ValueError("monitorCameraRotation: expected 1 substitution, got %d" % n)
    return new_c


def pick_main_scene_json(scene_dir):
    folder = os.path.normpath(scene_dir)
    base = os.path.basename(folder)
    try:
        names = os.listdir(folder)
    except OSError as e:
        raise SystemExit("listdir %s: %s" % (folder, e))

    json_files = [
        n
        for n in names
        if n.lower().endswith(".json") and n.lower() != "meta.json"
    ]
    if not json_files:
        raise SystemExit("no .json files in %s" % folder)

    preferred = base + ".json"
    if preferred in json_files:
        return os.path.join(folder, preferred)

    lower_to_name = {}
    for n in json_files:
        lower_to_name[n.lower()] = n
    pl = preferred.lower()
    if pl in lower_to_name:
        return os.path.join(folder, lower_to_name[pl])

    def bad_name(n):
        u = n.upper()
        return "COPY" in u or "COPIA" in u or "ORIGINAL" in u

    filtered = [n for n in json_files if not bad_name(n)]
    if not filtered:
        filtered = json_files
    longest = max(filtered, key=len)
    return os.path.join(folder, longest)


def resolve_scene_json_target(scene_target):
    normalized_target = os.path.normpath(scene_target)

    if os.path.isdir(normalized_target):
        return pick_main_scene_json(normalized_target)

    if not normalized_target.lower().endswith(".json"):
        raise SystemExit("target is not a .json file: %s" % normalized_target)

    if not os.path.isfile(normalized_target):
        raise SystemExit("scene json not found: %s" % normalized_target)

    return normalized_target


def patch_window_camera_atom(atom, wc):
    atom2 = patch_xyz_block(atom, "position", wc["position"]["x"], wc["position"]["y"], wc["position"]["z"])
    atom3 = patch_xyz_block(atom2, "rotation", wc["rotation"]["x"], wc["rotation"]["y"], wc["rotation"]["z"])
    atom4 = patch_xyz_block(
        atom3, "containerPosition", wc["containerPosition"]["x"], wc["containerPosition"]["y"], wc["containerPosition"]["z"]
    )
    atom5 = patch_xyz_block(
        atom4,
        "containerRotation",
        wc["containerRotation"]["x"],
        wc["containerRotation"]["y"],
        wc["containerRotation"]["z"],
    )
    ci = atom5.find('"id" : "control"')
    if ci < 0:
        ci = atom5.find('"id":"control"')
    if ci < 0:
        ci = atom5.find('"id": "control"')
    if ci < 0:
        raise ValueError("WindowCamera: no control storable found")
    tail = atom5[ci:]
    tail2 = patch_xyz_block(tail, "position", wc["controlPosition"]["x"], wc["controlPosition"]["y"], wc["controlPosition"]["z"])
    tail3 = patch_xyz_block(tail2, "rotation", wc["controlRotation"]["x"], wc["controlRotation"]["y"], wc["controlRotation"]["z"])
    return atom5[:ci] + tail3


def main(argv=None):
    if argv is None:
        argv = sys.argv[1:]
    if len(argv) < 2:
        print("Usage: patch_scene_initial_camera.py <scene_json> <request_json>", file=sys.stderr)
        return 2

    scene_target = argv[0]
    request_path = argv[1]

    tools_dir = os.path.dirname(os.path.abspath(__file__))
    log_path = os.path.join(tools_dir, LOG_NAME)

    with io.open(log_path, "a", encoding="utf-8") as log_fp:
        try:
            with io.open(request_path, "r", encoding="utf-8") as rf:
                req = json.load(rf)
        except Exception as e:
            _log("FAIL read request: %s" % e, log_fp)
            return 1

        try:
            scene_path = resolve_scene_json_target(scene_target)
        except SystemExit as e:
            _log("FAIL %s" % e, log_fp)
            return 1

        backup_path = scene_path + ".backup"
        _log("scene json=%s" % scene_path, log_fp)

        # First successful run snapshots the untouched scene once; further runs reuse in-place edits.
        if not os.path.isfile(backup_path):
            try:
                shutil.copy2(scene_path, backup_path)
                _log("created backup %s" % backup_path, log_fp)
            except Exception as e:
                _log("FAIL backup: %s" % e, log_fp)
                return 1
        else:
            _log("backup already exists, skipping: %s" % backup_path, log_fp)

        try:
            with io.open(scene_path, "r", encoding="utf-8", newline="") as sf:
                content = sf.read()
        except Exception as e:
            _log("FAIL read scene: %s" % e, log_fp)
            return 1

        orig_len = len(content)
        try:
            cr = req["cameraRig"]
            content = patch_player_height(content, req["playerHeightAdjust"])
            content = patch_monitor_rotation(
                content,
                req["monitorCameraRotation"]["x"],
                req["monitorCameraRotation"]["y"],
                req["monitorCameraRotation"]["z"],
            )

            b_cr = find_atom_bounds(content, "[CameraRig]")
            if not b_cr:
                raise ValueError("atom [CameraRig] not found")
            s0, e0 = b_cr
            atom_cr = content[s0:e0]
            atom_cr2 = patch_xyz_block(atom_cr, "position", cr["position"]["x"], cr["position"]["y"], cr["position"]["z"])
            atom_cr3 = patch_xyz_block(atom_cr2, "rotation", cr["rotation"]["x"], cr["rotation"]["y"], cr["rotation"]["z"])
            content = content[:s0] + atom_cr3 + content[e0:]

            if req.get("windowCamera") is not None:
                wc = req["windowCamera"]
                b_wc = find_atom_bounds(content, "WindowCamera")
                if not b_wc:
                    raise ValueError("atom WindowCamera not found")
                w0, w1 = b_wc
                atom_wc = content[w0:w1]
                patched_wc = patch_window_camera_atom(atom_wc, wc)
                content = content[:w0] + patched_wc + content[w1:]

            tail = content.rstrip()
            if not tail.endswith("}"):
                raise ValueError("result does not end with }; refusing to write (truncation check)")

            new_len = len(content)
            if new_len < orig_len * 0.5:
                raise ValueError(
                    "result length %d much smaller than original %d; refusing to write" % (new_len, orig_len)
                )

            tmp_path = scene_path + ".tmp"
            with io.open(tmp_path, "w", encoding="utf-8", newline="") as wf:
                wf.write(content)
            os.replace(tmp_path, scene_path)
            _log("OK patched %s (bytes %s -> %s)" % (scene_path, orig_len, new_len), log_fp)
            return 0
        except Exception as e:
            _log("FAIL patch: %s" % e, log_fp)
            return 1


if __name__ == "__main__":
    sys.exit(main() or 0)
