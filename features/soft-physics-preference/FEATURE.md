# VaMScripts soft physics scene preference

## Purpose
Keeps VaM **`UserPreferences.softPhysics`** aligned with VaMScripts’s long
non-loop scene policy: disables soft body physics on heavy non-loop timelines
(typical dances), enables it elsewhere. **Exception folders** match the path
tokens used by `AnimationNoLoopDetection` so those scenes stay on.

## Live Files
- `SoftPhysicsScenePreference.cs`

## Load Path
- Compiled in **`SessionPlugins.cslist`** / **`Hud.cslist`**
  with the session bundle.
- **`AnimationNoLoopDetection`** calls `ApplyFromLongNonLoopSceneFlags` once
  per scene whenever scene motion qualification is recomputed — no separate tick.

## Responsibilities
- Translate long non-loop + exception booleans into a single prefs write.

## Dependencies And Coupling
- Input flags are produced exclusively by **`AnimationNoLoopDetection`**
  (`EnsureSceneAnimationState`): same thresholds and exception haystack semantics.
- HUD hotkeys `K` / `L` in `Hotkeys` can still toggle prefs after scene
  init like any VaM shortcut.

## References
- `Custom/Scripts/features/animation-no-loop-detection/FEATURE.md`
- `Custom/Scripts/session-plugins/FEATURE.md`
- `Custom/Scripts/features/ui-hud/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Pref write policy or `UserPreferences` coupling.
- Call site or coupling to animation no-loop flags.
