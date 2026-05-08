# Gabriel E-Motion Hooks

## Purpose
Small hook layer around the external E-Motion plugin families. It decides when Gabriel should auto-merge E-Motion Lite and when a long non-loop mocap should auto-load the default scene.

## Live Files
- `EmotionPathKeywords.cs`
- `emotion_path_keywords.txt`
- `MotionAnimationEmotionEnd.cs`

## Load Path
- Compiled into `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist`.
- Called by `GabrielHud` and `GabrielHudButtons` when auto-merging or blocking emotion behavior.

## Responsibilities
- Parse path-keyword allow lists from `emotion_path_keywords.txt` using VaM file APIs.
- Match current load/save directory strings to decide whether E-Motion Lite should auto-merge.
- Detect long non-loop motion-animation endings and schedule `Saves/scene/Default.json` loads once per scene.

## Dependencies And Coupling
- Depends on external plugin families `E-Motion`, `E-MotionLite`, and `E-MotionFinal` exposed by `GabrielHudButtons`.
- Shares mocap-end blocking behavior with the `spankings` feature.

## References
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/spankings/FEATURE.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Keyword file semantics or path matching changes.
- Mocap-end thresholds or default-scene load conditions change.
- Any external E-Motion family path assumptions change.
