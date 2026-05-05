# -*- coding: utf-8 -*-
"""
Manual camera / rig patch: you choose the exact scene .json on disk.

Uses the same patch engine as patch_scene_initial_camera.py. After pressing K
in VaM, copy last_scene_camera_patch_request.json from this folder, or use the
path Easy Mate logs.

Usage:
  python patch_scene_camera_manual_cli.py <ABS_SCENE_JSON> <ABS_REQUEST_JSON>

Example (Windows):
  python patch_scene_camera_manual_cli.py ^
    "D:/Games/VaM/Saves/scene/sapuzex/jess bdroom/MyScene.json" ^
    "D:/Games/VaM/Custom/Scripts/Easy Mate/tools/last_scene_camera_patch_request.json"
"""
from __future__ import print_function

import os
import sys

_tools_dir = os.path.dirname(os.path.abspath(__file__))
if _tools_dir not in sys.path:
    sys.path.insert(0, _tools_dir)

import patch_scene_initial_camera


def main():
    argv = sys.argv[1:]
    if len(argv) < 2:
        print(__doc__.strip(), file=sys.stderr)
        return 2
    return patch_scene_initial_camera.main(argv) or 0


if __name__ == "__main__":
    sys.exit(main())
