# Group pivot handle (manual plugin)

## Purpose

Optional **`MVRScript`** to move several atoms together by parenting them to a
spawned Cube pivot (`Atom.parentAtom`). **Not part of SceneControlSuite session bootstrap
or `SessionPlugins.cslist`**.

## Live files

- [`MultiAtomPivotHandle.cs`](MultiAtomPivotHandle.cs)
- [`MultiAtomPivotHandle.cslist`](MultiAtomPivotHandle.cslist)

## Load path

Merge **`Custom/Scripts/features/group-pivot-handle/MultiAtomPivotHandle.cslist`**
onto **CoreControl** (or any atom) only when you want the tool. Do **not** add
this path to `SessionPlugins.cslist` unless you intend it to load every
session.

## Responsibilities

- Spawn a small Cube in front of the camera; scale from plugin slider.
- Attach up to four chosen atoms as children of the pivot; detach restores
  previous parents (state stored in plugin JSON for reload).

## References

- [`Custom/Scripts/FEATURE.md`](../FEATURE.md)
- `Reference/VaM-Scripting-Notes.md`

## Update checklist

Update this file when load path, behavior, or file list changes.

## Troubleshooting

- **"spawned Cube not found"** — fixed in-plugin by diffing atom UIDs before/after
  add (do not use `GetTempUID()` with `AddAtomByType`; VaM renames the uid).
  Reload the plugin script if you still see the old message.
