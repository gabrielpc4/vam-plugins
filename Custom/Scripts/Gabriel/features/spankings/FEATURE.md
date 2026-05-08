# Gabriel Spankings Hooks

## Purpose
Minimal blocklist feature for Spankings auto-merge, plus HUD-compiled deferral
helpers sourced from **`features/hands/`**.

## Live Files
- `SpankingsGripBlockPathKeywords.cs`
- `SpankingsAtomsRemoval.cs` *(HitAudioSource / cheek scene atoms when removing Spankings)*
- `spankings_grip_merge_block_path_keywords.txt`
- Deferred first-grip merge source: `features/hands/SpankingsGripDeferredMerge.cs`
  (referenced from `GabrielHud.cslist`).

## Load Path
- `SpankingsGripBlockPathKeywords` and **`SpankingsAtomsRemoval`** compile into
  `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist` (same session bundle
  as `GabrielSessionPlugins.cslist`).
- `SpankingsGripDeferredMerge` lives under `features/hands/` but ships in the same HUD compile unit.
- Queried by `GabrielHud` through `SpankingsGripDeferredMerge`
  and `SpankingsGripBlockPathKeywords` before the deferred first-grip merges run.

## Responsibilities
- `SpankingsAtomsRemoval` deletes Spankings-owned scene atoms when the HUD strips
  Spankings from Persons.
- Parse the block keyword file through VaM file APIs.
- Block the grip-driven Spankings merge when current load/save folders match a configured substring.
- `SpankingsGripDeferredMerge` (see `features/hands/`) applies the
  deferred first-grip merge when the HUD enables it.

## Dependencies And Coupling
- Used with `animation-no-loop-detection` (`AnimationNoLoopDetection` blocks until long
  timelines finish in long non-loop setups) and `GabrielHud` Spankings
  plugin toggles.

## References
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/hands/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`
- `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`
- `Custom/Scripts/Gabriel/features/e-motion/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Keyword file path or parsing behavior changes.
- Grip-merge block semantics or scene matching rules change.
