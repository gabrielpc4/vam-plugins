#!/usr/bin/env python3
"""
Shift ImagePanelEmissive atoms (except _SceneThumb_*) along local -Z by OFFSET_M
meters relative to containerRotation — same convention as scene thumbnail depth
(Unity/VaM UI).

Default OFFSET_M is 0 so re-running does not move panels again. Set OFFSET_M to
0.05 (etc.) when you need another step.

Run from repo / VaM root:
  python Custom/Scripts/tools/scene-menu/offset_non_thumb_image_panels_back.py
"""

from __future__ import annotations

import json
import math
import os
import shutil
import sys
from typing import Any, Dict, List, Optional, Tuple

OFFSET_M = 0.0


def _f(s: Any) -> float:
    return float(s)


def _fmt(x: float) -> str:
    if abs(x - round(x)) < 1e-6:
        return str(int(round(x)))
    t = f"{x:.8f}".rstrip("0").rstrip(".")
    return t if t else "0"


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


def _get_storable(atom: Dict[str, Any], sid: str) -> Optional[Dict[str, Any]]:
    for st in atom.get("storables", []):
        if st.get("id") == sid:
            return st
    return None


def _apply_offset(atom: Dict[str, Any]) -> bool:
    cp = atom.get("containerPosition")
    cr = atom.get("containerRotation")
    if not isinstance(cp, dict) or not isinstance(cr, dict):
        return False
    ex = _f(str(cr.get("x", "0")))
    ey = _f(str(cr.get("y", "0")))
    ez = _f(str(cr.get("z", "0")))
    dx, dy, dz = _rot_vec_unity_euler_deg(ex, ey, ez, 0.0, 0.0, -OFFSET_M)

    cp["x"] = _fmt(_f(str(cp["x"])) + dx)
    cp["y"] = _fmt(_f(str(cp["y"])) + dy)
    cp["z"] = _fmt(_f(str(cp["z"])) + dz)

    ctrl = _get_storable(atom, "control")
    if ctrl and isinstance(ctrl.get("position"), dict):
        p = ctrl["position"]
        p["x"] = _fmt(_f(str(p["x"])) + dx)
        p["y"] = _fmt(_f(str(p["y"])) + dy)
        p["z"] = _fmt(_f(str(p["z"])) + dz)
    return True


def run(vam_root: str, scene_rel: str, dry_run: bool) -> int:
    scene_path = os.path.join(vam_root, scene_rel.replace("/", os.sep))
    if not os.path.isfile(scene_path):
        print("Scene not found:", scene_path, file=sys.stderr)
        return 1

    with open(scene_path, "r", encoding="utf-8") as f:
        root = json.load(f)

    atoms: List[Dict[str, Any]] = root.get("atoms") or []
    n = 0
    for a in atoms:
        if a.get("type") != "ImagePanelEmissive":
            continue
        aid = str(a.get("id", ""))
        if aid.startswith("_SceneThumb_"):
            continue
        if _apply_offset(a):
            n += 1

    if dry_run:
        print(f"Dry run: would offset {n} ImagePanelEmissive atoms by {OFFSET_M} m local -Z")
        return 0

    bak = scene_path + ".bak"
    shutil.copy2(scene_path, bak)
    with open(scene_path, "w", encoding="utf-8") as f:
        json.dump(root, f, indent=3)
        f.write("\n")
    print(f"Offset {n} ImagePanelEmissive (non-thumb) by {OFFSET_M} m. Backup:", bak)
    return 0


def main() -> int:
    here = os.path.dirname(os.path.abspath(__file__))
    vam_root = os.path.abspath(os.path.join(here, ".."))
    scene_rel = os.path.join("Saves", "scene", "Default.json")
    dry = "--dry-run" in sys.argv
    return run(vam_root, scene_rel, dry)


if __name__ == "__main__":
    raise SystemExit(main())
