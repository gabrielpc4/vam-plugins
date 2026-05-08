# Gabriel E-Motion Hooks

## Purpose
Small hook layer around the external E-Motion plugin families. It decides when
Gabriel auto-merges E-Motion Lite from **path keywords** (`emotion_path_keywords.txt`).

## Live Files
- `EmotionPathKeywords.cs`
- `emotion_path_keywords.txt`
- `EmotionPathRuleMerge.cs`

## Load Path
- Compiled in **`GabrielSessionPlugins.cslist`** (same bundle as `GabrielHud`).
- Invoked by `GabrielHud` on scene settle, atom UID churn, and HUD button
  merges.

## Responsibilities
- Parse path-keyword allow lists from `emotion_path_keywords.txt` using VaM file APIs.
- Match current load/save directory strings to decide whether E-Motion Lite auto-merges.
- Schedule deferred merges when Persons appear after qualifying path matches.

## Dependencies And Coupling
- Depends on external plugin families `E-Motion`, `E-MotionLite`, and `E-MotionFinal`
  exposed by `GabrielHud`.
- Shares long non-loop **scene-motion** qualifiers with Spankings / clothing grip merges
  through `features/animation-no-loop-detection` (`AnimationNoLoopDetection`), not via this hook.

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/spankings/FEATURE.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Keyword file semantics or path matching change.
- Path-rule deferred merge staging/timing rules change.
- Any external E-Motion family path assumptions change.
