#!/usr/bin/env python3
"""
Insert ImagePanelEmissive atoms immediately BEFORE each UIButton that loads a scene
(LoadScene in Trigger.startActions), matching container pose, offset 1 cm behind
the button plane (local -Z in Unity/VaM UI convention), and matching size:
the ImagePanel uses the SAME atom ``position`` / ``rotation`` as the UIButton (VaM
scales the quad by those values; keeping the old 3.166×1.093 template swells the panel).
Storables ``scale`` matches Canvas×0.001×atom.y using the 2.03 reference (times
``THUMB_SCALE_EXPERIMENT_MULT`` for easy tuning — currently ¼ size).

Image panel Y = button :control Y + ¼ of button world height + ``THUMB_Y_EXTRA_M`` (tune; 0.05 m = 5 cm).

Thumbnails are added only for ``UIButton`` slots whose ``LoadScene`` path **differs**
from ``Saves/scene/MainMenu_Original.json`` (same button ``id``). Otherwise any
``_SceneThumb_*`` is removed.
JPEG next to the scene ``.json`` (same name, or newest in folder); if none, no panel.

After a successful edit to ``Default.json``, this script **copies** it to
``MainMenu.json`` so both hub files stay identical (same contract as
``rewire_hub_scene_button.py``). Use ``--no-sync-main-menu`` only if you are
sure you want to skip that mirror.

Run from repo / VaM root (folder that contains ``Saves/scene/`` and ``Custom/Scripts/Gabriel/tools/scene-menu/``):
  python Custom/Scripts/Gabriel/tools/scene-menu/inject_default_scene_thumbnails.py
"""

from __future__ import annotations

import json
import math
import os
import shutil
import sys
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

BACK_M = 0.01  # 1 cm behind button plane (local -Z after container rotation)
# VaM: ~1 canvas pixel = 1 mm in world for these menu atoms; atom position.y scales height.
CANVAS_PIXEL_TO_METERS = 0.001
REF_CALIB_CANVAS_Y = 830.0
REF_CALIB_ATOM_POS_Y = 1.0
REF_CALIB_PANEL_SCALE = 2.03
_IMAGE_PANEL_UNIT_SCALE_WORLD_HEIGHT = (
    REF_CALIB_CANVAS_Y * CANVAS_PIXEL_TO_METERS * REF_CALIB_ATOM_POS_Y
) / REF_CALIB_PANEL_SCALE
THUMB_SCALE_EXPERIMENT_MULT = 0.25
THUMB_Y_QUARTER_HEIGHT_FRAC = 0.25
THUMB_Y_EXTRA_M = 0.05

DEFAULT_BASELINE_MENU_REL = os.path.join("Saves", "scene", "MainMenu_Original.json")
_MAIN_MENU_SCENE_REL = os.path.join("Saves", "scene", "MainMenu.json")


def _find_vam_root_from_here() -> str:
    """Folder that contains ``Saves/scene`` (walk up from this script)."""
    start = Path(__file__).resolve()
    for anc in [start.parent, *start.parents]:
        if (anc / "Saves" / "scene").is_dir():
            return str(anc)
    return str(start.parent.parent)


def _f(s: Any) -> float:
    return float(s)


def _fmt(x: float) -> str:
    if abs(x - round(x)) < 1e-6:
        return str(int(round(x)))
    t = f"{x:.8f}".rstrip("0").rstrip(".")
    return t if t else "0"


def _abs_path_to_vam_url(vam_root: str, abs_file: str) -> Optional[str]:
    abs_file = os.path.normpath(os.path.abspath(abs_file))
    root = os.path.normpath(os.path.abspath(vam_root))
    try:
        rel = os.path.relpath(abs_file, root)
    except ValueError:
        return None
    if rel.startswith(".."):
        return None
    return rel.replace("\\", "/")


def _scene_json_abs_path(vam_root: str, scene_path: str) -> str:
    s = scene_path.strip().replace("\\", "/").lstrip("./")
    if s.lower().startswith("saves/scene/"):
        return os.path.normpath(os.path.join(vam_root, s.replace("/", os.sep)))
    return os.path.normpath(os.path.join(vam_root, "Saves", "scene", s.replace("/", os.sep)))


