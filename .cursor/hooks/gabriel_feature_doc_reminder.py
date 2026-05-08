#!/usr/bin/env python3
import json
import sys

AREA_DOCS = {
    "Custom/Scripts/Gabriel/bootstrap/": "Custom/Scripts/Gabriel/bootstrap/FEATURE.md",
    "Custom/Scripts/Gabriel/session-plugins/": "Custom/Scripts/Gabriel/session-plugins/FEATURE.md",
    "Custom/Scripts/Gabriel/features/ui-hud/": "Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md",
    "Custom/Scripts/Gabriel/features/palm-hud/": "Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md",
    "Custom/Scripts/Gabriel/features/passenger-possession/": "Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md",
    "Custom/Scripts/Gabriel/features/scene-camera/": "Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md",
    "Custom/Scripts/Gabriel/features/clothing-interactions/": "Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md",
    "Custom/Scripts/Gabriel/features/head-hide/": "Custom/Scripts/Gabriel/features/head-hide/FEATURE.md",
    "Custom/Scripts/Gabriel/features/e-motion/": "Custom/Scripts/Gabriel/features/e-motion/FEATURE.md",
    "Custom/Scripts/Gabriel/features/animation-no-loop/": (
        "Custom/Scripts/Gabriel/features/animation-no-loop/FEATURE.md"
    ),
    "Custom/Scripts/Gabriel/features/spankings/": "Custom/Scripts/Gabriel/features/spankings/FEATURE.md",
    "Custom/Scripts/Gabriel/features/dildo-on-hands/": "Custom/Scripts/Gabriel/features/dildo-on-hands/FEATURE.md",
    "Custom/Scripts/Gabriel/features/improved-pov/": "Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md",
    "Custom/Scripts/Gabriel/tools/scene-camera/": "Custom/Scripts/Gabriel/tools/scene-camera/FEATURE.md",
    "Custom/Scripts/Gabriel/tools/scene-menu/": "Custom/Scripts/Gabriel/tools/scene-menu/FEATURE.md",
}

def iter_strings(value):
    if isinstance(value, str):
        yield value
        return
    if isinstance(value, dict):
        for child in value.values():
            for item in iter_strings(child):
                yield item
        return
    if isinstance(value, list):
        for child in value:
            for item in iter_strings(child):
                yield item

def main():
    raw = sys.stdin.read()
    if not raw.strip():
        print("{}")
        return

    try:
        payload = json.loads(raw)
    except Exception:
        print("{}")
        return

    normalized = [item.replace("\\", "/") for item in iter_strings(payload)]

    touched = []
    for area_prefix, doc_path in AREA_DOCS.items():
        area_touched = any(area_prefix in item for item in normalized)
        doc_touched = any(doc_path in item for item in normalized)
        if area_touched and not doc_touched:
            touched.append((area_prefix, doc_path))

    if not touched:
        print("{}")
        return

    lines = ["Gabriel feature doc reminder:"]
    for area_prefix, doc_path in touched:
        area_name = area_prefix.rstrip("/").split("/")[-1]
        lines.append(
            "- You edited `{area}`. Update `{doc}` before finishing if behavior, files, load paths, inputs, settings, or dependencies changed.".format(
                area=area_name,
                doc=doc_path,
            )
        )

    print(json.dumps({"additional_context": "\n".join(lines)}))

if __name__ == "__main__":
    main()
