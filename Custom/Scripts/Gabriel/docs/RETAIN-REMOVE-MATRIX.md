# Gabriel Retain/Remove Matrix

## Purpose

Current-state cutover summary for the Gabriel tree. This file is no longer a
planning scratchpad; it records what is canonical now, what is intentionally
kept outside Gabriel, and which legacy paths still remain for compatibility
with existing scenes or other scripts.

## Documentation

Gabriel keeps **no merged feature index**. Use this file plus each area's adjacent
**`FEATURE.md`** (for example `bootstrap/FEATURE.md`, `session-plugins/FEATURE.md`,
`features/ui-hud/FEATURE.md`, and the same pattern under `features/*/`, `tools/*/`).

## Canonical Gabriel Areas

These are the only Gabriel-owned implementation paths that should be extended:

- `Custom/Scripts/Gabriel/bootstrap/**`
- `Custom/Scripts/Gabriel/session-plugins/**`
- `Custom/Scripts/Gabriel/features/ui-hud/**`
- `Custom/Scripts/Gabriel/features/palm-hud/**`
- `Custom/Scripts/Gabriel/features/passenger-possession/**`
- `Custom/Scripts/Gabriel/features/scene-camera/**`
- `Custom/Scripts/Gabriel/features/clothing-interactions/**`
- `Custom/Scripts/Gabriel/features/head-hide/**`
- `Custom/Scripts/Gabriel/features/e-motion/**`
- `Custom/Scripts/Gabriel/features/animation-no-loop/**`
- `Custom/Scripts/Gabriel/features/spankings/**`
- `Custom/Scripts/Gabriel/features/dildo-on-hands/**`
- `Custom/Scripts/Gabriel/features/improved-pov/**`
- `Custom/Scripts/Gabriel/tools/scene-camera/**`
- `Custom/Scripts/Gabriel/tools/scene-menu/**`

## External Dependencies Still In Use

These are not Gabriel-owned rewrites and can remain where they already live:

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

These old paths still exist because something outside the Gabriel tree still
depends on them:

- `Custom/Scripts/Easy Mate/src/RemoveThisObject.cs`
  Current `Default.json` and `MainMenu.json` still reference it.

## Removed In This Cutover

These old Gabriel-owned paths were deleted and should stay deleted unless a new
real feature replaces them:

- Old Easy Mate custom bootstrap, HUD, palm-hud, passenger, camera, clothing,
  head-hide, e-motion, spankings, and helper source copies.
- Old AutoMate custom loader/session source copies and obsolete settings files.
- `Custom/Scripts/LocalMp4Viewer/**`
- `Custom/Scripts/ImprovedPoV_TongueLicking.cs`
- `Custom/Scripts/Gabriel/features/improved-pov/ImprovedPoV_TongueLicking.cs`
- `Custom/Scripts/AutoMate/SESSION_PLUGINS/ClockSessionPlugin.cs`
- Old `scene_menu_tools/**` root copies after migration into
  `Custom/Scripts/Gabriel/tools/scene-menu/**`
- Old build folders, plugin-builder projects, sample defaults, logs, and other
  one-shot sidecars under Easy Mate / AutoMate.

## Current Follow-Up Rules

- Do not expand scene JSON rewrites beyond the current Gabriel bootstrap/menu
  cutover unless the user explicitly asks for more scene edits.
- If `RemoveThisObject.cs` is retired later, update the dependent scenes first,
  then remove the legacy path.