def _newest_jpeg_in_dir(directory: str) -> Optional[str]:
    candidates: List[Tuple[float, str]] = []
    try:
        for name in os.listdir(directory):
            if name.lower().endswith((".jpg", ".jpeg")):
                fp = os.path.join(directory, name)
                if os.path.isfile(fp):
                    candidates.append((os.path.getmtime(fp), fp))
    except OSError:
        return None
    if not candidates:
        return None
    candidates.sort(key=lambda t: t[0], reverse=True)
    return candidates[0][1]


def _pick_scene_adjacent_jpeg(vam_root: str, scene_path: str) -> Optional[str]:
    json_abs = _scene_json_abs_path(vam_root, scene_path)
    folder = os.path.dirname(json_abs)
    stem, _ = os.path.splitext(os.path.basename(json_abs))
    for ext in (".jpg", ".jpeg"):
        preferred = os.path.join(folder, stem + ext)
        if os.path.isfile(preferred):
            url = _abs_path_to_vam_url(vam_root, preferred)
            if url:
                return url
    newest = _newest_jpeg_in_dir(folder)
    if newest:
        url = _abs_path_to_vam_url(vam_root, newest)
        if url:
            return url
    return None


def _thumb_url(scene_path: str, vam_root: str) -> Optional[str]:
    return _pick_scene_adjacent_jpeg(vam_root, scene_path)


def _get_storable(atom: Dict[str, Any], sid: str) -> Optional[Dict[str, Any]]:
    for st in atom.get("storables", []):
        if st.get("id") == sid:
            return st
    return None


def _extract_load_scene_path(atom: Dict[str, Any]) -> Optional[str]:
    tr = _get_storable(atom, "Trigger")
    if not tr:
        return None
    start = tr.get("trigger", {}).get("startActions") or []
    if not start:
        return None
    a0 = start[0]
    if a0.get("receiver") != "SceneLoader" or a0.get("receiverTargetName") != "LoadScene":
        return None
    p = a0.get("sceneFilePath")
    return str(p) if p else None


def _norm_scene_path(p: str) -> str:
    return p.strip().replace("\\", "/").lstrip("./")


def _load_baseline_load_scene_by_button_id(
    vam_root: str, baseline_rel: str
) -> Optional[Dict[str, str]]:
    p = os.path.join(vam_root, baseline_rel.replace("/", os.sep))
    if not os.path.isfile(p):
        print("Baseline menu not found:", p, file=sys.stderr)
        return None
    with open(p, "r", encoding="utf-8") as f:
        root = json.load(f)
    out: Dict[str, str] = {}
    for a in root.get("atoms") or []:
        if a.get("type") != "UIButton":
            continue
        sp = _extract_load_scene_path(a)
        if sp:
            out[str(a["id"])] = _norm_scene_path(sp)
    return out


def _thumb_slot_replaced_from_baseline(
    baseline: Dict[str, str], button_id: str, scene_path: str
) -> bool:
    bid = str(button_id)
    if bid not in baseline:
        return False
    return baseline[bid] != _norm_scene_path(scene_path)


def _rot_vec_unity_euler_deg(ex: float, ey: float, ez: float, vx: float, vy: float, vz: float) -> Tuple[float, float, float]:
    rx, ry, rz = map(math.radians, (ex, ey, ez))
    cx, sx = math.cos(rx), math.sin(rx)
    cy, sy = math.cos(ry), math.sin(ry)
    cz, sz = math.cos(rz), math.sin(rz)
    x1 = cz * vx - sz * vy
    y1 = sz * vx + cz * vy
    z1 = vz
    x2 = x1
    y2 = cx * y1 - sx * z1
    z2 = sx * y1 + cx * z1
    x3 = cy * x2 + sy * z2
    y3 = y2
    z3 = -sy * x2 + cy * z2
    return x3, y3, z3


def _offset_container_back(
    cpx: str, cpy: str, cpz: str, rotx: str, roty: str, rotz: str
) -> Tuple[str, str, str]:
    ex, ey, ez = _f(rotx), _f(roty), _f(rotz)
    dx, dy, dz = _rot_vec_unity_euler_deg(ex, ey, ez, 0.0, 0.0, -BACK_M)
    return _fmt(_f(cpx) + dx), _fmt(_f(cpy) + dy), _fmt(_f(cpz) + dz)


