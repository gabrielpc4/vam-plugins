# SceneControlSuite Retain/Remove Matrix

## Purpose

Current-state cutover summary for the SceneControlSuite tree. This file is no longer a
planning scratchpad; it records what is canonical now, what is intentionally
kept outside SceneControlSuite, and which legacy paths still remain for compatibility
with existing scenes or other scripts.

## Documentation

SceneControlSuite keeps **no merged feature index** beyond this file plus **`FEATURE.md` at
`Custom/Scripts/FEATURE.md`** (bootstrap + session bundle overview) and
each area's adjacent **`FEATURE.md`**
(for example `bootstrap/FEATURE.md`, `session-plugins/FEATURE.md`,
`features/ui-hud/FEATURE.md`, and the same pattern under `features/*/`, `tools/*/`).

## Canonical SceneControlSuite Areas

These are the only SceneControlSuite-owned implementation paths that should be extended:

- `Custom/Scripts/bootstrap/**`
- `Custom/Scripts/session-plugins/**`
- `Custom/Scripts/features/ui-hud/**`
- `Custom/Scripts/features/palm-hud/**`
- `Custom/Scripts/features/passenger-possession/**`
- `Custom/Scripts/features/scene-camera/**`
- `Custom/Scripts/features/clothing-interactions/**`
- `Custom/Scripts/features/hands/**`
- `Custom/Scripts/features/head-hide/**`
- `Custom/Scripts/features/e-motion/**`
- `Custom/Scripts/features/animation-no-loop-detection/**`
- `Custom/Scripts/features/spankings/**`
- `Custom/Scripts/features/dildo-on-hands/**`
- `Custom/Scripts/features/improved-pov/**`
- `Custom/Scripts/tools/scene-camera/**`
- `Custom/Scripts/tools/scene-menu/**`

## External Dependencies Still In Use

These are not SceneControlSuite-owned rewrites and can remain where they already live:

- `Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/**`
- `Custom/Scripts/E-MotionLite/**`
- `Custom/Scripts/E-MotionFinal/**`
- `Custom/Scripts/Spankings/**`
- `Custom/Scripts/SexHelper/**`
- `Custom/Scripts/LFE/**`
- `Custom/Scripts/prestigitis_DesktopClothGrab.cs`
- `Custom/Scripts/Easy Background Sounds/**`
- `Custom/Scripts/VAMLaunch/**`
- `Custom/Scripts/Possess Sex/**`
- `Custom/Scripts/Kiss5.cs`
- `Custom/Scripts/ExplosionLimiter-ns.cs`
- `Custom/Scripts/MacGruber/**`
- `Custom/Scripts/VAMDeluxe/**`

## Intentionally Retained Legacy Paths

These old paths still exist because something outside the SceneControlSuite tree still
depends on them:

- `Custom/Scripts/Easy Mate/src/RemoveThisObject.cs`
  Current `Default.json` and `MainMenu.json` still reference it.

## Removed In This Cutover

These old SceneControlSuite-owned paths were deleted and should stay deleted unless a new
real feature replaces them:

- Old Easy Mate custom bootstrap, HUD, palm-hud, passenger, camera, clothing,
  head-hide, e-motion, spankings, and helper source copies.
- Old AutoMate custom loader/session source copies and obsolete settings files.
- `Custom/Scripts/LocalMp4Viewer/**`
- `Custom/Scripts/ImprovedPoV_TongueLicking.cs`
- `Custom/Scripts/features/improved-pov/ImprovedPoV_TongueLicking.cs`
- `Custom/Scripts/AutoMate/SESSION_PLUGINS/ClockSessionPlugin.cs`
- Old `scene_menu_tools/**` root copies after migration into
  `Custom/Scripts/tools/scene-menu/**`
- Old build folders, plugin-builder projects, sample defaults, logs, and other
  one-shot sidecars under Easy Mate / AutoMate.

## Current Follow-Up Rules

- Do not expand scene JSON rewrites beyond the current SceneControlSuite bootstrap/menu
  cutover unless the user explicitly asks for more scene edits.
- If `RemoveThisObject.cs` is retired later, update the dependent scenes first,
  then remove the legacy path.
