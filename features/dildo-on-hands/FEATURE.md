# SceneControlSuite Dildo On Hands

## Purpose
VR toy spawn utility loaded as a session plugin. It spawns or clones a toy at the right hand from a curated catalog and cleans up spawned toys on scene-load edges.

## Live Files
- `DildoOnHands.cs`
- `DildoOnHands.cslist` *(optional standalone compile for isolated testing only).*
- `handspawn_toy_atoms.json`

## Load Path
- Defaults load with **`SessionPlugins.cslist`** (SceneControlSuite bootstrap also
  inserts log clipboard). `DildoOnHands.cs` is **listed inside**
  `SessionPlugins.cslist`; **`DildoOnHands.cslist`** remains for
  toy-only standalone testing and now also includes
  `../../util/PersonAtomCache.cs` for the shared possession snapshot.
- Uses `handspawn_toy_atoms.json` as the default catalog scene extracted beside the plugin.

## Responsibilities
- Listen for right-thumbstick-click spawn input and create toys from a catalog or by atom type fallback.
- Apply hand-local offsets, rotation corrections, and material tweaks for spawned toys.
- Remove spawned runtime toys when a new scene load begins.

## Dependencies And Coupling
- This feature is mostly self-contained, but bootstrap load order and scene-load cleanup assumptions matter.

## References
- `Custom/Scripts/FEATURE.md`
- `Custom/Scripts/bootstrap/FEATURE.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Spawn input bindings, catalog path, or cleanup rules change.
- Any offset, tip-correction, or clone-vs-add-atom behavior changes.