def _canvas_sizes(button: Dict[str, Any]) -> Tuple[str, str]:
    st = _get_storable(button, "Canvas")
    if not st:
        return "830", "830"
    return str(st.get("xSize", "830")), str(st.get("ySize", "830"))


def _button_world_height_m(canvas_y: str, atom_pos_y: str) -> float:
    return float(canvas_y) * CANVAS_PIXEL_TO_METERS * float(atom_pos_y)


def _thumb_scale_for_button(button: Dict[str, Any]) -> str:
    _, ysz = _canvas_sizes(button)
    pos = button.get("position") or {}
    atom_y = str(pos.get("y", "1"))
    h = _button_world_height_m(ysz, atom_y)
    if _IMAGE_PANEL_UNIT_SCALE_WORLD_HEIGHT <= 0:
        base = 2.03
    else:
        base = h / _IMAGE_PANEL_UNIT_SCALE_WORLD_HEIGHT
    return _fmt(base * THUMB_SCALE_EXPERIMENT_MULT)


def _build_thumb_atom(
    button: Dict[str, Any], scene_path: str, vam_root: str
) -> Optional[Dict[str, Any]]:
    url = _thumb_url(scene_path, vam_root)
    if not url:
        return None

    bid = str(button["id"])
    thumb_id = "_SceneThumb_" + bid

    cpx = str(button["containerPosition"]["x"])
    cpy = str(button["containerPosition"]["y"])
    cpz = str(button["containerPosition"]["z"])
    rx = str(button["containerRotation"]["x"])
    ry = str(button["containerRotation"]["y"])
    rz = str(button["containerRotation"]["z"])

    ox, _, oz = _offset_container_back(cpx, cpy, cpz, rx, ry, rz)

    ctrl = _get_storable(button, "control") or {}
    ctrl_pos = ctrl.get("position") or {}
    control_y = _f(str(ctrl_pos.get("y", cpy)))
    _, canvas_y = _canvas_sizes(button)
    atom_y = str((button.get("position") or {}).get("y", "1"))
    h_world = _button_world_height_m(canvas_y, atom_y)
    oy = _fmt(control_y + h_world * THUMB_Y_QUARTER_HEIGHT_FRAC + THUMB_Y_EXTRA_M)

    panel_scale = _thumb_scale_for_button(button)

    atom_pos = button.get("position") or {"x": "1", "y": "1", "z": "1"}
    atom_rot = button.get("rotation") or {"x": "0", "y": "0", "z": "0"}

    crx = str((ctrl.get("rotation") or button["containerRotation"])["x"])
    cry = str((ctrl.get("rotation") or button["containerRotation"])["y"])
    crz = str((ctrl.get("rotation") or button["containerRotation"])["z"])

    return {
        "id": thumb_id,
        "on": "true",
        "type": "ImagePanelEmissive",
        "position": {k: str(v) for k, v in atom_pos.items()},
        "rotation": {k: str(v) for k, v in atom_rot.items()},
        "containerPosition": {"x": ox, "y": oy, "z": oz},
        "containerRotation": {
            "x": str(button["containerRotation"]["x"]),
            "y": str(button["containerRotation"]["y"]),
            "z": str(button["containerRotation"]["z"]),
        },
        "storables": [
            {
                "id": "CollisionTrigger",
                "trigger": {"startActions": [], "transitionActions": [], "endActions": []},
            },
            {"id": "scale", "scale": panel_scale},
            {"id": "Image", "url": url},
            {"id": "PluginManager", "plugins": {}},
            {
                "id": "control",
                "position": {"x": ox, "y": oy, "z": oz},
                "rotation": {"x": crx, "y": cry, "z": crz},
            },
        ],
    }


def inject(
    vam_root: str,
    scene_rel: str,
    baseline_rel: str,
    dry_run: bool,
    sync_main_menu: bool,
) -> int:
    scene_path = os.path.join(vam_root, scene_rel.replace("/", os.sep))
    if not os.path.isfile(scene_path):
        print("Scene not found:", scene_path, file=sys.stderr)
        return 1

    baseline = _load_baseline_load_scene_by_button_id(vam_root, baseline_rel)
    if baseline is None:
        return 1

    with open(scene_path, "r", encoding="utf-8") as f:
        root = json.load(f)

    atoms: List[Dict[str, Any]] = root.get("atoms") or []
    new_atoms: List[Dict[str, Any]] = []
    inserted = 0
    i = 0
    while i < len(atoms):
        a = atoms[i]
        if a.get("type") == "UIButton" and _extract_load_scene_path(a):
            sp = _extract_load_scene_path(a)
            assert sp is not None
            if i > 0 and str(atoms[i - 1].get("id", "")).startswith("_SceneThumb_"):
                new_atoms.append(a)
                i += 1
                continue
            if _thumb_slot_replaced_from_baseline(baseline, str(a["id"]), sp):
                thumb = _build_thumb_atom(a, sp, vam_root)
                if thumb is not None:
                    new_atoms.append(thumb)
                    inserted += 1
        new_atoms.append(a)
        i += 1

    root["atoms"] = new_atoms
    resynced, stripped, dropped = _resync_scene_thumbs(root["atoms"], vam_root, baseline)

    if dry_run:
        print(
            "Dry run: would insert",
            inserted,
            "thumbs; resync",
            resynced,
            "; strip",
            stripped,
            "(unchanged vs baseline); drop",
            dropped,
            "(replaced, no JPEG)",
        )
        return 0

    bak = scene_path + ".bak"
    shutil.copy2(scene_path, bak)
    with open(scene_path, "w", encoding="utf-8") as f:
        json.dump(root, f, indent=3)
        f.write("\n")
    print(
        "Inserted",
        inserted,
        "thumbnails; resynced",
        resynced,
        "; stripped",
        stripped,
        "(unchanged); dropped",
        dropped,
        "(no JPEG). Backup:",
        bak,
    )
    if sync_main_menu and _is_hub_default_scene(scene_rel):
        mp = os.path.join(vam_root, _MAIN_MENU_SCENE_REL.replace("/", os.sep))
        shutil.copyfile(scene_path, mp)
        print(
            "COPIED:",
            scene_path,
            "->",
            mp,
        )
    return 0


def _is_hub_default_scene(scene_rel: str) -> bool:
    tail = scene_rel.strip().replace("\\", "/").lstrip("./").lower()
    return tail.endswith("saves/scene/default.json")


def _resync_scene_thumbs(
    atoms: List[Dict[str, Any]],
    vam_root: str,
    baseline: Dict[str, str],
) -> Tuple[int, int, int]:
    out: List[Dict[str, Any]] = []
    i = 0
    updated = 0
    stripped = 0
    dropped = 0
    while i < len(atoms):
        a = atoms[i]
        tid = str(a.get("id", ""))
        if tid.startswith("_SceneThumb_") and i + 1 < len(atoms):
            b = atoms[i + 1]
            bid = tid[len("_SceneThumb_") :]
            if str(b.get("id")) == bid and b.get("type") == "UIButton":
                sp = _extract_load_scene_path(b)
                if sp:
                    if not _thumb_slot_replaced_from_baseline(baseline, str(b.get("id")), sp):
                        stripped += 1
                        out.append(b)
                        i += 2
                        continue
                    rebuilt = _build_thumb_atom(b, sp, vam_root)
                    if rebuilt is None:
                        dropped += 1
                        out.append(b)
                        i += 2
                        continue
                    updated += 1
                    out.append(rebuilt)
                    out.append(b)
                    i += 2
                    continue
        out.append(a)
        i += 1
    atoms[:] = out
    return updated, stripped, dropped


def main() -> int:
    vam_root = _find_vam_root_from_here()
    scene_rel = os.path.join("Saves", "scene", "Default.json")
    dry = "--dry-run" in sys.argv
    sync = "--no-sync-main-menu" not in sys.argv
    return inject(
        vam_root, scene_rel, DEFAULT_BASELINE_MENU_REL, dry, sync_main_menu=sync
    )


if __name__ == "__main__":
    raise SystemExit(main())
